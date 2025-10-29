using UnityEngine;

public class DamageEffect : BulletEffect
{
    public float damage = 10f;

    protected override void ApplyEffect(GameObject target)
    {
        EnemyHealth health = target.GetComponent<EnemyHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }
}