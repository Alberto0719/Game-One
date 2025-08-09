using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BoomerangAction : MonoBehaviour, IAction
{
    [Header("Boomerang Settings")]
    [SerializeField] private float cooldownDuration = 3f;
    [SerializeField] private float telegraphDuration = 0.4f;
    [SerializeField] private float damage = 25f;
    private float range;
    
    private GameObject telegraphObject;
    private GameObject boomerangObject;
    private SpriteRenderer telegraphRenderer;
    private SpriteRenderer boomerangRenderer;
    private Animator boomerangAnimator;
    
    private float cooldownTimer = 0f;
    private ActionResult currentStatus = ActionResult.Success;
    private bool isExecuting = false;
    private HashSet<Damageable> hitTargets = new HashSet<Damageable>();

    // layer mask for collision detection
    [SerializeField] private LayerMask targetLayer;
    
    public float GetRange()
    {
        return range;
    }
    void Start()
    {
        SetupBoomerangComponents();
        cooldownTimer = cooldownDuration; // Start on cooldown
    }
    
    void Update()
    {
        UpdateCooldown();
    }
    
    private void SetupBoomerangComponents()
    {
        // Find Telegraph child
        Transform telegraphTransform = transform.Find("Telegraph");
        if (telegraphTransform != null)
        {
            telegraphObject = telegraphTransform.gameObject;
            telegraphRenderer = telegraphTransform.GetComponent<SpriteRenderer>();
            range = telegraphRenderer.bounds.size.x;
        }


        // Find Boomerang child
        Transform boomerangTransform = transform.Find("Boomerang");
        if (boomerangTransform != null)
        {
            boomerangObject = boomerangTransform.gameObject;
            boomerangRenderer = boomerangTransform.GetComponent<SpriteRenderer>();
        }
        
        // Get animator on this root object
        boomerangAnimator = GetComponent<Animator>();
        
        // Ensure animator speed is normal
        if (boomerangAnimator != null)
        {
            boomerangAnimator.speed = 1f;
        }
        

        // Deactivate Telegraph and Boomerang objects initially
        if (telegraphObject != null)
        {
            telegraphObject.SetActive(false);
        }
        
        if (boomerangObject != null)
        {
            boomerangObject.SetActive(false);
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
        
        // Activate only the Telegraph first
        if (telegraphObject != null)
        {
            telegraphObject.SetActive(true);
        }
        
        // Keep Boomerang inactive during telegraph
        if (boomerangObject != null)
        {
            boomerangObject.SetActive(false);
        }
        
        // Start telegraph animation
        if (telegraphRenderer != null && boomerangRenderer != null)
        {
            yield return StartCoroutine(AnimateTelegraph());
        }
        
        // After telegraph, switch to boomerang
        if (telegraphObject != null)
        {
            telegraphObject.SetActive(false);
        }
        
        if (boomerangObject != null)
        {
            boomerangObject.SetActive(true);
        }
        
        // Start projectile animation with slower speed
        if (boomerangAnimator != null)
        {
            boomerangAnimator.speed = 0.8f;
            boomerangAnimator.SetTrigger("Attack");
        }
        
        // Keep executing until animation ends
        // The StateMachineBehaviour will call OnAnimationEnd when complete
    }
    
    private IEnumerator AnimateTelegraph()
    {
        if (telegraphRenderer == null || boomerangRenderer == null) yield break;
        
        // Get the target color from the boomerang projectile
        Color targetColor = boomerangRenderer.color;
        
        // Start with 0.2 alpha version of the color
        Color startColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0.2f);
        telegraphRenderer.color = startColor;
        
        // Animate to 0.4 alpha over telegraphDuration
        float elapsedTime = 0f;
        while (elapsedTime < telegraphDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0.2f, 0.4f, elapsedTime / telegraphDuration);
            telegraphRenderer.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            yield return null;
        }
        
        // Ensure we reach the exact target alpha
        telegraphRenderer.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0.4f);
        
        // Make telegraph disappear
        telegraphRenderer.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
    }
    
    public void Interrupt()
    {
        // Debug.Log("Boomerang action interrupted!");
        
        // Stop any running coroutines
        StopAllCoroutines();
        
        // Reset animator speed
        if (boomerangAnimator != null)
        {
            boomerangAnimator.speed = 1f;
            boomerangAnimator.SetTrigger("Interrupt");
        }
        
        // Clear hit targets
        hitTargets.Clear();
        
        // Deactivate all components
        if (telegraphObject != null)
        {
            telegraphObject.SetActive(false);
        }
        
        if (boomerangObject != null)
        {
            boomerangObject.SetActive(false);
        }
        
        // Reset state
        isExecuting = false;
        currentStatus = ActionResult.Failure;
        
        // Reset cooldown to a shorter time so it can attack again sooner
        cooldownTimer = 1f;
    }
    
    public float GetCooldownRemaining()
    {
        return Mathf.Max(0f, cooldownTimer);
    }
    
    public bool IsOnCooldown()
    {
        return cooldownTimer > 0f;
    }
    
    // This method will be called by the BoomerangStateBehaviour when animation ends
    public void OnAnimationEnd()
    {
        // Reset animator speed
        if (boomerangAnimator != null)
        {
            boomerangAnimator.speed = 1f;
        }
        
        // Deactivate the Boomerang object
        if (boomerangObject != null)
        {
            boomerangObject.SetActive(false);
        }
        
        // Clear hit targets
        hitTargets.Clear();
        
        // Reset state
        isExecuting = false;
        currentStatus = ActionResult.Success;
        cooldownTimer = cooldownDuration;
    }
    
    // Collision detection for damage
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only deal damage when boomerang is executing
        if (!isExecuting) return;

        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            // Debug.Log($"Boomerang hit valid target: {other.name}");
        }
        else
        {
            // Debug.Log($"Boomerang hit invalid target: {other.name}");
            return;
        }

        Damageable damageable = other.GetComponent<Damageable>();
        if (damageable != null && !hitTargets.Contains(damageable))
        {
            hitTargets.Add(damageable);
            
            // Debug.Log($"Dealing {damage} damage to {other.name}");
            damageable.TakeDamage(damage);
        }
        else if (damageable != null)
        {
            // Debug.Log("Target already hit by this boomerang");
        }
        else
        {
            // Debug.Log("No Damageable component found");
        }
    }
}