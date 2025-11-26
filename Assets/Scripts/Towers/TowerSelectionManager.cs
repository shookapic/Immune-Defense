using System;
using UnityEngine;

/// <summary>
/// Singleton that tracks which tower is currently selected.
/// Other systems (like UI) can query this to display upgrade menus.
/// </summary>
public class TowerSelectionManager : MonoBehaviour
{
    public static TowerSelectionManager Instance { get; private set; }

    public GameObject SelectedTower { get; private set; }

    // Events
    public event Action<GameObject> OnTowerSelected;
    public event Action OnTowerDeselected;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optional: persist across scenes if needed
        // DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Deselect if clicking empty space
        if (Input.GetMouseButtonDown(0))
        {
            // Check if we clicked on nothing (ground, etc.)
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // If we hit something that's not a tower, deselect
                if (!hit.collider.CompareTag("Tower"))
                {
                    // Only deselect if not clicking UI
                    if (UnityEngine.EventSystems.EventSystem.current != null && 
                        !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    {
                        DeselectTower();
                    }
                }
            }
        }

        // Optional: ESC to deselect
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectTower();
        }
    }

    public void SelectTower(GameObject tower)
    {
        if (tower == null) return;

        // Deselect previous
        if (SelectedTower != null && SelectedTower != tower)
        {
            var prevDetector = SelectedTower.GetComponent<TowerClickDetector>();
            if (prevDetector != null)
                prevDetector.SetSelected(false);
        }

        SelectedTower = tower;

        // Update visual
        var detector = tower.GetComponent<TowerClickDetector>();
        if (detector != null)
            detector.SetSelected(true);

        OnTowerSelected?.Invoke(tower);
    }

    public void DeselectTower()
    {
        if (SelectedTower == null) return;

        var detector = SelectedTower.GetComponent<TowerClickDetector>();
        if (detector != null)
            detector.SetSelected(false);

        SelectedTower = null;
        OnTowerDeselected?.Invoke();
    }

    public TowerInfo GetSelectedTowerInfo()
    {
        return SelectedTower != null ? SelectedTower.GetComponent<TowerInfo>() : null;
    }

    public TowerUpgradeProgress GetSelectedTowerUpgradeProgress()
    {
        return SelectedTower != null ? SelectedTower.GetComponent<TowerUpgradeProgress>() : null;
    }
}
