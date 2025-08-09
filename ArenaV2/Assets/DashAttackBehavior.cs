using UnityEngine;

public class DashAttackBehavior : IBehavior
{
    private AgentSoldier agent;
    private MovementController movementController;
    private float dashSpeed;
    private float dashRange;
    private float dashDuration;
    
    private bool isDashing = false;
    private Vector2 dashDirection;
    private float dashStartTime;
    private Vector2 startPosition;
    
    public DashAttackBehavior(AgentSoldier agent, MovementController movementController, float dashSpeed, float dashRange, float dashDuration)
    {
        this.agent = agent;
        this.movementController = movementController;
        this.dashSpeed = dashSpeed;
        this.dashRange = dashRange;
        this.dashDuration = dashDuration;
    }
    
    public BehaviorResult Execute()
    {
        Transform target = agent.GetTarget();
        if (target == null) return BehaviorResult.Failure;
        
        if (!isDashing)
        {
            // Start the dash
            isDashing = true;
            dashStartTime = Time.time;
            startPosition = agent.transform.position;
            dashDirection = (target.position - agent.transform.position).normalized;
            
            // Set attack cooldown
            agent.SetLastAttackTime();
            
            // Face the target
            agent.SetFacing(target.position);
            
            // Start dash movement toward target
            movementController.MoveTowardsPoint(target.position, dashDuration);
        }
        
        float dashProgress = (Time.time - dashStartTime) / dashDuration;
        
        if (dashProgress < 1f)
        {
            // Continue dashing
            movementController.MoveSpeed = dashSpeed;
            return BehaviorResult.Continue;
        }
        else
        {
            // Dash complete
            isDashing = false;
            movementController.MoveSpeed = agent.moveSpeed; // Reset to normal speed
            
            // Deal damage to nearby enemies
            DealDashDamage();
            
            return BehaviorResult.Success;
        }
    }
    
    private void DealDashDamage()
    {
        // Find all colliders within dash range
        Collider2D[] hits = Physics2D.OverlapCircleAll(agent.transform.position, 1f);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Damageable playerDamageable = hit.GetComponent<Damageable>();
                if (playerDamageable != null)
                {
                    playerDamageable.TakeDamage(1f); // Adjust damage as needed
                }
            }
        }
    }
}