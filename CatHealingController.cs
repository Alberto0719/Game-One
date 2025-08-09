using UnityEngine;
using UnityEngine.InputSystem;

public class CatHealingController : MonoBehaviour
{
    [Tooltip("Key to press for healing")]
    public Key healKey = Key.E;

    [Tooltip("Player to heal")]
    public Damageable playerDamageable;

    [Tooltip("Max distance to allow healing")]
    public float healingRadius = 3f;

    private Transform playerTransform;

    void Start()
    {
        if (playerDamageable != null)
        {
            playerTransform = playerDamageable.transform;
        }
    }

    void Update()
    {
        if (Keyboard.current[healKey].wasPressedThisFrame)
        {
            TryHeal();
        }
    }

    void TryHeal()
    {
        if (playerDamageable != null && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= healingRadius)
            {
                playerDamageable.Heal(20f); // Change amount if needed
                Debug.Log("🐱 Cat healed the player!");
            }
            else
            {
                Debug.Log("⛔ Too far to heal.");
            }
        }
    }
}
