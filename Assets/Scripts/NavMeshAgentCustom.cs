using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgentCustom : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    [SerializeField]
    private Transform target;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    private void Start()
    {
        NavMeshPath path = new NavMeshPath();
        navMeshAgent.CalculatePath(target.position, path);
        navMeshAgent.SetPath(path);
    }
}
