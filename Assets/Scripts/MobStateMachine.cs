using System.Collections;
using UnityEngine;

/// <summary>
///     Mob state machine to control movement
/// </summary>
public class MobMovementMachine : MovementStateMachine
{
}

/// <summary>
///     Mob controller to manage all state machines
/// </summary>
public class MobController : MonoBehaviour
{
    public MobMovementMachine movementMachine;
    public Mob mob;
    public IEnumerator Task;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    /// <param name="mob"></param>
    public void Init(Mob mob)
    {
        movementMachine = gameObject.AddComponent<MobMovementMachine>();
        movementMachine.SetActor(mob);
        this.mob = mob;
    }

    /// <summary>
    ///     Called on every frame.
    /// </summary>
    public void Update()
    {
        if (Task != null)
        {
            return;
        }

        // Always wander whenever it is idling.
        Task = Wander();
        StartCoroutine(Task);
    }

    /// <summary>
    ///     Randomly selects a point in the traversal radius around origin, and paths there.
    ///     Once complete, randomly idles between minimum and maximum idle time and resets.
    /// </summary>
    /// <returns></returns>
    public IEnumerator Wander()
    {
        Vector3 position = FindRandomPointInRadius(mob.origin, mob.traversalRadius);
        movementMachine.PathToPosition(position);

        while (!mob.navMeshAgent.hasPath)
        {
            yield return null;
        }

        movementMachine.SetState(new MoveState(movementMachine));

        while (movementMachine.State != movementMachine.DefaultState)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(Random.Range(mob.minimumIdleTime, mob.maximumIdleTime));
        Task = null;
    }

    /// <summary>
    ///     Finds a random point around the origin in an area of size radius.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public Vector3 FindRandomPointInRadius(Vector3 origin, float radius)
    {
        Vector3 target = origin + (Vector3)Random.insideUnitCircle * radius;
        target.z = 0f;
        return target;
    }
}