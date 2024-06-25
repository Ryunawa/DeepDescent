using System;
using System.Collections;
using System.Collections.Generic;
using _2Scripts.Entities.Player;
using _2Scripts.Manager;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.AI;

namespace _2Scripts.Entities.AI
{
    public class AIController : NetworkBehaviour, IController
    {
        [Header("Movement Settings")]
        [SerializeField] private float distanceAcceptation = 2f; // distance where we can consider the IA managed to go to his goal
        [SerializeField] private float startWaitTime = 4; // wait time before the AI starts moving between each action
        [SerializeField] private float timeToRotate = 2; // time it takes for the AI to rotate its direction while patrolling or chasing
        [SerializeField] private float speedWalk = 1; // speed when patrolling
        [SerializeField] private float speedRun = 4; // speed when chasing

        [Header("Detection Settings")]
        [SerializeField] private float viewDistance = 15; // radius of detection
        [SerializeField] private float viewAngle = 90; // angle of detection
        [SerializeField] private LayerMask playerMask;
        [SerializeField] private LayerMask obstacleMask;

        [Header("Combat Settings")]
        [SerializeField] private float attackRange = 0.2f;
        [SerializeField] private float timeBeforeAttack = 2f; // attack speed in seconds
        [SerializeField] private bool _isSwinging;
        [SerializeField] private GameObject deathFxPrefab;
        public bool isBoss;

        [Header("Patrol")]
        [SerializeField] private Transform[] waypoints; // patrol points
        private int m_CurrentWaypointIndex;
        [SerializeField][Range(0f, 1f)] private float stopChance = 0.5f; // percentage of chance to stop once at the destination
        private bool isWaiting = false; // is the ia waiting before patrolling again?
        private bool didAlreadyWait = false; // did it already wait once at its patrol point?

        // Components
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Animator animator;
        [SerializeField] private HealthComponent healthComponent;

        // Other variables...
        private Vector3 playerLastPosition = Vector3.zero;
        private Vector3 m_PlayerPosition;
        private Transform m_PlayerTransform;
        private float m_WaitTime;
        private float m_TimeToRotate;
        private bool m_PlayerDetected; // player has been seen
        private bool m_PlayerNear; // player close to the AI
        private bool m_IsPatrol;
        private bool m_CanAttackPlayer;
        private bool isSpawnAnimationComplete = false;

        void Start()
        {
            if (!IsServer)return;
            // init var
            m_IsPatrol = true;
            m_CanAttackPlayer = false;
            m_PlayerDetected = false;

            m_PlayerPosition = Vector3.zero;
            m_PlayerTransform = null;
            m_WaitTime = startWaitTime;
            m_TimeToRotate = timeToRotate;
            m_CurrentWaypointIndex = 0;

            navMeshAgent.stoppingDistance = attackRange - 0.5f;
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = speedWalk;

            healthComponent.OnDeath.AddListener(HandleDeath);

            // start attack coroutine
            StartCoroutine(AttackLoop());
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void ChangeSkinRpc(int index)
        {
            foreach (Transform child in transform)
            { 
                child.gameObject.SetActive(child.transform.GetSiblingIndex() == index);
            }
        }

        public void OnSpawnAnimationComplete()
        {
            if (!IsServer)return;
            isSpawnAnimationComplete = true;

            if (waypoints != null && waypoints.Length > 0)
            {
                navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
            }

            // play sound
            GameManager.GetManager<AudioManager>().PlaySfx("MonsterSpawn", this, 1, 5);
        }

        void Update()
        {
            if (!IsServer)return;
            
            // wait end animation "Getting Up"
            if (!isSpawnAnimationComplete) return;

            if (_isSwinging)
            {
                Stop();
                return;
            }

            float currentSpeed = navMeshAgent.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);

            EnvironmentView();

            // Patrol mode
            if (m_IsPatrol)
            {
                Patrolling();
            }
            // Chase mode
            else
            {
                Chasing();
            }
        }

