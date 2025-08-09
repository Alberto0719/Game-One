using UnityEngine;

public class RetreatBehavior : IBehavior
{
    private AgentSoldier agent;
    private MovementController movementController;
    private float retreatSpeed;
    private float retreatDuration;
    
    private bool isRetreating = false;
    private Vector2 retreatDirection;
    private float retreatStartTime;
    
    public RetreatBehavior(AgentSoldier agent, MovementController movementController, float retreatSpeed, float retreatDuration)
    {
        this.agent = agent;
        this.movementController = movementController;
        this.retreatSpeed = retreatSpeed;
        this.retreatDuration = retreatDuration;
    }
    
    public BehaviorResult Execute()
    {
        Transform target = agent.GetTarget();
        if (target == null) return BehaviorResult.Failure;
        
        // Only retreat if we recently attacked
        if (agent.CanAttack()) return BehaviorResult.Failure;
        
        if (!isRetreating)
        {
            // Start retreating
            isRetreating = true;
            retreatStartTime = Time.time;
            
            // Choose a random retreat direction (away from player with some randomness)
            Vector2 awayFromPlayer = (agent.transform.position - target.position).normalized;
            float randomAngle = Random.Range(-45f, 45f) * Mathf.Deg2Rad;
            retreatDirection = new Vector2(
                awayFromPlayer.x * Mathf.Cos(randomAngle) - awayFromPlayer.y * Mathf.Sin(randomAngle),
                awayFromPlayer.x * Mathf.Sin(randomAngle) + awayFromPlayer.y * Mathf.Cos(randomAngle)
            ).normalized;
            
            // Start retreat movement in calculated direction
            movementController.MoveInDirection(retreatDirection, retreatDuration);
        }
        
        float retreatProgress = (Time.time - retreatStartTime) / retreatDuration;
        
        if (retreatProgress < 1f)
        {
            // Continue retreating
            movementController.MoveSpeed = retreatSpeed;
            return BehaviorResult.Continue;
        }
        else
        {
            // Retreat complete
            isRetreating = false;
            movementController.MoveSpeed = agent.moveSpeed; // Reset to normal speed
            return BehaviorResult.Success;
        }
    }
}