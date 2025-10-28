using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    public float travelingSpeed = 5f;
    protected GameObject target;
    protected BulletEffect[] effects;

    protected virtual void Start()
    {
        effects = GetComponents<BulletEffect>();
    }

    protected virtual void Update()
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

    protected abstract void OnHit(GameObject enemy);
}
