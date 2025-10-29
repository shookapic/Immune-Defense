using UnityEngine;

public class SlowEffect : BulletEffect
{
    public float slowAmount = 0.5f;     // 50% de la vitesse d’origine
    public float slowDuration = 2f;     // durée en secondes

    protected override void ApplyEffect(GameObject target)
    {
        EnemyMovement movement = target.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            Debug.Log("Slow applied");
            // movement.ApplySlow(slowAmount, slowDuration);
        }
    }
}