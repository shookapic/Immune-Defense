using UnityEngine;

public class SingleTargetBullet : Bullet
{
    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("OnTrigger");
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TriggerHit(collision.gameObject);
            Destroy(gameObject);
            
        }
    }

}
