using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Base Enemy Settings")]
    public float health = 50;
    public float money_ammount = 100;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            GameSystems.ResourceManager.Instance.AddBalance((int)money_ammount);
            Destroy(gameObject);
        }
    }
    
    public void TakeDamage(float damage_receive)
    {
        health -= damage_receive;
    }
}