using Unity.VisualScripting;
using UnityEngine;

public class CloseBehavior : IBehavior
{
    private IAgent agent;
    private MovementController movement;

    public CloseBehavior(IAgent agent, MovementController movement)
    {
        this.agent = agent;
        this.movement = movement;
    }
    public BehaviorResult Execute()
    {
        if (agent.GetTarget() == null)
            return BehaviorResult.Failure;

        // Always face the target while closing
        agent.SetFacing(agent.GetTarget().position);

        if (!movement.CanReachDestination(agent.GetTarget().position))
            return BehaviorResult.Failure;

        if (!movement.IsFollowingPath() ||
            Vector3.Distance(movement.transform.position, agent.GetTarget().position) > 1f)
        {
            movement.MoveToTarget(agent.GetTarget().position);
        }

        return BehaviorResult.Success;
    }
}