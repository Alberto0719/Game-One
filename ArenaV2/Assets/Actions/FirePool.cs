using UnityEngine;
using System.Collections.Generic;

public class FirePool : MonoBehaviour
{
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private int poolSize = 10;
    
    private Queue<Fireball> availableFireballs = new Queue<Fireball>();
    private List<Fireball> activeFireballs = new List<Fireball>();
    
    public static FirePool Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializePool();
    }
    
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject fireballObj = Instantiate(fireballPrefab, transform);
            Fireball fireball = fireballObj.GetComponent<Fireball>();
            fireball.Initialize(this);
            fireballObj.SetActive(false);
            availableFireballs.Enqueue(fireball);
        }
    }
    
    public Fireball GetFireball()
    {
        if (availableFireballs.Count > 0)
        {
            Fireball fireball = availableFireballs.Dequeue();
            activeFireballs.Add(fireball);
            fireball.gameObject.SetActive(true);
            return fireball;
        }
        
        return null; // Pool exhausted
    }
    
    public void ReturnFireball(Fireball fireball)
    {
        if (activeFireballs.Remove(fireball))
        {
            fireball.gameObject.SetActive(false);
            fireball.transform.SetParent(transform);
            availableFireballs.Enqueue(fireball);
        }
    }
    
    public bool HasAvailableFireball()
    {
        return availableFireballs.Count > 0;
    }
}
