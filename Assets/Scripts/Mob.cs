using System;
using System.Collections;
using UnityEngine;

/// <summary>
///     Handles all mob (non-NPC and non-Player) processing.
/// </summary>
class Mob : Actor
{
    private Vector2 origin;

    // Traversal radius is the maximum range in world units that 
    // a mob can wander around its origin.
    [SerializeField]
    private float traversalRadius = 3;
    // Minimum and maximum idle times after a movement is complete to delay movement again.
    [SerializeField]
    private float minimumIdleTime = 3;
    [SerializeField]
    private float maximumIdleTime = 15;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        origin = transform.position;
    }

    /// <summary>
    ///     Called on every frame.
    /// </summary>
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
            // SetDestination is async call so hasPath conditional=true after a few frames.
            navMeshAgent.SetDestination(target);
        }
        // Once SetDestination is done, we call the path here.
        else if (navMeshAgent.hasPath)
        {
            actorCoroutine = Move();
            StartCoroutine(actorCoroutine);
        }
    }

    /// <summary>
    ///     Moves the NavMeshAgent according to the preset path, and changes the animator states 
    ///     accordingly.
    /// </summary>
    /// <returns>Coroutine to be run.</returns>
    private IEnumerator Move()
    {
        state = State.Moving;
        animator.SetBool("isMoving", true);

        while (navMeshAgent.hasPath)
        {
            Vector2 nextPos = navMeshAgent.nextPosition;
            Vector2 diff = nextPos - (Vector2)transform.position;
            transform.position = nextPos;
            // Set the animator to the relative movement vector
            animator.SetFloat("moveX", diff.x);
            animator.SetFloat("moveY", diff.y);

            yield return null;
        }

        animator.SetBool("isMoving", false);
        // Set the final position exactly to the destination, with the z=0 
        // due to some bug that causes it to be non-zero.
        transform.position = new Vector3(navMeshAgent.destination.x, navMeshAgent.destination.y, 0f);
        yield return new WaitForSeconds(UnityEngine.Random.Range(minimumIdleTime, maximumIdleTime));
        moveEvent.RaiseOnChange();
    }
}