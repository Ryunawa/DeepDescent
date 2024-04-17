using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.AI;
public class AIController : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Animator animator;

    public float startWaitTime = 4;
    public float timeToRotate = 2;
    public float speedWalk = 6;
    public float speedRun = 9;
    public float attackRange = 2;

    public float viewRadius = 15;
    public float viewAngle = 90;
    public LayerMask playerMask;
    public LayerMask obstacleMask;
    public float meshResolution = 1f;
    public int edgeIterations = 4;
    public float edgeDistance = 0.5f;

    public Transform[] waypoints;
    int m_CurrentWaypointIndex;

    Vector3 playerLastPosition = Vector3.zero;
    Vector3 m_PlayerPosition;

    float m_WaitTime;
    float m_TimeToRotate;
    bool m_PlayerInRange;
    bool m_PlayerNear;
    bool m_IsPatrol;
    bool m_CanAttackPlayer;


    void Start()
    {
        // init var
        m_PlayerPosition = Vector3.zero;
        m_IsPatrol = true;
        m_CanAttackPlayer = false;
        m_PlayerInRange = false;
        m_WaitTime = startWaitTime;
        m_TimeToRotate = timeToRotate;
        m_CurrentWaypointIndex = 0;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speedWalk;
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);

        StartCoroutine(AttackLoop());
    }

    // Update is called once per frame
    void Update()
    {
        EnvironmentView();

        if(!m_IsPatrol)
        {
            Chasing();
        }
        else
        {
            Patrolling();
        }
    }

    // Function for chasing the player
    private void Chasing()
    {
        m_PlayerNear = false;
        playerLastPosition = Vector3.zero;

        if (!m_CanAttackPlayer)
        {
            Move(speedRun); // Move character with running speed
            navMeshAgent.SetDestination(m_PlayerPosition);
        }

        if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (m_WaitTime <= 0 && !m_CanAttackPlayer && Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) >= 6f)
            {
                m_IsPatrol = true;
                m_PlayerNear = false;
                Move(speedWalk); // Move character with walking speed

                m_TimeToRotate = timeToRotate;
                m_WaitTime = startWaitTime;
                navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
            }
            else
            {
                if (Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) >= 2.5f)
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                }
            }
        }
    }

    // Function for patrolling
    private void Patrolling()
    {
        if (m_PlayerNear)
        {
            if (m_TimeToRotate <= 0)
            {
                Move(speedWalk);
                LookingPlayer(playerLastPosition);
            }
            else
            {
                Stop();
                m_TimeToRotate -= Time.deltaTime;
            }
        }
        else
        {
            m_PlayerNear = false;
            playerLastPosition = Vector3.zero;
            navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);

            if(navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if(m_WaitTime <= 0)
                {
                    NextPoint();
                    Move(speedWalk);
                    m_WaitTime = startWaitTime;
                }
                else
                {
                    Stop();
                    m_WaitTime -= Time.deltaTime;
                }
            }
        }
    }

    // Function to attack the player
    private void AttackPlayer()
    {
        Stop();
        animator.SetBool("isAttacking", true);
        Debug.Log("ATTACKING");

        // DO IT ONE TIME - you have to repeat it after x secondes (maybe by doing 2 animations (idle attack -> when finish punch)
    }

    // Function to move the character with a given speed
    private void Move(float speed)
    {
        if(speed == speedRun)
        {
            animator.SetBool("isPatrolling", false);
            animator.SetBool("isChasing", true);
        }
        else
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isPatrolling", true);
        }

        navMeshAgent.isStopped = false;
        navMeshAgent.speed = speed ;
    }

    // Function to stop the character
    private void Stop()
    {
        PlayIdle();
        navMeshAgent.isStopped = true;
        navMeshAgent.speed = 0;
    }

    // stop animations to play the idle one
    private void PlayIdle()
    {
        animator.SetBool("isChasing", false);
        animator.SetBool("isPatrolling", false);
    }

    // Function to move to the next waypoint
    public void NextPoint()
    {
        m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
    }

    // Function to look towards the player
    private void LookingPlayer(Vector3 player)
    {
        navMeshAgent.SetDestination(player);
        if(Vector3.Distance(transform.position, player) <= 0.3)
        {
            if(m_WaitTime <= 0)
            {
                m_PlayerNear = false;
                Move(speedWalk);
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

    // Function to detect player within view
    private void EnvironmentView()
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, viewRadius, playerMask);

        for(int i = 0; i < playersInRange.Length; i++)
        {
            Transform player = playersInRange[i].transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            float dstToPlayer = Vector3.Distance(transform.position, player.position);

            // if close to the player, attack!
            if (dstToPlayer < attackRange)
            {
                m_CanAttackPlayer = true;
                break;
            }
            else
            {
                m_CanAttackPlayer = false;
                animator.SetBool("isAttacking", false);
            }

            if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
            {
                if (!Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleMask))
                {
                    m_PlayerInRange = true;
                    m_IsPatrol = false;
                }
                else
                {
                    m_PlayerInRange = false;
                }
            }

            if(Vector3.Distance(transform.position, player.position) > viewAngle)
            {
                m_PlayerInRange = false;
            }

            if (m_PlayerInRange)
            {
                m_PlayerPosition = player.transform.position;
            }
        }
    }

    IEnumerator AttackLoop()
    {
        while (true)
        {
            if (m_CanAttackPlayer)
            {
                AttackPlayer();
                yield return new WaitForSeconds(2f);
            }
            else
            {
                yield return null;
            }
        }
    }
}
