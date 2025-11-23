using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public NavMeshAgent agent = null;
    public Animator anim = null; 
    public EnemyStats stats = null;

    [SerializeField] public List<Transform> waypoints;
    [SerializeField] private int waypointIndex = 0;


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.SetDestination(waypoints[waypointIndex].position);
    }

    private void Update()
    {
        if (stats.IsDead())
        {
            // handle death
        }
        else
        {
            agent.SetDestination(waypoints[waypointIndex].position);
            anim.SetFloat("Speed", 1f, 0.3f, Time.deltaTime);

            float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[waypointIndex].position);
            if (distanceToWaypoint <= agent.stoppingDistance)
            {
                // check if final destination is reached
                // maybe delete game object? or make hidden
                // if reached final destination, deal damage to player equal to health

                waypointIndex = (waypointIndex + 1) % waypoints.Count;
                agent.SetDestination(waypoints[waypointIndex].position);
            }
        }   
    }
}