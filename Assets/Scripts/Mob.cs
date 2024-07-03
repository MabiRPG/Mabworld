using UnityEngine;
using UnityEngine.AI;

class Mob : Actor
{
    private Vector2 origin;
    private NavMeshAgent navMeshAgent;
    private NavMeshPath path;

    public float traverseRadius = 3;
    public float delayBetweenAction;
    public float idleChance;

    protected override void Awake()
    {
        base.Awake();

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        transform.rotation = Quaternion.identity;

        origin = transform.position;
        path = new NavMeshPath();
    }

    private void Update()
    {
        if (!navMeshAgent.hasPath)
        {
            Vector2 nextPos = origin + Random.insideUnitCircle * traverseRadius;

            if (navMeshAgent.CalculatePath(nextPos, path))
            {
                navMeshAgent.SetPath(path);
            }
        }
    }
}