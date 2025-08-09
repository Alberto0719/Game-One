using UnityEngine;

public class CatFollow : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 3f;
    public float stopDistance = 1.5f;

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > stopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position += (Vector3)direction * followSpeed * Time.deltaTime;
        }
    }
}
