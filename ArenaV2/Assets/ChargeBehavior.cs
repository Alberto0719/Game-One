using UnityEngine;

public class ChargeBehavior : IBehavior
{
    private AgentSoldier agent;
    private MovementController movementController;
    
    public ChargeBehavior(AgentSoldier agent, MovementController movementController)
    {
        this.agent = agent;
        this.movementController = movementController;
    }
    
    public BehaviorResult Execute()
    {
        Transform target = agent.GetTarget();
        if (target == null) return BehaviorResult.Failure;
        
        // Face the target
        agent.SetFacing(target.position);
        
        // Move toward the target using pathfinding
        if (!movementController.CanReachDestination(target.position))
            return BehaviorResult.Failure;

        if (!movementController.IsFollowingPath() ||
            Vector3.Distance(movementController.transform.position, target.position) > 1f)
        {
            movementController.MoveToTarget(target.position);
        }
        
        return BehaviorResult.Success;
    }
}