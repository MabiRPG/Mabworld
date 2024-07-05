using System;
using System.Collections;
using UnityEngine;

class Mob : Actor
{
    private Vector2 origin;

    [SerializeField]
    private float traversalRadius = 3;
    [SerializeField]
    private float minimumIdleTime = 3;
    [SerializeField]
    private float maximumIdleTime = 15;

    protected override void Awake()
    {
        base.Awake();
        origin = transform.position;
    }

    private void Update()
    {
        if (state != State.Idle)
        {
            return;
        }

        if (!navMeshAgent.pathPending && !navMeshAgent.hasPath)
        {
            Vector3 target = origin + UnityEngine.Random.insideUnitCircle * traversalRadius;
            target.z = 0;
            navMeshAgent.SetDestination(target);
        }
        else if (navMeshAgent.hasPath)
        {
            actorCoroutine = Move();
            StartCoroutine(actorCoroutine);
        }
    }

    private IEnumerator Move()
    {
        state = State.Moving;
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
        transform.position = new Vector3(navMeshAgent.destination.x, navMeshAgent.destination.y, 0f);
        yield return new WaitForSeconds(UnityEngine.Random.Range(minimumIdleTime, maximumIdleTime));
        moveEvent.RaiseOnChange();
    }
}