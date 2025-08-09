using UnityEngine;
using System;

public class KnockbackEffect : MonoBehaviour
{
    private float defaultKnockbackDuration = 0.3f;
    
    private float knockbackTimer = 0f;
    private float knockbackDuration = 0f;
    private Rigidbody2D rb;
    private MovementController movementController;
    
    // Events for notifications
    public event Action OnKnockbackStarted;
    public event Action OnKnockbackEnded;
        
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movementController = GetComponent<MovementController>();
        enabled = false; // Start disabled
    }
    
    void Update()
    {
        knockbackTimer += Time.deltaTime;
        
        if (knockbackTimer >= knockbackDuration)
        {
            EndKnockback();
        }
    }
    
    public void ApplyKnockback(Vector2 direction, float force)
    {
        ApplyKnockback(direction, force, defaultKnockbackDuration);
    }
    
    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        // Apply force through MovementController if available
        if (movementController != null)
        {
            movementController.SetExternalForce(direction, force, ForceMode2D.Impulse);
        }
        else if (rb != null)
        {
            // Fallback for entities without MovementController
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogWarning("KnockbackEffect: No MovementController or Rigidbody2D found!");
            return;
        }
        
        // Set knockback effect with custom duration
        knockbackDuration = duration;
        knockbackTimer = 0f;
        enabled = true;
        
        OnKnockbackStarted?.Invoke();
        // Debug.Log($"KnockbackEffect applied with force {force} for {duration}s in direction {direction}");
    }
    
    private void EndKnockback()
    {
        // Release external force control if using MovementController
        if (movementController != null)
        {
            movementController.ReleaseExternalForce();
        }
        
        enabled = false;
        knockbackTimer = 0f;
        knockbackDuration = 0f;
        
        OnKnockbackEnded?.Invoke();
        // Debug.Log("KnockbackEffect ended");
    }
    
    public void ForceEnd()
    {
        if (enabled)
        {
            EndKnockback();
        }
    }
}