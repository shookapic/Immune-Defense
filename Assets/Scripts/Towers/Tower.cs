using System.Collections;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;

    [Header("Tower Settings")]
    public float attackSpeed = 1.0f; // shots per second
    public float range = 5f;         // detection range
    public float turnSpeed = 5f;
    public Transform[] shootPoint;     // point from which bullets are fired
    public bool hasXRotation = false;
    public bool hasYRotation = false;
    public Transform rotatingPartX;
    public Transform rotatingPartY;
    public float multiShootOffsetSeconds = 0.1f;

    private float attackCooldown = 0f;

    void Update()
    {
        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0f)
        {
            StartCoroutine(Shoot());
            attackCooldown = 1f / attackSpeed;
        }
    }

    public bool IsInRange(Vector3 targetPosition, out float distance)
    {
        distance = Mathf.Abs(Vector3.Distance(transform.position, targetPosition));
        return distance <= range;
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (IsInRange(enemy.transform.position, out float distance))
            {
                shortestDistance = distance < shortestDistance ? distance : shortestDistance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    IEnumerator Shoot()
    {
        if (bulletPrefab == null) yield break;

        GameObject target = FindNearestEnemy();

        if (target == null) yield break;
        
        RotateTowards(target.transform.position);
            
        foreach(Transform sp in shootPoint)
        {
            Transform spawn = shootPoint != null ? sp : transform;
            GameObject newBullet = Instantiate(bulletPrefab, spawn.position, spawn.rotation);

            Bullet bulletScript = newBullet.GetComponent<Bullet>();
            if (bulletScript != null)
                bulletScript.SetTarget(target);
            
            yield return new WaitForSeconds(multiShootOffsetSeconds);
        }

    }
    void RotateTowards(Vector3 targetPosition)
    {
        if (rotatingPartX != null)
        {
            Vector3 dir = targetPosition - rotatingPartY.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            rotatingPartY.rotation = Quaternion.Lerp(rotatingPartY.rotation, lookRotation, 1f);
        }

        if (rotatingPartY != null)
        {
            Vector3 dir = targetPosition - rotatingPartX.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            rotatingPartX.rotation = Quaternion.Lerp(rotatingPartX.rotation, lookRotation, 1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
