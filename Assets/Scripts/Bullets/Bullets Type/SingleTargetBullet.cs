using UnityEngine;

public class SingleTargetBullet : Bullet
{
    private float damage = 0f;

    override protected void Start()
    {
        base.Start();

        BulletEffect damageEffect = GetComponent<DamageEffect>();
        if (damageEffect != null)
        {
            damage = ((DamageEffect)damageEffect).damage;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("OnTrigger, damage: " + damage);
        if (collision.gameObject != null && collision.gameObject.CompareTag("Enemy"))
        {
            //TriggerHit(collision.gameObject);
            collision.gameObject.GetComponent<EnemyBase>().TakeDamage(damage);
            //Destroy(gameObject);
        }
    }

}
