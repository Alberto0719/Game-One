using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 15f;
    
    private Vector2 direction;
    private float timeAlive;
    private FirePool pool;
    private Camera playerCamera;
    private bool isActive = false;
    private LayerMask targetLayer;
    
    public void Initialize(FirePool firePool)
    {
        pool = firePool;
        playerCamera = Camera.main;
    }
    
    public void Launch(Vector3 startPosition, Vector2 launchDirection, LayerMask targetLayerMask)
    {
        transform.position = startPosition;
        direction = launchDirection.normalized;
        timeAlive = 0f;
        isActive = true;
        targetLayer = targetLayerMask;
        
        // Keep fireball upright (no rotation)
        transform.rotation = Quaternion.identity;
    }
    
    // Legacy method for compatibility
    public void Launch(Vector3 startPosition, Vector2 launchDirection)
    {
        Launch(startPosition, launchDirection, ~0); // Default to all layers
    }
    
    void Update()
    {
        if (!isActive) return;
        
        // Move fireball
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        
        // Update lifetime
        timeAlive += Time.deltaTime;
        
        // Check if should return to pool
        if (timeAlive >= lifetime || IsOffscreen())
        {
            ReturnToPool();
        }
    }
    
    private bool IsOffscreen()
    {
        if (playerCamera == null) return false;
        
        Vector3 screenPos = playerCamera.WorldToScreenPoint(transform.position);
        float margin = 100f; // Extra margin beyond screen edges
        
        return screenPos.x < -margin || 
               screenPos.x > Screen.width + margin || 
               screenPos.y < -margin || 
               screenPos.y > Screen.height + margin;
    }
    
    private void ReturnToPool()
    {
        if (!isActive) return; // Prevent multiple returns
        
        isActive = false;
        pool.ReturnFireball(this);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only hit if fireball is active
        if (!isActive) return;

        // Check if target is on valid layer
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Debug.Log($"Fireball hit valid target: {other.name}");
        }
        else
        {
            Debug.Log($"Fireball hit invalid target: {other.name}");
            return;
        }

        // Check for damageable target
        Damageable damageable = other.GetComponent<Damageable>();
        if (damageable != null)
        {
            // Apply burn effect via BurnEffect component
            BurnEffect burnEffect = other.GetComponent<BurnEffect>();
            if (burnEffect != null)
            {
                burnEffect.ApplyBurn(damage);
            }
            else
            {
                // Fallback: direct damage if no burn effect available
                damageable.TakeDamage(damage);
            }
            
            ReturnToPool();
            return;
        }
        
        // Return to pool on wall collision (remove tag check for now)
        // TODO: Add "Wall" tag to wall objects if needed
        // if (other.CompareTag("Wall"))
        // {
        //     ReturnToPool();
        // }
    }
    
    void OnDisable()
    {
        isActive = false;
    }
}
