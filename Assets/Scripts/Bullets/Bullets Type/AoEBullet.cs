using UnityEngine;

public class AoEBullet : Bullet
{
    public float radius = 2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var e in enemies)
            {
                if (e.CompareTag("Enemy"))
                {
                    OnHit(e.gameObject);
                }
            }
            Destroy(gameObject);
        }
    }

    protected override void OnHit(GameObject enemy)
    {
        foreach (var effect in effects)
        {
            effect.ApplyEffect(enemy);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}