using System;
using UnityEngine;

/// <summary>
/// Singleton that stores the currently selected tower prefab and broadcasts selection changes.
/// Assign this to an always-present GameObject in the scene (for example an "UI" or "GameManager" object).
/// </summary>
public class TowerPlacementController : MonoBehaviour
{
    public static TowerPlacementController Instance { get; private set; }

    // Currently selected tower prefab (null means no selection)
    public GameObject SelectedTowerPrefab { get; private set; }

    // Event fired when selection changes. Sends the selected prefab (or null).
    public event Action<GameObject> OnSelectionChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optional: persist between scenes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && SelectedTowerPrefab != null)
        {
            ClearSelection();
        }
    }

    public void SelectTower(GameObject prefab)
    {
        SelectedTowerPrefab = prefab;
        OnSelectionChanged?.Invoke(prefab);
    }

    public void ClearSelection()
    {
        SelectedTowerPrefab = null;
        OnSelectionChanged?.Invoke(null);
    }
}
