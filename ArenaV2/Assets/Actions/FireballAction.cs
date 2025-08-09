using UnityEngine;
using System.Collections;

/// <summary>
/// FireballAction handles fireball spellcasting as a modular action component.
/// Should be placed on FireballRoot under Player -> ActionPivot -> ActionAnchor.
/// Requires FirePool to be available in the scene and player Animator with "Spellcast" trigger.
/// Animation events should call LaunchFireball() and OnAnimationEnd() at appropriate times.
/// </summary>
public class FireballAction : MonoBehaviour, IAction
{
    [Header("Fireball Settings")]
    [SerializeField] private float cooldownDuration = 1f;
    [SerializeField] private LayerMask targetLayer;
    
    private float cooldownTimer = 0f;
    private ActionResult currentStatus = ActionResult.Success;
    private bool isExecuting = false;
    private Vector2 targetDirection;
    private Animator playerAnimator;
    private Transform actionAnchor;
    
    public float GetRange()
    {
        return 10f; // Fireball has good range
    }
    
    void Start()
    {
        // Get reference to player animator (parent of ActionPivot -> ActionAnchor -> FireballRoot)
        Transform actionPivot = transform.parent?.parent;
        if (actionPivot != null)
        {
            playerAnimator = actionPivot.parent?.GetComponent<Animator>();
        }
        
        // Get ActionAnchor reference
        actionAnchor = transform.parent;
    }
    
    void Update()
    {
        UpdateCooldown();
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
        return !isExecuting && cooldownTimer <= 0f && FirePool.Instance != null && FirePool.Instance.HasAvailableFireball();
    }
    
    public IEnumerator Execute()
    {
        return Execute(Vector2.right); // Default direction, should be set via Execute(direction)
    }
    
    public IEnumerator Execute(Vector2 direction)
    {
        if (!CanExecute())
        {
            currentStatus = ActionResult.Failure;
            yield break;
        }
        
        isExecuting = true;
        currentStatus = ActionResult.InProgress;
        targetDirection = direction.normalized;
        
        // Start spellcast animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("Spellcast");
        }
        
        // Animation will call OnAnimationEnd when complete
        yield return null;
    }
    
    public void Interrupt()
    {
        Debug.Log("Fireball action interrupted!");
        
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
    
    // Called by animation event during spellcast animation
    public void LaunchFireball()
    {
        if (FirePool.Instance == null) return;
        
        // Get fireball from pool and launch it
        Fireball fireball = FirePool.Instance.GetFireball();
        if (fireball != null)
        {
            // Launch from ActionAnchor position if available, otherwise from transform position
            Vector3 launchPosition = actionAnchor != null ? actionAnchor.position : transform.position;
            fireball.Launch(launchPosition, targetDirection, targetLayer);
            Debug.Log($"Fireball launched in direction {targetDirection} from {launchPosition} targeting layers: {targetLayer}");
        }
    }
    
    // Called by animation event or state machine behavior when spellcast completes
    public void OnAnimationEnd()
    {
        // Reset state
        isExecuting = false;
        currentStatus = ActionResult.Success;
        cooldownTimer = cooldownDuration;
        
        Debug.Log("Fireball action completed");
    }
}
