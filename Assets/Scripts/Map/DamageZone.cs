using UnityEngine;
using GameSystems;

public class DamageZone : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Amount of health to remove from the player when an enemy enters.")]
    [SerializeField] private int damageToPlayer = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Damage the player
            if (ResourceManager.Instance != null)
            {
                Destroy(other.gameObject);
                ResourceManager.Instance.RemoveHealthPoints(damageToPlayer);
                Debug.Log($"Enemy reached the end! Player took {damageToPlayer} damage. Current HP: {ResourceManager.Instance.HealthPoints}");
            }
            else
            {
                Debug.LogWarning("ResourceManager not found! Cannot damage player.");
            }

            // Destroy the enemy
        }
        else
        {
            Debug.Log($"[DamageZone] Objet non ennemi entré : {other.name} (Tag: {other.tag})");
        }
    }
}
