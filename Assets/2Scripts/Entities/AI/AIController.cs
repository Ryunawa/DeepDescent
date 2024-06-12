using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace _2Scripts.Entities.AI
{
    public class AIController : NetworkBehaviour, IController
    {
        [Header("Movement Settings")]
        [SerializeField]
        private float distanceAcceptation = 2f; // distance where we can considere the IA managed to go to his goal
        [SerializeField]
        private float startWaitTime = 4; // wait time before the AI starts moving between each action
        [SerializeField]
        private float timeToRotate = 2; // time it takes for the AI to rotate its direction while patrolling or chasing
        [SerializeField]
        private float speedWalk = 3; // speed when patrolling
        [SerializeField]
        private float speedRun = 7; // speed when chasing

        [Header("Detection Settings")]
        [SerializeField]
        private float viewDistance = 15; // radius of detection
        [SerializeField]
        private float viewAngle = 90; // angle of detection
        [SerializeField]
        private LayerMask playerMask;
        [SerializeField]
        private LayerMask obstacleMask;

        [Header("Combat Settings")]
        [SerializeField]
        private float attackRange = 1.5f;
        [SerializeField]
        private float timeBeforeAttack = 2f; // attack speed in seconds
        [SerializeField]
        private bool _isSwinging;
        public bool isBoss;


        [Header("Patrol")]
        [SerializeField] private Transform[] waypoints; // patrol points
        private int m_CurrentWaypointIndex;
        [SerializeField][Range(0f, 1f)] private float stopChance = 0.5f; // percentage of chance to stop once at the destination
        private bool isWaiting = false; // is the ia waiting before patrolling again?
        private bool didAlreadyWait = false; // did it already wait once at its patrol point?

        // Components
        private NavMeshAgent navMeshAgent;
        private Animator animator;
        private HealthComponent healthComponent;

        // Other variables...
        private Vector3 playerLastPosition = Vector3.zero;
        private Vector3 m_PlayerPosition;
        private float m_WaitTime;
        private float m_TimeToRotate;
        private bool m_PlayerDetected; // player has been seeing
        private bool m_PlayerNear; // player close to the AI
        private bool m_IsPatrol;
        private bool m_CanAttackPlayer;


        void Start()
        {
            // init var
            m_IsPatrol = true;
            m_CanAttackPlayer = false;
            m_PlayerDetected = false;

            m_PlayerPosition = Vector3.zero;
            m_WaitTime = startWaitTime;
            m_TimeToRotate = timeToRotate;
            m_CurrentWaypointIndex = 0;

            animator = GetComponent<Animator>();

            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.stoppingDistance = attackRange - 0.5f;
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = speedWalk;
            if(waypoints.Length > 0) navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);

            healthComponent = GetComponent<HealthComponent>();
            healthComponent.OnDeath.AddListener(HandleDeath);

            // start attack coroutine
            StartCoroutine(AttackLoop());
        }

        void Update()
        {
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
                FaceTarget();
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
                    navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
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

    
        // patrolling points by points
        private void PatrollingPointsByPoints()
        {
            // if player is near, move to the last position known of him
            if (m_PlayerNear)
            {
                if (m_TimeToRotate <= 0)
                {
                    ActivateMovements(speedWalk);
                    MoveToPlayer(playerLastPosition);
                }
                else
                {
                    Stop();
                    m_TimeToRotate -= Time.deltaTime;
                }
            }
            // player not near, just move waypoints by waypoints
            else
            {
                m_PlayerNear = false;
                playerLastPosition = Vector3.zero;
                navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);

                if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    if(m_WaitTime <= 0)
                    {
                        // move to the next waypoint
                        NextPoint();
                        ActivateMovements(speedWalk);
                        m_WaitTime = startWaitTime;
                    }
                    else
                    {
                        // wait before moving
                        Stop();
                        m_WaitTime -= Time.deltaTime;
                    }
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

        private void Patrolling()
        {
            // if ia close to its destination and finished his journey
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < distanceAcceptation)
            {
                if(!didAlreadyWait)
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

        private IEnumerator StartWaiting(float waitTime)
        {
            // Debug.LogWarning("waiting... " +  waitTime);
            isWaiting = true;
            yield return new WaitForSeconds(waitTime);
            isWaiting = false;
        }


        private Vector3 GetRandomAccessiblePoint()
        {
            int[] distances = { 30, 20, 10, 5 }; // distances to try to find a point
            bool pointFound = false;

            foreach (int distance in distances)
            {
                int failedAttempts = 0;

                for (int i = 0; i < 2; i++) // test 2 times for each distances
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
                            pointFound = true;
                            return randomPoint; // return the point if has been found
                        }
                    }
                }
            }
            // no valid point
            if (!pointFound)
            {
                //Debug.LogWarning("No points found in all distances, just gonna wait then...");
            }

            return Vector3.zero;
        }


        public event Action<bool> OnSwingStateChanged;

        // attack the player
        private void Attack()
        {
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
            navMeshAgent.speed = speed ;
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
            m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
            navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
        }

        // move towards the player
        private void MoveToPlayer(Vector3 player)
        {
            navMeshAgent.SetDestination(player);
            if(Vector3.Distance(transform.position, player) <= 0.3)
            {
                if(m_WaitTime <= 0)
                {
                    m_PlayerNear = false;
                    ActivateMovements(speedWalk);
                    navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
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
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
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
            Collider[] playersInRange = Physics.OverlapSphere(transform.position, viewDistance, playerMask);

            for(int i = 0; i < playersInRange.Length; i++)
            {
                Transform player = playersInRange[i].transform;
                Vector3 dirToPlayer = (player.position - transform.position).normalized;

                float dstToPlayer = Vector3.Distance(transform.position, player.position);

                // if close to the player, attack!
                if (dstToPlayer <= attackRange)
                {
                    LookAt(player);
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
                if(Vector3.Distance(transform.position, player.position) > viewAngle)
                {
                    m_PlayerDetected = false;
                }

                // detected : player becomes the goal
                if (m_PlayerDetected)
                {
                    m_PlayerPosition = player.transform.position;
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
            if (isBoss)
            {
                GameFlowManager.Instance.SetGameState(GameFlowManager.GameState.BossDefeated);
            }

            // potential reward?

            //Unsubscribe first
            healthComponent.OnDeath.RemoveAllListeners();

            //Despawn
            DespawnNetworkObjectRpc();
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void DespawnNetworkObjectRpc()
        {
            NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();

            networkObject.Despawn(true);
        }

    }
}
