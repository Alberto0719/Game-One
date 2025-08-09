using UnityEngine;
using System.Collections;

public interface IAgent
{
    Transform GetTarget();
    void SetFacing(Vector3 targetPoint);

    Transform player { get; }
    Rigidbody2D rb { get; }
    float moveSpeed { get; }
    Transform transform { get; }
    
    Coroutine StartCoroutine(IEnumerator routine);
}