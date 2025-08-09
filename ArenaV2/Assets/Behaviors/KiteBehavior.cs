using UnityEngine;

public class KiteBehavior : IBehavior
{
    private bool isExecuting = false;
    private float danceTimer = 0f;
    private Vector2 sideDirection; // Random side direction perpendicular to player
    private float directionChangeTimer = 0f;
    
    private const float DANCE_DURATION = 3f; // Dance for 3 seconds before releasing control
    private const float DIRECTION_CHANGE_INTERVAL = 0.8f; // Change direction every 0.8 seconds
    private const float MIN_KITE_DISTANCE = 2f; // Minimum distance from player
    private const float MAX_KITE_DISTANCE = 5f; // Maximum distance from player
    private const float CAMERA_BOUNDARY_BUFFER = 1f; // Stay this far from camera edge
    
    private IAgent agent;
    private MovementController movement;
    private Camera playerCamera;

    public KiteBehavior(IAgent agent, MovementController movement)
    {
        this.agent = agent;
        this.movement = movement;
        this.playerCamera = Camera.main; // Get the main camera
    }

    public BehaviorResult Execute()
    {
        if (agent.GetTarget() == null)
            return BehaviorResult.Failure;

        // Update timers
        danceTimer += Time.deltaTime;
        directionChangeTimer += Time.deltaTime;

        // Change direction periodically
        if (directionChangeTimer >= DIRECTION_CHANGE_INTERVAL)
        {
            GenerateRandomSideDirection(agent);
            directionChangeTimer = 0f;
        }

        // Always face the target while kiting
        agent.SetFacing(agent.GetTarget().position);

        // Calculate movement based on distance to player
        float distanceToPlayer = Vector2.Distance(agent.transform.position, agent.GetTarget().position);
        Vector2 moveDirection = CalculateKiteMovement(agent, distanceToPlayer);
        
        // Apply camera boundary constraints
        moveDirection = ConstrainToCameraBounds(agent.transform.position, moveDirection);

        // Use directional movement for kiting (allows collision with environment)
        if (movement != null && moveDirection != Vector2.zero)
        {
            Vector2 targetPosition = (Vector2)agent.transform.position + moveDirection * 2f; // Move 2 units in direction
            movement.MoveTowardsPoint(targetPosition, DIRECTION_CHANGE_INTERVAL);
        }

        return BehaviorResult.Success; // Release control back to priority system
    }
    
    private Vector2 ConstrainToCameraBounds(Vector3 currentPosition, Vector2 moveDirection)
    {
        if (playerCamera == null) return moveDirection;
        
        // Get camera bounds in world coordinates
        float cameraHeight = playerCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * playerCamera.aspect;
        
        Vector3 cameraPosition = playerCamera.transform.position;
        float leftBound = cameraPosition.x - cameraWidth/2f + CAMERA_BOUNDARY_BUFFER;
        float rightBound = cameraPosition.x + cameraWidth/2f - CAMERA_BOUNDARY_BUFFER;
        float bottomBound = cameraPosition.y - cameraHeight/2f + CAMERA_BOUNDARY_BUFFER;
        float topBound = cameraPosition.y + cameraHeight/2f - CAMERA_BOUNDARY_BUFFER;
        
        // Check if the intended movement would go out of bounds
        Vector2 intendedPosition = (Vector2)currentPosition + moveDirection * 2f;
        Vector2 constrainedDirection = moveDirection;
        
        // If moving outside bounds, adjust the direction
        if (intendedPosition.x < leftBound && moveDirection.x < 0)
        {
            constrainedDirection.x = 0; // Stop horizontal movement toward left bound
        }
        else if (intendedPosition.x > rightBound && moveDirection.x > 0)
        {
            constrainedDirection.x = 0; // Stop horizontal movement toward right bound
        }
        
        if (intendedPosition.y < bottomBound && moveDirection.y < 0)
        {
            constrainedDirection.y = 0; // Stop vertical movement toward bottom bound
        }
        else if (intendedPosition.y > topBound && moveDirection.y > 0)
        {
            constrainedDirection.y = 0; // Stop vertical movement toward top bound
        }
        
        // If we're already outside bounds, move back toward center
        if (currentPosition.x < leftBound)
        {
            constrainedDirection.x = Mathf.Max(0, constrainedDirection.x); // Only allow rightward movement
        }
        else if (currentPosition.x > rightBound)
        {
            constrainedDirection.x = Mathf.Min(0, constrainedDirection.x); // Only allow leftward movement
        }
        
        if (currentPosition.y < bottomBound)
        {
            constrainedDirection.y = Mathf.Max(0, constrainedDirection.y); // Only allow upward movement
        }
        else if (currentPosition.y > topBound)
        {
            constrainedDirection.y = Mathf.Min(0, constrainedDirection.y); // Only allow downward movement
        }
        
        return constrainedDirection.normalized;
    }
    
    private void GenerateRandomSideDirection(IAgent target)
    {
        // Get direction to player
        Vector2 toPlayer = (target.player.position - target.transform.position).normalized;
        
        // Get perpendicular direction (90 degrees rotated)
        Vector2 perpendicular = new Vector2(-toPlayer.y, toPlayer.x);
        
        // Randomly choose left or right side
        if (Random.Range(0f, 1f) < 0.5f)
        {
            perpendicular = -perpendicular;
        }
        
        sideDirection = perpendicular;
    }
    
    private Vector2 CalculateKiteMovement(IAgent target, float distanceToPlayer)
    {
        Vector2 toPlayer = (target.player.position - target.transform.position).normalized;
        Vector2 awayFromPlayer = -toPlayer;
        
        // If too close, move away from player
        if (distanceToPlayer < MIN_KITE_DISTANCE)
        {
            return awayFromPlayer;
        }
        // If too far, move toward player
        else if (distanceToPlayer > MAX_KITE_DISTANCE)
        {
            return toPlayer;
        }
        // In ideal range, move side to side
        else
        {
            return sideDirection;
        }
    }
}