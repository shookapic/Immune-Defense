using UnityEngine;
using System;

public abstract class Bullet : MonoBehaviour
{
    public float travelingSpeed = 5f;
    protected GameObject target;
    // protected BulletEffect[] effects;

    public event Action<GameObject> OnHit;

    protected virtual void Start()
    {
        // effects = GetComponents<BulletEffect>();
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        MoveTowardsTarget();
    }

    protected void MoveTowardsTarget()
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * travelingSpeed * Time.deltaTime;
    }

    public void SetTarget(GameObject enemyTarget)
    {
        target = enemyTarget;
    }

    protected void TriggerHit(GameObject enemy)
    {
        OnHit?.Invoke(enemy);
    }

    // protected abstract void OnHit(GameObject enemy);
}
