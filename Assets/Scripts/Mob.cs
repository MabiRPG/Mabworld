using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

class Mob : Actor
{
    private Vector2 origin;
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    [SerializeField]
    private float traversalRadius = 3;
    [SerializeField]
    private float delayBetweenAction;

    private IEnumerator movementCoroutine;

    protected override void Awake()
    {
        base.Awake();

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        transform.rotation = Quaternion.identity;

        origin = transform.position;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (movementCoroutine == null)
        {
            Vector3 nextPos = origin + UnityEngine.Random.insideUnitCircle * traversalRadius;
            nextPos.z = 0;

            if (navMeshAgent.SetDestination(nextPos))
            {
                movementCoroutine = Move(); 
                StartCoroutine(movementCoroutine);
            }
        }
    }

    private IEnumerator Move()
    {
        while (navMeshAgent.pathPending)
        {
            yield return null;
        }

        if (!navMeshAgent.hasPath)
        {
            movementCoroutine = null;
            yield break;
        }

        animator.SetBool("isMoving", true);

        while (navMeshAgent.hasPath)
        {
            Vector2 nextPos = navMeshAgent.nextPosition;
            Vector2 diff = nextPos - (Vector2)transform.position;
            transform.position = nextPos;
            animator.SetFloat("moveX", diff.x);
            animator.SetFloat("moveY", diff.y);

            yield return null;
        }

        animator.SetBool("isMoving", false);
        yield return new WaitForSeconds(UnityEngine.Random.Range(3, 15));
        movementCoroutine = null;
    }
}