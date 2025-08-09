using UnityEngine;
using System.Collections.Generic;
using System.Xml.XPath;
using System;

public class AgentSoldier : MonoBehaviour, IAgent
{
    public Transform player { get; private set; }
    private Transform pointerAnchor;
    private Damageable damageable;
    
    // Dash attack properties
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float dashRange = 2f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float retreatDuration = 1f;
    [SerializeField] private float retreatSpeed = 4f;
    
    private float lastAttackTime = -10f;
    
    // Movement controller
    public MovementController movementController { get; private set; }
    
    // Effects system
    private BurnEffect burnEffect;
    private KnockbackEffect knockbackEffect;
    
    // Behavior system
    private List<IBehavior> behaviors = new List<IBehavior>();
    private ChargeBehavior chargeBehavior;
    private DashAttackBehavior dashAttackBehavior;
    private RetreatBehavior retreatBehavior;
    private IBehavior currentBehavior;

    // for debugging
    [SerializeField] private string lastBehaviorExecuted;
    
    
    public Rigidbody2D rb { get; private set; }
    
    [SerializeField] public float moveSpeed { get; set; } = 3f;
    
    
    public Transform GetTarget()
    {
        return player;
    }
    
    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }
    
    public void SetLastAttackTime()
    {
        lastAttackTime = Time.time;
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
    
    private void InitializeBehaviors()
    {
        // Initialize behavior instances for soldier
        chargeBehavior = new ChargeBehavior(this, movementController);
        dashAttackBehavior = new DashAttackBehavior(this, movementController, dashSpeed, dashRange, dashDuration);
        retreatBehavior = new RetreatBehavior(this, movementController, retreatSpeed, retreatDuration);
        
        // Add to behaviors list for management
        behaviors.Add(chargeBehavior);
        behaviors.Add(dashAttackBehavior);
        behaviors.Add(retreatBehavior);
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

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool attackOffCooldown = CanAttack();
        bool inDashRange = distanceToPlayer <= dashRange;
        
        // Priority 2: If ready and in dash range, perform dash attack
        if (attackOffCooldown && inDashRange)
        {
            BehaviorResult result = dashAttackBehavior.Execute();
            lastBehaviorExecuted = dashAttackBehavior.GetType().Name;
            if (result == BehaviorResult.Continue) currentBehavior = dashAttackBehavior;
            if (result == BehaviorResult.Success || result == BehaviorResult.Continue) return;
        }

        // Priority 3: If ready but out of range, charge toward player
        else if (attackOffCooldown)
        {
            BehaviorResult result = chargeBehavior.Execute();
            lastBehaviorExecuted = chargeBehavior.GetType().Name;
            if (result == BehaviorResult.Continue) currentBehavior = chargeBehavior;
            if (result == BehaviorResult.Success || result == BehaviorResult.Continue) return;
        }

        // Priority 4: Otherwise retreat (after attacking)
        BehaviorResult retreatResult = retreatBehavior.Execute();
        lastBehaviorExecuted = retreatBehavior.GetType().Name;
        if (retreatResult == BehaviorResult.Continue) currentBehavior = retreatBehavior;
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
        // Debug.Log($"AgentSoldier took {damage} damage");
        // Additional damage handling can be added here if needed
    }
    
    private void HandleKnockbackStarted()
    {
        // Interrupt current action when knockback starts
        // Debug.Log("AgentSoldier knockback started - actions interrupted");
        
        // Interrupt current behavior
        currentBehavior = null;
    }
    
    private void HandleKnockbackEnded()
    {
        // Debug.Log("AgentSoldier knockback ended - can resume normal behavior");
    }
    
    private void HandleBurnStarted()
    {
        // Debug.Log("AgentSoldier burn effect started");
    }
    
    private void HandleBurnEnded()
    {
        // Debug.Log("AgentSoldier burn effect ended");
    }
        
}