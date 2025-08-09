using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Transform healthValue;
    private Transform statusFire;
    private Vector3 originalScale;
    
    void Start()
    {
        // Find HealthFill child (now nested under HealthAnchor)
        Transform healthAnchor = transform.Find("HealthAnchor");
        if (healthAnchor != null)
        {
            healthValue = healthAnchor.Find("HealthFill");
            // Debug.Log($"Found HealthAnchor, looking for HealthFill: {healthValue != null}");
        }
        else
        {
            // Debug.Log("HealthAnchor not found, trying direct search");
            // Fallback: try to find HealthFill directly
            healthValue = transform.Find("HealthFill");
        }
        
        // Find StatusFire child
        statusFire = transform.Find("StatusFire");
        if (statusFire != null)
        {
            statusFire.gameObject.SetActive(false); // Start inactive
            // Debug.Log("HealthBar found StatusFire child");
        }
        else
        {
            // Debug.Log("HealthBar: StatusFire child not found!");
        }
        
        if (healthValue != null)
        {
            originalScale = healthValue.localScale;
            // Debug.Log($"HealthBar found HealthFill child, original scale: {originalScale}");
        }
        else
        {
            // Debug.Log("HealthBar: HealthFill child not found!");
        }
    }
    
    public void SetPctHealth(float healthPercentage)
    {
        if (healthValue == null) return;
        
        // Clamp percentage between 0 and 1
        healthPercentage = Mathf.Clamp01(healthPercentage);
        
        // With left pivot, we only need to scale - no position adjustment needed!
        Vector3 newScale = originalScale;
        newScale.x = originalScale.x * healthPercentage;
        healthValue.localScale = newScale;
        
        // Debug.Log($"HealthBar: {healthPercentage:F2}%, Scale: {newScale.x:F2}");
    }
    
    public void SetFireStatus(bool isOnFire)
    {
        if (statusFire != null)
        {
            statusFire.gameObject.SetActive(isOnFire);
            // Debug.Log($"Fire status: {(isOnFire ? "ON" : "OFF")}");
        }
    }
}
