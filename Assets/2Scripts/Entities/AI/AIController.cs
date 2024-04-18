using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements.Experimental;

public class AIController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField]
    private float startWaitTime = 4; // wait time before the ai starts moving between each action
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

    [Header("Waypoints")]
    [SerializeField]
    private Transform[] waypoints; // patrol points
    private int m_CurrentWaypointIndex;

    // Components
    private NavMeshAgent navMeshAgent;
    private Animator animator;

    // Other variables...
    private Vector3 playerLastPosition = Vector3.zero;
    private Vector3 m_PlayerPosition;
    private float m_WaitTime;
    private float m_TimeToRotate;
    private bool m_PlayerDetected; // player has been saw
    private bool m_PlayerNear; // player close to the ai
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
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);

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
    private void Patrolling()
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

    // attack the player
    private void Attack()
    {
        animator.SetTrigger("IsAttacking");
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
}
