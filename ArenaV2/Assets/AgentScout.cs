using UnityEngine;
using System.Collections.Generic;
using System.Xml.XPath;
using System;

public class AgentScout : MonoBehaviour, IAgent
{
    public Transform player { get; private set; }
    private Transform pointerAnchor;
    private Damageable damageable;
    
    // Boomerang action component
    public BoomerangAction boomerangAction { get; private set; }
    
    // Movement controller
    public MovementController movementController { get; private set; }
    
    // Effects system
    private BurnEffect burnEffect;
    private KnockbackEffect knockbackEffect;
    
    // Behavior system
    private List<IBehavior> behaviors = new List<IBehavior>();
    private AttackBehavior attackBehavior;
    private CloseBehavior closeBehavior;
    private KiteBehavior kiteBehavior;
    private IBehavior currentBehavior;

    // for debugging
    [SerializeField] private string lastBehaviorExecuted;
    
    
    public Rigidbody2D rb { get; private set; }
    
    [SerializeField] public float moveSpeed { get; set; } = 2.5f;
    
    
    public Transform GetTarget()
    {
        return player;
    }
    
    void Start()
    {
        // global player awareness for now
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // pointer to show facing
        pointerAnchor = transform.Find("PointerAnchor");        
       
        // Initialize damageable component
        damageable = GetComponent<Damageable>();
        
        // Subscribe to damage events
        damageable.OnDamageTaken += HandleDamageTaken;

        // Initialize effects
        burnEffect = GetComponent<BurnEffect>();      
        knockbackEffect = GetComponent<KnockbackEffect>();

        // Subscribe to effect events for additional agent logic
        if (knockbackEffect != null)
        {
            knockbackEffect.OnKnockbackStarted += HandleKnockbackStarted;
            knockbackEffect.OnKnockbackEnded += HandleKnockbackEnded;
        }
        
        if (burnEffect != null)
        {
            burnEffect.OnBurnStarted += HandleBurnStarted;
            burnEffect.OnBurnEnded += HandleBurnEnded;
        }
        
        // boomerang action component
        boomerangAction = GetComponentInChildren<BoomerangAction>();
        Debug.Assert(boomerangAction != null, "BoomerangAction component is missing on TargetFSM object!");
        
        // Initialize movement controller
        movementController = GetComponent<MovementController>();
        movementController.MoveSpeed = moveSpeed;
                
        // Initialize behaviors
        InitializeBehaviors();

        rb = GetComponent<Rigidbody2D>();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (damageable != null)
        {
            damageable.OnDamageTaken -= HandleDamageTaken;
        }
        
        if (knockbackEffect != null)
        {
            knockbackEffect.OnKnockbackStarted -= HandleKnockbackStarted;
            knockbackEffect.OnKnockbackEnded -= HandleKnockbackEnded;
        }
        
        if (burnEffect != null)
        {
            burnEffect.OnBurnStarted -= HandleBurnStarted;
            burnEffect.OnBurnEnded -= HandleBurnEnded;
        }
    }
    
    private void InitializeEffects()
    {
        // Get or add effect components
    }
    
    private void InitializeBehaviors()
    {
        // Initialize behavior instances
        attackBehavior = new AttackBehavior(this, movementController, boomerangAction);
        closeBehavior = new CloseBehavior(this, movementController);
        kiteBehavior = new KiteBehavior(this, movementController);
        
        // Add to behaviors list for management
        behaviors.Add(attackBehavior);
        behaviors.Add(closeBehavior);
        behaviors.Add(kiteBehavior);
    }
    
    
    void Update()
    {
        // no target, nothing to do
        if (player == null) return;

        // no control if knocked back
        if (knockbackEffect != null && knockbackEffect.enabled) return;

        // Priority 1: continue current behavior
        if (currentBehavior != null)
        {
            lastBehaviorExecuted = currentBehavior.GetType().Name + " (Continue)";

            BehaviorResult result = currentBehavior.Execute();
            if (result == BehaviorResult.Success || result == BehaviorResult.Failure) currentBehavior = null;
            if (result == BehaviorResult.Success || result == BehaviorResult.Continue) return;
        }

        // Facing is now handled by individual behaviors

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool attackOffCooldown = boomerangAction.CanExecute();
        bool inAttackRange = distanceToPlayer <= boomerangAction.GetRange();
        
        // Priority 2: If ready and in range, attack
        if (attackOffCooldown && inAttackRange)
        {
            BehaviorResult result = attackBehavior.Execute();
            lastBehaviorExecuted = attackBehavior.GetType().Name;
            if (result == BehaviorResult.Continue) currentBehavior = attackBehavior;
            if (result == BehaviorResult.Success || result == BehaviorResult.Continue) return;
        }

        // Priority 3: If ready but out of range, close
        else if (attackOffCooldown)
        {
            BehaviorResult result = closeBehavior.Execute();
            lastBehaviorExecuted = closeBehavior.GetType().Name;
            if (result == BehaviorResult.Continue) currentBehavior = closeBehavior;
            if (result == BehaviorResult.Success || result == BehaviorResult.Continue) return;
        }

        // Priority 4: Otherwise kite
        if (kiteBehavior.Execute() == BehaviorResult.Continue) currentBehavior = kiteBehavior;
        lastBehaviorExecuted = kiteBehavior.GetType().Name;
    }
    
    public void SetFacing(Vector3 targetPoint)
    {
        if (pointerAnchor == null) return;
        
        // Calculate direction to target point
        Vector2 directionToTarget = (targetPoint - transform.position).normalized;
        
        // Calculate angle and rotate pointer anchor
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        pointerAnchor.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
        
    private void HandleDamageTaken(float damage)
    {
        // Debug.Log($"AgentScout took {damage} damage");
        // Additional damage handling can be added here if needed
    }
    
    private void HandleKnockbackStarted()
    {
        // Interrupt current action when knockback starts
        // Debug.Log("AgentScout knockback started - actions interrupted");

        if (boomerangAction != null && boomerangAction.GetStatus() == ActionResult.InProgress)
        {
            // Debug.Log("Boomerang interrupted");
            boomerangAction.Interrupt();
        }
        
        // Interrupt current behavior
        currentBehavior = null;
    }
    
    private void HandleKnockbackEnded()
    {
        // Debug.Log("AgentScout knockback ended - can resume normal behavior");
    }
    
    private void HandleBurnStarted()
    {
        // Debug.Log("AgentScout burn effect started");
    }
    
    private void HandleBurnEnded()
    {
        // Debug.Log("AgentScout burn effect ended");
    }
        
}