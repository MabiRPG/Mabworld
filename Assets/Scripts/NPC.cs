using UnityEngine;
using UnityEngine.AI;

class NPC : Actor
{
    private NavMeshAgent navMeshAgent;

    protected override void Awake()
    {
        base.Awake();

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        transform.rotation = new Quaternion(0, 0, 0, 0);
    }
}