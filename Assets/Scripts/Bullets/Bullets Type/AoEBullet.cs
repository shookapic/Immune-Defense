using UnityEngine;

public class AoEBullet : Bullet
{
    [Header("AoE Settings")]
    public float radius = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    TriggerHit(hit.gameObject);
                }
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
