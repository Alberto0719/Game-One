using UnityEngine;
using System.Collections;
using System;

public class Damageable : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float flashDuration = 0.1f;
    
    private float currentHealth;
    private bool isFlashing = false;
    private HealthBar healthBar;
    private SpriteRenderer[] spriteRenderers;
    
    // Events
    public event Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    public event Action OnDeath;
    public event Action<float> OnDamageTaken; // damage only - effects handled separately
    
    // Public properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;
    public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    
    void Start()
    {
        InitializeHealth();
    }
    
    private void InitializeHealth()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Setup health bar
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetPctHealth(1f);
        }
        
        // Get sprite renderers for flashing
        SetupSpriteRenderers();
        
        // Notify health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private void SetupSpriteRenderers()
    {
        // Get all sprite renderers in this object and children
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();
        
        // Filter out health bar and UI renderers if needed
        spriteRenderers = allRenderers;
        
        // Fallback: get just the main sprite renderer
        if (spriteRenderers.Length == 0)
        {
            SpriteRenderer mainRenderer = GetComponent<SpriteRenderer>();
            if (mainRenderer != null)
            {
                spriteRenderers = new SpriteRenderer[] { mainRenderer };
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        
        // Apply damage to health
        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Update health bar
        if (healthBar != null)
        {
            healthBar.SetPctHealth(HealthPercentage);
        }
        
        // Flash visual feedback
        FlashRed();
        
        // Notify events
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Check for death
        if (IsDead && previousHealth > 0)
        {
            OnDeath?.Invoke();
        }
        
        // Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
    }
    
    // Legacy method for compatibility - will be removed once all code is updated
    [System.Obsolete("Use TakeDamage(float damage) and handle effects separately")]
    public void TakeDamage(float damage, Vector2 knockbackDirection = default, float knockbackForce = 0f, bool interruptActions = true)
    {
        TakeDamage(damage);
    }
    
    public void Heal(float healAmount)
    {
        if (IsDead) return;
        
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        
        // Update health bar
        if (healthBar != null)
        {
            healthBar.SetPctHealth(HealthPercentage);
        }
        
        // Notify health change
        if (currentHealth != previousHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            // Debug.Log($"{gameObject.name} healed {healAmount}. Health: {currentHealth}/{maxHealth}");
        }
    }
    
    public void SetMaxHealth(float newMaxHealth)
    {
        float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 1f;
        maxHealth = newMaxHealth;
        currentHealth = newMaxHealth * healthPercentage;
        
        // Update health bar
        if (healthBar != null)
        {
            healthBar.SetPctHealth(HealthPercentage);
        }
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetHealth(float newHealth)
    {
        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        
        // Update health bar
        if (healthBar != null)
        {
            healthBar.SetPctHealth(HealthPercentage);
        }
        
        // Notify health change
        if (currentHealth != previousHealth)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            // Check for death
            if (IsDead && previousHealth > 0)
            {
                OnDeath?.Invoke();
            }
        }
    }
    
    public void FlashRed()
    {
        if (isFlashing || spriteRenderers.Length == 0) return;
        StartCoroutine(FlashRedCoroutine());
    }

    private IEnumerator FlashRedCoroutine()
    {
        isFlashing = true;
        
        // Store original colors
        Color[] originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                originalColors[i] = spriteRenderers[i].color;
        }
        
        Color flashColor = Color.red;
        
        // Flash twice
        for (int i = 0; i < 2; i++)
        {
            // Flash red
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                if (sr != null)
                    sr.color = flashColor;
            }
            
            yield return new WaitForSeconds(flashDuration);
            
            // Return to original colors
            for (int j = 0; j < spriteRenderers.Length; j++)
            {
                if (spriteRenderers[j] != null)
                    spriteRenderers[j].color = originalColors[j];
            }
            
            yield return new WaitForSeconds(flashDuration);
        }
        
        isFlashing = false;
    }
    
    public void Kill()
    {
        SetHealth(0);
    }
    
    public void Revive(float healthAmount = -1)
    {
        if (healthAmount < 0)
            healthAmount = maxHealth;
            
        SetHealth(healthAmount);
    }
}