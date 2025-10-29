using UnityEngine;


[RequireComponent(typeof(Bullet))]
public abstract class BulletEffect : MonoBehaviour
{
    protected Bullet bullet;

    protected virtual void Awake()
    {
        bullet = GetComponent<Bullet>();
        bullet.OnHit += ApplyEffect;
    }

    protected virtual void OnDestroy()
    {
        bullet.OnHit -= ApplyEffect;
    }

    protected abstract void ApplyEffect(GameObject target);
}