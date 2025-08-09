using UnityEngine;

public class CatHeal : MonoBehaviour
{
    public float healAmount = 20f;
    public float healCooldown = 5f;

    private bool playerInRange = false;
    private Player player;
    private bool canHeal = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.GetComponent<Player>();
            Debug.Log("Press Q to get healed!");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Q) && player != null && canHeal)
        {
            player.Heal(healAmount);
            Debug.Log("Healed by cat!");

            canHeal = false;
            Invoke(nameof(ResetHeal), healCooldown);
        }
    }

    void ResetHeal()
    {
        canHeal = true;
    }
}
