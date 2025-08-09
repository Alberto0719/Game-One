using UnityEngine;
using System;

public class BurnEffect : MonoBehaviour
{
    [Header("Burn Settings")]
    [SerializeField] private float burnTickInterval = 1f;
    [SerializeField] private int totalTicks = 3;
    
    private float burnDamagePerTick = 0f;
    private int burnTicksRemaining = 0;
    private float burnTickTimer = 0f;
    private Damageable damageable;
    private HealthBar healthBar;
    
    // Events for notifications
    public event Action OnBurnStarted;
    public event Action OnBurnEnded;
    public event Action<float> OnBurnTick; // damage dealt this tick
      
    void Awake()
    {
        damageable = GetComponent<Damageable>();
        healthBar = GetComponentInChildren<HealthBar>();
        enabled = false; // Start disabled
    }
    
    void Update()
    {
        burnTickTimer += Time.deltaTime;
        
        if (burnTickTimer >= burnTickInterval)
        {
            // Deal burn damage
            if (damageable != null)
            {
                damageable.TakeDamage(burnDamagePerTick);
                OnBurnTick?.Invoke(burnDamagePerTick);
            }
            
            burnTickTimer -= burnTickInterval;
            burnTicksRemaining--;
            
            if (burnTicksRemaining <= 0)
            {
                EndBurn();
            }
        }
    }
    
    public void ApplyBurn(float totalBurnDamage)
    {
        burnDamagePerTick = totalBurnDamage / totalTicks;
        burnTicksRemaining = totalTicks;
        burnTickTimer = 0f;
        enabled = true;
        
        if (healthBar != null)
        {
            healthBar.SetFireStatus(true);
        }
        
        OnBurnStarted?.Invoke();
        // Debug.Log($"BurnEffect applied! {burnDamagePerTick} damage per tick for {burnTicksRemaining} ticks");
    }
    
    private void EndBurn()
    {
        enabled = false;
        burnTicksRemaining = 0;
        burnTickTimer = 0f;
        
        if (healthBar != null)
        {
            healthBar.SetFireStatus(false);
        }
        
        OnBurnEnded?.Invoke();
        // Debug.Log("BurnEffect ended");
    }
    
    public void ForceEnd()
    {
        if (enabled)
        {
            EndBurn();
        }
    }
}