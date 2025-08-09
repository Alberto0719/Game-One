using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlashAction : MonoBehaviour, IAction
{
    [Header("Slash Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private float cooldownDuration = 0.5f;

    private Animator slashAnimator;
    private GameObject slashObject;
    
    private float cooldownTimer = 0f;
    private ActionResult currentStatus = ActionResult.Success;
    private bool isExecuting = false;
    private HashSet<Damageable> hitTargets = new HashSet<Damageable>();

    // layer mask for collision detection
    [SerializeField] private LayerMask targetLayer;

    public float GetRange()
    {
        return 1f;
    }

    void Start()
    {
        SetupSlashComponents();
    }
    
    void Update()
    {
        UpdateCooldown();
    }
    
    private void SetupSlashComponents()
    {
        // Get animator on this root object
        slashAnimator = GetComponent<Animator>();

        slashObject = transform.Find("Slash").gameObject;
        slashObject.SetActive(false);
        
        // Ensure animator speed is normal
        if (slashAnimator != null)
        {
            slashAnimator.speed = 1f;
        }
        
    }
    
    private void UpdateCooldown()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }
    
    public ActionResult GetStatus()
    {
        return currentStatus;
    }
    
    public bool CanExecute()
    {
        return !isExecuting && cooldownTimer <= 0f;
    }
    
    public IEnumerator Execute()
    {
        if (!CanExecute())
        {
            currentStatus = ActionResult.Failure;
            yield break;
        }
        
        isExecuting = true;
        currentStatus = ActionResult.InProgress;
        
        // Clear hit targets for this attack
        hitTargets.Clear();
        
        // Activate root object (weapon visible)
        slashObject.SetActive(true);
        
        // Start slash animation
        if (slashAnimator != null)
        {
            slashAnimator.SetTrigger("Attack");
        }
        
        // The animation will call OnAnimationEnd when complete
    }
    
    public void Interrupt()
    {
        // Debug.Log("Slash action interrupted!");
        
        // Stop any running coroutines
        StopAllCoroutines();
        
        // Reset animator
        if (slashAnimator != null)
        {
            slashAnimator.SetTrigger("Interrupt");
        }
        
        // Clear hit targets
        hitTargets.Clear();
        
        // Deactivate
        slashObject.SetActive(false);
        
        // Reset state
        isExecuting = false;
        currentStatus = ActionResult.Failure;
        
        // Reset cooldown to a shorter time
        cooldownTimer = 0.2f;
    }
    
    public float GetCooldownRemaining()
    {
        return Mathf.Max(0f, cooldownTimer);
    }
    
    public bool IsOnCooldown()
    {
        return cooldownTimer > 0f;
    }
    
    // This method will be called by the SlashAttackBehaviour when animation ends
    public void OnAnimationEnd()
    {
        // Reset animator speed
        if (slashAnimator != null)
        {
            slashAnimator.speed = 1f;
        }
        
        // Clear hit targets
        hitTargets.Clear();
        
        slashObject.SetActive(false);
        
        // Reset state
        isExecuting = false;
        currentStatus = ActionResult.Success;
        cooldownTimer = cooldownDuration;
    }
    
    // Called when slash collider hits something
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isExecuting) return;

        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            // Debug.Log($"Slash hit valid target: {other.name}");
        }
        else
        {
            // Debug.Log($"Slash hit invalid target: {other.name}");
            return;
        }

        Damageable damageable = other.GetComponent<Damageable>();
        if (damageable != null && !hitTargets.Contains(damageable))
        {
            hitTargets.Add(damageable);
            
            // Calculate knockback direction from slash to target
            Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
            
            // Apply damage directly to Damageable (no knockback applied here)
            // Debug.Log($"Dealing {damage} damage without automatic knockback");
            damageable.TakeDamage(damage);
            
            // Apply knockback directly to KnockbackEffect if present
            KnockbackEffect knockbackEffect = other.GetComponent<KnockbackEffect>();
            if (knockbackEffect != null && knockbackForce > 0f)
            {
                // Debug.Log($"Applying {knockbackForce} knockback force");
                knockbackEffect.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration);
            }
        }
        else if (damageable != null)
        {
            // Debug.Log("Damageable already hit by this slash");
        }
        else
        {
            // Debug.Log("No Damageable component found");
        }
    }
    
    // Called when slash becomes active to reset hit tracking
    void OnEnable()
    {
        hitTargets.Clear();
    }
}