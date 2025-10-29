using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
   
    [Header("Enemy Settings")]
    public float moveSpeed = 2f; // vitesse de déplacement en unités par seconde

    void Update()
    {
        // Déplacement vers le haut (axe Y)
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }
}
