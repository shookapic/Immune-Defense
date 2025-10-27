using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("References")]
    public GameObject bullet = null;

    [Header("Tower Settings")]
    public float attackSpeed = 1.0f; // shot/seconds
    public float range = 5f;         // detection range

    private float attackCooldown = 0f;

    void Update()
    {
        attackCooldown -= Time.deltaTime;

        GameObject target = FindNearestEnemy();

        // Si on a une cible et que le cooldown est fini → tirer
        if (target != null && attackCooldown <= 0f)
        {
            Shoot(target);
            attackCooldown = 1f / attackSpeed; // reset cooldown
        }
    }

    public bool IsInRange(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) <= range;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance && distance <= range)
            {
                shortestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    void Shoot(GameObject target)
    {
        if (bullet == null)
        {
            return;
        }

        GameObject newBullet = Instantiate(bullet, transform.position, Quaternion.identity);
        Bullet bulletScript = newBullet.GetComponent<Bullet>();

        if (bulletScript != null)
            bulletScript.SetTarget(target);
    }
}