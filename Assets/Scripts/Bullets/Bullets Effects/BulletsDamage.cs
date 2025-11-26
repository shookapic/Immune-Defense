using UnityEngine;

public class DamageEffect : BulletEffect
{
    public float damage = 10f;

    protected override void ApplyEffect(GameObject target)
    {
        Debug.Log("Applying Damage Effect");
        EnemyBase health = target.GetComponent<EnemyBase>();
        health.TakeDamage(damage);
    }
}