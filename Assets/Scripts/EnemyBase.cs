using GameSystems;
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
            AudioController.Instance.PlayEnemyDeath();
            
            // Give player 20 coins for killing the enemy
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddBalance(5);
                Debug.Log("[EnemyBase] Enemy killed! Player earned 5 coins.");
            }
            
            Destroy(gameObject);
        }
    }
    
    public void TakeDamage(float damage_receive)
    {
        health -= damage_receive;
    }
}