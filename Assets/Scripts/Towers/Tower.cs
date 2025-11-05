using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;

    [Header("Tower Settings")]
    public float attackSpeed = 1.0f; // shots per second
    public float range = 5f;         // detection range
    public Transform shootPoint;     // point from which bullets are fired
    public Transform rotatingPart;

    private float attackCooldown = 0f;

    void Update()
    {
        attackCooldown -= Time.deltaTime;

        GameObject target = FindNearestEnemy();

        if (target != null)
        {
            // RotateTowards(target.transform.position);

            if (attackCooldown <= 0f)
            {
                Shoot(target);
                attackCooldown = 1f / attackSpeed;
            }
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
        if (bulletPrefab == null) return;
        Transform spawn = shootPoint != null ? shootPoint : transform;
        GameObject newBullet = Instantiate(bulletPrefab, spawn.position, spawn.rotation);

        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.SetTarget(target);
    }

    void RotateTowards(Vector3 targetPosition)
    {
        if (rotatingPart == null) return;

        Vector3 direction = (targetPosition - rotatingPart.position).normalized;
        direction.y = 0f;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        rotatingPart.rotation = Quaternion.Lerp(rotatingPart.rotation, lookRotation, Time.deltaTime * 5f);
    }
}
