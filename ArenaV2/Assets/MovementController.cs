using UnityEngine;
using UnityEngine.AI;

public class MovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float waypointThreshold = 0.2f;
    [SerializeField] private float pathUpdateInterval = 0.1f;
    [SerializeField] private float directionalMovementDecay = 2f;
    
    private NavMeshAgent agent;
    private Rigidbody2D rb;
    private Vector2 currentVelocity = Vector2.zero;
    private float pathUpdateTimer = 0f;
    private int currentWaypointIndex = 0;
    
    // Movement modes
    private enum MovementMode
    {
        Idle,
        PathToTarget,
        DirectionalMovement
    }
    
    private MovementMode currentMode = MovementMode.Idle;
    private Vector3 targetDestination;
    private Vector2 directionalTarget;
    private float directionalMovementTimer = 0f;
    private float directionalMovementDuration = 0f;
    
    // External force management
    private bool isExternalForceActive = false;
    
    // Public properties
    public bool IsMoving => currentVelocity.magnitude > 0.1f;
    public bool HasReachedDestination => currentMode == MovementMode.PathToTarget && 
                                       Vector3.Distance(transform.position, targetDestination) < waypointThreshold;
    public Vector2 CurrentVelocity => currentVelocity;
    public float MoveSpeed 
    { 
        get => moveSpeed; 
        set => moveSpeed = value; 
    }
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody2D>();
        
        if (agent == null)
        {
            Debug.LogError("MovementController requires a NavMeshAgent component!");
            return;
        }
        
        if (rb == null)
        {
            Debug.LogError("MovementController requires a Rigidbody2D component!");
            return;
        }
        
        // Configure NavMeshAgent for 2D pathfinding only
        agent.updatePosition = false; // We'll handle movement ourselves
        agent.updateRotation = false; // We don't want automatic rotation
        agent.updateUpAxis = false;   // For 2D games
        agent.speed = moveSpeed;
    }
    
    void Update()
    {
        UpdateMovement();
    }
    
    void FixedUpdate()
    {
        // Only apply velocity if no external force is active
        if (!isExternalForceActive)
        {
            rb.linearVelocity = currentVelocity;
        }
        // When external force is active, let physics handle velocity
    }
    
    private void UpdateMovement()
    {
        switch (currentMode)
        {
            case MovementMode.Idle:
                currentVelocity = Vector2.zero;
                break;
                
            case MovementMode.PathToTarget:
                UpdatePathMovement();
                break;
                
            case MovementMode.DirectionalMovement:
                UpdateDirectionalMovement();
                break;
        }
    }
    
    private void UpdatePathMovement()
    {
        if (agent == null || !agent.hasPath)
        {
            currentVelocity = Vector2.zero;
            return;
        }
        
        // Update agent position to our actual position periodically
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateInterval)
        {
            agent.nextPosition = transform.position;
            pathUpdateTimer = 0f;
        }
        
        // Check if we've reached the destination
        if (HasReachedDestination)
        {
            currentVelocity = Vector2.zero;
            currentMode = MovementMode.Idle;
            return;
        }
        
        // Get the next waypoint
        Vector3 nextWaypoint = GetNextWaypoint();
        if (nextWaypoint != Vector3.zero)
        {
            // Calculate direction to next waypoint
            Vector2 direction = ((Vector2)(nextWaypoint - transform.position)).normalized;
            currentVelocity = direction * moveSpeed;
        }
        else
        {
            currentVelocity = Vector2.zero;
        }
    }
    
    private void UpdateDirectionalMovement()
    {
        directionalMovementTimer += Time.deltaTime;
        
        if (directionalMovementTimer >= directionalMovementDuration)
        {
            // Directional movement finished
            currentMode = MovementMode.Idle;
            currentVelocity = Vector2.zero;
            return;
        }
        
        // Calculate decay factor (movement gets weaker over time)
        float decayFactor = 1f - (directionalMovementTimer / directionalMovementDuration);
        decayFactor = Mathf.Pow(decayFactor, directionalMovementDecay);
        
        // Apply directional movement
        Vector2 direction = (directionalTarget - (Vector2)transform.position).normalized;
        currentVelocity = direction * moveSpeed * decayFactor;
    }
    
    private Vector3 GetNextWaypoint()
    {
        if (agent == null || !agent.hasPath || agent.path.corners.Length == 0)
            return Vector3.zero;
        
        // Find the next corner we haven't reached yet
        for (int i = currentWaypointIndex; i < agent.path.corners.Length; i++)
        {
            Vector3 corner = agent.path.corners[i];
            float distance = Vector3.Distance(transform.position, corner);
            
            if (distance > waypointThreshold)
            {
                currentWaypointIndex = i;
                return corner;
            }
        }
        
        // If we've reached all waypoints, return the final destination
        if (agent.path.corners.Length > 0)
        {
            return agent.path.corners[agent.path.corners.Length - 1];
        }
        
        return Vector3.zero;
    }
    
    // Public API Methods
    
    /// <summary>
    /// Move towards a target destination using pathfinding
    /// </summary>
    public bool MoveToTarget(Vector3 destination)
    {
        if (agent == null) return false;
        
        targetDestination = destination;
        currentMode = MovementMode.PathToTarget;
        currentWaypointIndex = 0;
        
        // Calculate path using NavMeshAgent
        bool pathFound = agent.SetDestination(destination);
        
        if (!pathFound)
        {
            Debug.LogWarning($"No path found to destination: {destination}");
            currentMode = MovementMode.Idle;
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Move in a specific direction for a duration (e.g., dash, retreat)
    /// This movement may be stopped by collisions
    /// </summary>
    public void MoveInDirection(Vector2 direction, float duration)
    {
        currentMode = MovementMode.DirectionalMovement;
        directionalTarget = (Vector2)transform.position + direction.normalized * moveSpeed * duration;
        directionalMovementTimer = 0f;
        directionalMovementDuration = duration;
    }
    
    /// <summary>
    /// Move towards a specific point in a straight line for a duration
    /// </summary>
    public void MoveTowardsPoint(Vector2 point, float duration)
    {
        currentMode = MovementMode.DirectionalMovement;
        directionalTarget = point;
        directionalMovementTimer = 0f;
        directionalMovementDuration = duration;
    }
    
    /// <summary>
    /// Stop all movement immediately
    /// </summary>
    public void Stop()
    {
        currentMode = MovementMode.Idle;
        currentVelocity = Vector2.zero;
        
        if (agent != null)
        {
            agent.ResetPath();
        }
    }
    
    /// <summary>
    /// Check if the controller can reach a specific destination
    /// </summary>
    public bool CanReachDestination(Vector3 destination)
    {
        if (agent == null) return false;
        
        NavMeshPath path = new NavMeshPath();
        return agent.CalculatePath(destination, path) && path.status == NavMeshPathStatus.PathComplete;
    }
    
    /// <summary>
    /// Get the distance to the target destination along the calculated path
    /// </summary>
    public float GetPathDistance()
    {
        if (agent == null || !agent.hasPath) return 0f;
        
        float distance = 0f;
        Vector3[] corners = agent.path.corners;
        
        for (int i = 0; i < corners.Length - 1; i++)
        {
            distance += Vector3.Distance(corners[i], corners[i + 1]);
        }
        
        return distance;
    }
    
    /// <summary>
    /// Check if currently following a path
    /// </summary>
    public bool IsFollowingPath()
    {
        return currentMode == MovementMode.PathToTarget && agent != null && agent.hasPath;
    }
    
    /// <summary>
    /// Check if currently doing directional movement
    /// </summary>
    public bool IsDoingDirectionalMovement()
    {
        return currentMode == MovementMode.DirectionalMovement;
    }
    
    /// <summary>
    /// Apply external force and pause normal movement control
    /// </summary>
    public void SetExternalForce(Vector2 direction, float force, ForceMode2D forceMode = ForceMode2D.Impulse)
    {
        if (rb == null) return;
        
        isExternalForceActive = true;
        rb.AddForce(direction.normalized * force, forceMode);
        
        // Debug.Log($"MovementController: External force applied - {direction.normalized * force}");
    }
    
    /// <summary>
    /// Release external force control and resume normal movement
    /// </summary>
    public void ReleaseExternalForce()
    {
        isExternalForceActive = false;
        
        // Immediately resume normal movement control
        if (rb != null)
        {
            rb.linearVelocity = currentVelocity;
        }
        
        // Debug.Log("MovementController: External force released, resuming normal movement");
    }
    
    /// <summary>
    /// Check if external force is currently active
    /// </summary>
    public bool IsExternalForceActive()
    {
        return isExternalForceActive;
    }
    
    void OnDrawGizmosSelected()
    {
        if (agent != null && agent.hasPath)
        {
            // Draw the path
            Gizmos.color = Color.yellow;
            Vector3[] corners = agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
            
            // Draw current waypoint
            if (corners.Length > currentWaypointIndex)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(corners[currentWaypointIndex], waypointThreshold);
            }
        }
        
        // Draw directional target
        if (currentMode == MovementMode.DirectionalMovement)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, directionalTarget);
            Gizmos.DrawWireSphere(directionalTarget, 0.2f);
        }
    }
}