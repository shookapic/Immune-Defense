using UnityEngine;

public class SingleTargetBullet : Bullet
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            TriggerHit(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
