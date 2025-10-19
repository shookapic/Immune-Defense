using UnityEngine;

public class DamageEffect : BulletEffect
{
    public float damage = 10f;

    public override void ApplyEffect(GameObject target)
    {
        EnemyHealth health = target.GetComponent<EnemyHealth>();
        if (health != null)
        {
            Debug.Log("Damage dealt");
            health.OnHit(damage);
        }
    }
}