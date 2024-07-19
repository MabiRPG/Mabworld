using System.Collections;
using UnityEngine;

public class MobMovementMachine : MovementStateMachine
{
}

public class MobController : MonoBehaviour
{
    public MobMovementMachine movementMachine;
    public Mob mob;
    public IEnumerator Task;

    public void Init(Mob mob)
    {
        movementMachine = gameObject.AddComponent<MobMovementMachine>();
        movementMachine.SetActor(mob);
        this.mob = mob;
    }

    public void Update()
    {
        if (Task != null)
        {
            return;
        }

        Task = Wander();
        StartCoroutine(Task);
    }

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

    public Vector3 FindRandomPointInRadius(Vector3 origin, float radius)
    {
        Vector3 target = origin + (Vector3)Random.insideUnitCircle * radius;
        target.z = 0f;
        return target;
    }
}