        // run after the player or its last known position
        private void Chasing()
        {
            m_PlayerNear = false;
            playerLastPosition = Vector3.zero;

            // Run after the player !
            if (!m_CanAttackPlayer)
            {
                ActivateMovements(speedRun);
                navMeshAgent.SetDestination(m_PlayerPosition);
                FaceTarget();
            }

            // Check if the player is close enough to stop chasing
            if (m_PlayerDetected && Vector3.Distance(transform.position, m_PlayerPosition) <= attackRange)
            {
                // Stop moving towards the player
                Stop();
                // Set the destination to the current position to prevent further movement
                navMeshAgent.SetDestination(transform.position);
            }

            // The AI arrived at the last known position
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                // lost the player for too long
                if (m_WaitTime <= 0 && !m_CanAttackPlayer && Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) > attackRange)
                {
                    // Get back to patrol
                    m_IsPatrol = true;
                    m_PlayerNear = false;

                    ActivateMovements(speedWalk);
                    m_TimeToRotate = timeToRotate;
                    m_WaitTime = startWaitTime;

                    if (waypoints != null && waypoints.Length > 0)
                    {
                        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
                    }
                    else
                    {
                        Debug.LogWarning("Waypoints array is either null or empty.");
                    }
                }
                else
                {
                    // Just lost the player
                    if (Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) >= 2.5f)
                    {
                        // Stop moving and wait
                        Stop();
                        m_WaitTime -= Time.deltaTime;
                    }
                }
            }
        }

        private void Patrolling()
        {
            // if ia close to its destination and finished his journey
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < distanceAcceptation)
            {
                if (!didAlreadyWait)
                {
                    float percentage = UnityEngine.Random.value;
                    if (percentage <= stopChance)
                    {
                        didAlreadyWait = true;
                        float waitTime = UnityEngine.Random.Range(2f, 5f);
                        StartCoroutine(StartWaiting(waitTime));
                    }
                    else
                    {
                        NewRandomPointPatrolling();
                    }
                }
                else if (!isWaiting)
                {
                    NewRandomPointPatrolling();
                }
            }
        }

        private void NewRandomPointPatrolling()
        {
            Vector3 randomPoint = GetRandomAccessiblePoint();

            // if a point is valid -> become destination
            if (randomPoint != Vector3.zero)
            {
                navMeshAgent.SetDestination(randomPoint);
                didAlreadyWait = false;
            }
        }


        private IEnumerator StartWaiting(float waitTime)
        {
            isWaiting = true;
            yield return new WaitForSeconds(waitTime);
            isWaiting = false;
        }

        private Vector3 GetRandomAccessiblePoint()
        {
            int[] distances = { 30, 20, 10, 5 }; // distances to try to find a point

            foreach (int distance in distances)
            {
                int failedAttempts = 0;

                for (int i = 0; i < 2; i++) // test 2 times for each distance
                {
                    Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
                    randomDirection += transform.position;
                    NavMeshHit hit;
                    Vector3 randomPoint = Vector3.zero;

                    if (NavMesh.SamplePosition(randomDirection, out hit, distance, NavMesh.AllAreas))
                    {
                        randomPoint = hit.position;

                        // is the point accessible? by trying to raycast it
                        RaycastHit raycastHit;
                        if (Physics.Raycast(transform.position, randomPoint - transform.position, out raycastHit, distance))
                        {
                            // raycast collides with an obstacle
                            if (raycastHit.collider.gameObject.layer == obstacleMask)
                            {
                                randomPoint = Vector3.zero;
                                failedAttempts++;
                                continue;
                            }
                        }
                        else
                        {
                            return randomPoint; // return the point if has been found
                        }
                    }
                }
            }
            return Vector3.zero;
        }

        public event Action<bool> OnSwingStateChanged;

        // attack the player
        private void Attack()
        {
            LookAt(m_PlayerTransform);
            animator.SetTrigger("IsAttacking");
        }

        public void StartSwing()
        {
            _isSwinging = true;
            OnSwingStateChanged?.Invoke(_isSwinging);
        }

        public void EndSwing()
        {
            _isSwinging = false;
            OnSwingStateChanged?.Invoke(_isSwinging);
        }

        // set the character state to "move" with a given speed
        private void ActivateMovements(float speed)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = speed;
        }

        // stop the character
        private void Stop()
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.speed = 0;
        }

        // move to the next waypoint
        public void NextPoint()
        {
            if (waypoints != null && waypoints.Length > 0)
            {
                m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
                navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
            }
            else
            {
                Debug.LogWarning("Waypoints array is either null or empty.");
            }
        }

        // move towards the player
        private void MoveToPlayer(Vector3 player)
        {
            navMeshAgent.SetDestination(player);
            if (Vector3.Distance(transform.position, player) <= 0.3)
            {
                if (m_WaitTime <= 0)
                {
                    m_PlayerNear = false;
                    ActivateMovements(speedWalk);

                    if (waypoints != null && waypoints.Length > 0)
                    {
                        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
                    }
                    else
                    {
                        Debug.LogWarning("Waypoints array is either null or empty.");
                    }

                    m_WaitTime = startWaitTime;
                    m_TimeToRotate = timeToRotate;
                }
                else
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                }
            }
        }

        // look at a transform
        private void LookAt(Transform target)
        {
            if (!this || !target) return;

            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion newRotation = Quaternion.LookRotation(direction);
                transform.rotation = newRotation;
            }
        }

        // face move goal, rotation is way more natural and faster
        void FaceTarget()
        {
            var turnTowardNavSteeringTarget = navMeshAgent.steeringTarget;

            if (turnTowardNavSteeringTarget != transform.position)
            {
                Vector3 direction = (turnTowardNavSteeringTarget - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
            }
        }

        // detect player within view
        private void EnvironmentView()
        {
            Collider[] alivePlayersInRange = findAlivePlayerInRange();

            for (int i = 0; i < alivePlayersInRange.Length; i++)
            {
                Transform player = alivePlayersInRange[i].transform;
                Vector3 dirToPlayer = (player.position - transform.position).normalized;

                float dstToPlayer = Vector3.Distance(transform.position, player.position);

                // if close to the player, attack!
                if (dstToPlayer <= attackRange)
                {
                    // LookAt(player);
                    FaceTarget();
                    m_CanAttackPlayer = true;
                    Stop();
                    animator.SetBool("IsPreparingToAttack", true);
                    break;
                }
                else
                {
                    m_CanAttackPlayer = false;
                    animator.SetBool("IsPreparingToAttack", false);
                }

                if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
                {
                    // player is seen
                    if (!Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleMask))
                    {
                        m_PlayerDetected = true;
                        m_IsPatrol = false;
                    }
                    else
                    {
                        m_PlayerDetected = false;
                    }
                }

                // player too far
                if (Vector3.Distance(transform.position, player.position) > viewAngle)
                {
                    m_PlayerDetected = false;
                }

                // detected : player becomes the goal
                if (m_PlayerDetected)
                {
                    m_PlayerPosition = player.transform.position;
                    m_PlayerTransform = player.transform;
                }
            }

            // if no player in sight -> patrol
            if (alivePlayersInRange.Length <= 0)
            {
                m_IsPatrol = true;
                m_CanAttackPlayer = false;
                m_PlayerDetected = false;
                animator.SetBool("IsPreparingToAttack", false);
                if (waypoints != null && waypoints.Length > 0)
                {
                    ActivateMovements(speedWalk);
                    navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
                }
            }
        }

        // attack trigger - coroutine
        IEnumerator AttackLoop()
        {
            while (true)
            {
                if (m_CanAttackPlayer)
                {
                    Attack();
                    yield return new WaitForSeconds(timeBeforeAttack);
                }
                else
                {
                    yield return null;
                }
            }
        }

        // Death function
        private void HandleDeath()
        {
            // play sound
            GameManager.GetManager<AudioManager>().PlaySfx("MonsterDie", this, 1, 5);

            if (isBoss)
            {
                // play sound
                GameManager.GetManager<AudioManager>().PlaySfx("BossSlain");
                GameManager.GetManager<GameFlowManager>().SetGameState(GameFlowManager.LevelState.BossDefeated);
            }

            if (deathFxPrefab != null)
            {
                GameObject deathFx = Instantiate(deathFxPrefab, transform.position + Vector3.up, Quaternion.identity);
                deathFx.GetComponent<NetworkObject>().Spawn();
            }

            // potential reward?

            //Unsubscribe first
            healthComponent.OnDeath.RemoveAllListeners();

            //Despawn
            DespawnNetworkObjectRpc();
        }


        private Collider[] findAlivePlayerInRange()
        {
            Collider[] playersInRange = Physics.OverlapSphere(transform.position, viewDistance, playerMask);
            Collider[] alivePlayersInRange = new Collider[playersInRange.Length];
            int aliveCount = 0;

            foreach (var playerCollider in playersInRange)
            {
                PlayerBehaviour playerBehavior = playerCollider.GetComponent<PlayerBehaviour>();
                if (playerBehavior != null && !playerBehavior.IsDead.Value)
                {
                    alivePlayersInRange[aliveCount] = playerCollider;
                    aliveCount++;
                }
            }
            Array.Resize(ref alivePlayersInRange, aliveCount);

            return alivePlayersInRange;
        }

        // Switch to chase mode when attacked
        public void SwitchToChaseMode(Transform attacker)
        {
            m_PlayerDetected = true;
            m_IsPatrol = false;
            m_PlayerPosition = attacker.position;
            m_PlayerTransform = attacker;
            ActivateMovements(speedRun);
            navMeshAgent.SetDestination(m_PlayerPosition);
        }


        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void DespawnNetworkObjectRpc()
        {
            NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();

            networkObject.Despawn(true);
        }
    }
}


