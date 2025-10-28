using UnityEngine;

public class SingleTargetBullet : Bullet
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            OnHit(collision.gameObject);
            Destroy(gameObject);
        }
    }

    protected override void OnHit(GameObject enemy)
    {
        foreach (var effect in effects)
        {
            effect.ApplyEffect(enemy);
        }
    }
}
