using UnityEngine;

public class AoEBullet : Bullet
{
    public float radius = 2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                    TriggerHit(hit.gameObject);
            }

            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}