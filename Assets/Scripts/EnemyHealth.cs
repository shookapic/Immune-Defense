using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health Settings")]
    public float health = 50;
    
    private bool isDead = false;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0 && !isDead)
        {
            isDead = true;
            OnDeath();
            Destroy(gameObject);
        }
    }
    
    public void TakeDamage(float damage_receive)
    {
        health -= damage_receive;
    }

    private void OnDeath()
    {
        // Enemy death - no UI notification needed
    }
}
