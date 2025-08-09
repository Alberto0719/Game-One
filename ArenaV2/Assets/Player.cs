using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private const float SPEED = 3.0f;  // 1 unit per sec
    private const float CAMERA_BOUNDARY_BUFFER = 0.5f; // Stay this far from camera edge
    private Vector2 velocity = Vector2.zero;
    private Vector2 direction = Vector2.zero;
    private Vector2 lastDirection = Vector2.down; // Start facing down
    private InputSystem_Actions controls;
    private Animator animator;
    private SlashAction slashAction;
    private FireballAction fireballAction;
    private Camera playerCamera;
    private Transform actionPivot;
    private Transform actionAnchor;
    private Damageable damageable;

    public void Heal(float amount)
{
    if (damageable != null)
    {
        damageable.Heal(amount);
        Debug.Log($"Player healed by {amount}");
    }
}

    


    void Awake()
    {
        controls = new InputSystem_Actions();
        controls.Player.Move.performed += ctx => direction = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => direction = Vector2.zero;
        controls.Player.Attack.performed += ctx => TryAttack();
        controls.UI.RightClick.performed += ctx => TryCastFireball();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        playerCamera = Camera.main;
        
        // Find ActionPivot and ActionAnchor
        actionPivot = transform.Find("ActionPivot");
        if (actionPivot != null)
        {
            actionAnchor = actionPivot.Find("ActionAnchor");

            // Get action components under ActionAnchor
            if (actionAnchor != null)
            {
                slashAction = actionAnchor.GetComponentInChildren<SlashAction>();
                fireballAction = actionAnchor.GetComponentInChildren<FireballAction>();
            }
        }
        
        // Initialize damageable component
        damageable = GetComponent<Damageable>();
        if (damageable == null)
        {
            damageable = gameObject.AddComponent<Damageable>();
        }
        
        // Subscribe to damage events if needed for knockback
        damageable.OnDamageTaken += HandleDamageTaken;
    }

    private void OnEnable() 
    { 
        controls.Player.Enable(); 
        controls.UI.Enable(); // Enable UI actions for right-click
    }
    
    private void OnDisable() 
    { 
        controls.Player.Disable(); 
        controls.UI.Disable();
    }

    void Update()
    {
        // Check if any action is currently executing
        bool isActionExecuting = (slashAction != null && slashAction.GetStatus() == ActionResult.InProgress) ||
                                (fireballAction != null && fireballAction.GetStatus() == ActionResult.InProgress);
        
        // Freeze movement during action execution
        if (!isActionExecuting)
        {
            velocity = direction * SPEED;
            
            // Apply camera boundary constraints to movement
            Vector2 constrainedVelocity = ConstrainToCameraBounds(transform.position, velocity);
            transform.position = (Vector2)transform.position + constrainedVelocity * Time.deltaTime;
        }
        else
        {
            velocity = Vector2.zero;
        }

        // Update facing direction based on mouse position
        UpdateFacingDirection();

        // Animation logic
        bool isMoving = direction.magnitude > 0.1f && !isActionExecuting;
        animator.SetBool("IsMoving", isMoving);

        animator.SetFloat("DirectionX", lastDirection.x);
        animator.SetFloat("DirectionY", lastDirection.y);
    }
    
    private void UpdateFacingDirection()
    {
        if (playerCamera == null) return;
        
        // Get mouse position in world space
        Vector3 mouseWorldPos = playerCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0;
        
        // Calculate direction to mouse
        Vector2 mouseDirection = (mouseWorldPos - transform.position).normalized;
        
        // Update ActionPivot to point toward mouse
        if (actionPivot != null)
        {
            float angle = Mathf.Atan2(mouseDirection.y, mouseDirection.x) * Mathf.Rad2Deg;
            actionPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        // Always face mouse cursor direction (no snapping)
        lastDirection = mouseDirection;
    }
    
    private void TryAttack()
    {
        if (slashAction == null || !slashAction.CanExecute()) return;
                        
        // Execute slash action
        StartCoroutine(slashAction.Execute());
    }
    
    private void TryCastFireball()
    {
        if (fireballAction == null || !fireballAction.CanExecute()) return;
        
        // Get mouse position in world space to determine direction
        Vector3 mouseWorldPos = playerCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0;
        
        // Calculate direction from player to mouse
        Vector2 fireballDirection = (mouseWorldPos - transform.position).normalized;
        
        // Execute fireball action with direction
        StartCoroutine(fireballAction.Execute(fireballDirection));
    }
    
    // Called by animation event during spellcast animation
    public void LaunchFireball()
    {
        // Delegate to FireballAction
        if (fireballAction != null)
        {
            fireballAction.LaunchFireball();
        }
    }
    
    // Called by animation event or state machine behavior
    public void OnAttackComplete()
    {
        // Delegate to SlashAction
        if (slashAction != null)
        {
            slashAction.OnAnimationEnd();
        }
    }
    
    // Called by spellcast animation event or state machine behavior
    public void OnSpellcastComplete()
    {
        // Delegate to FireballAction
        if (fireballAction != null)
        {
            fireballAction.OnAnimationEnd();
        }
    }
    
    
    public void TakeDamage(float damage, Vector2 knockbackDirection = default, float knockbackForce = 0f)
    {
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            
            // Apply knockback if player has a knockback effect component
            if (knockbackForce > 0f && knockbackDirection != Vector2.zero)
            {
                KnockbackEffect knockbackEffect = GetComponent<KnockbackEffect>();
                if (knockbackEffect != null)
                {
                    knockbackEffect.ApplyKnockback(knockbackDirection, knockbackForce);
                }
            }
        }
    }
    
    private void HandleDamageTaken(float damage)
    {
        Debug.Log($"Player took {damage} damage");
        // Additional damage handling can be added here if needed
    }
    
    
    public float GetCurrentHealth()
    {
        return damageable != null ? damageable.CurrentHealth : 0f;
    }
    
    public float GetMaxHealth()
    {
        return damageable != null ? damageable.MaxHealth : 0f;
    }
    
    public bool IsDead()
    {
        return damageable != null ? damageable.IsDead : false;
    }
    
    private Vector2 ConstrainToCameraBounds(Vector3 currentPosition, Vector2 velocity)
    {
        if (playerCamera == null) return velocity;
        
        // Get camera bounds in world coordinates
        float cameraHeight = playerCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * playerCamera.aspect;
        
        Vector3 cameraPosition = playerCamera.transform.position;
        float leftBound = cameraPosition.x - cameraWidth/2f + CAMERA_BOUNDARY_BUFFER;
        float rightBound = cameraPosition.x + cameraWidth/2f - CAMERA_BOUNDARY_BUFFER;
        float bottomBound = cameraPosition.y - cameraHeight/2f + CAMERA_BOUNDARY_BUFFER;
        float topBound = cameraPosition.y + cameraHeight/2f - CAMERA_BOUNDARY_BUFFER;
        
        // Check if the intended movement would go out of bounds
        Vector2 intendedPosition = (Vector2)currentPosition + velocity * Time.deltaTime;
        Vector2 constrainedVelocity = velocity;
        
        // If moving outside bounds, stop movement in that direction
        if (intendedPosition.x < leftBound && velocity.x < 0)
        {
            constrainedVelocity.x = 0; // Stop horizontal movement toward left bound
        }
        else if (intendedPosition.x > rightBound && velocity.x > 0)
        {
            constrainedVelocity.x = 0; // Stop horizontal movement toward right bound
        }
        
        if (intendedPosition.y < bottomBound && velocity.y < 0)
        {
            constrainedVelocity.y = 0; // Stop vertical movement toward bottom bound
        }
        else if (intendedPosition.y > topBound && velocity.y > 0)
        {
            constrainedVelocity.y = 0; // Stop vertical movement toward top bound
        }
        
        // If already outside bounds, push back toward center
        if (currentPosition.x < leftBound)
        {
            constrainedVelocity.x = Mathf.Max(0, constrainedVelocity.x); // Only allow rightward movement
        }
        else if (currentPosition.x > rightBound)
        {
            constrainedVelocity.x = Mathf.Min(0, constrainedVelocity.x); // Only allow leftward movement
        }
        
        if (currentPosition.y < bottomBound)
        {
            constrainedVelocity.y = Mathf.Max(0, constrainedVelocity.y); // Only allow upward movement
        }
        else if (currentPosition.y > topBound)
        {
            constrainedVelocity.y = Mathf.Min(0, constrainedVelocity.y); // Only allow downward movement
        }
        
        return constrainedVelocity;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (damageable != null)
        {
            damageable.OnDamageTaken -= HandleDamageTaken;
        }
    }
}

