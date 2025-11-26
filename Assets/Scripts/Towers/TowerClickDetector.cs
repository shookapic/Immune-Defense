using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to each tower prefab to make it clickable.
/// When clicked, registers with TowerSelectionManager.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TowerClickDetector : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Visual feedback when tower is selected.")]
    [SerializeField] private GameObject selectionIndicator;
    
    private bool isSelected = false;

    void Start()
    {
        // Ensure collider for raycasting
        var collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogWarning($"TowerClickDetector on {gameObject.name} requires a Collider component.");
        }

        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);
    }

    void OnMouseDown()
    {
        // Ignore if clicking over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Notify the selection manager
        if (TowerSelectionManager.Instance != null)
        {
            TowerSelectionManager.Instance.SelectTower(gameObject);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selectionIndicator != null)
            selectionIndicator.SetActive(selected);
    }
}
