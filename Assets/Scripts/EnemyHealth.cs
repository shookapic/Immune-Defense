using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health Settings")]
    public float health = 50;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    public void OnHit(float damage_receive)
    {
        health -= damage_receive;
    }
}
