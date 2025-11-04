using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spawns UI buttons for each tower prefab and notifies the TowerPlacementController when one is selected.
/// Usage:
/// - Create a Canvas and a Panel to hold buttons.
/// - Create a simple Button prefab (with a Text child) and assign it to buttonPrefab.
/// - Assign the buttonParent to the panel's transform and populate towerPrefabs in the inspector.
/// </summary>
public class TowerSelectionUI : MonoBehaviour
{
    [Header("Data")]
    public GameObject[] towerPrefabs;

    [Header("UI")]
    public Button buttonPrefab; // assign a UI Button prefab
    public Transform buttonParent; // container under Canvas where buttons will be instantiated

    // Optional: tint for selected/unselected state
    public Color selectedColor = Color.green;
    public Color normalColor = Color.white;

    private readonly List<Button> spawnedButtons = new List<Button>();

    void Start()
    {
        if (buttonPrefab == null || buttonParent == null)
        {
            Debug.LogWarning("TowerSelectionUI: buttonPrefab and buttonParent must be assigned in the inspector.");
            return;
        }

        CreateButtons();

        // Subscribe to selection changes so we can update visuals
        if (TowerPlacementController.Instance != null)
            TowerPlacementController.Instance.OnSelectionChanged += OnSelectionChanged;
    }

    void OnDestroy()
    {
        if (TowerPlacementController.Instance != null)
            TowerPlacementController.Instance.OnSelectionChanged -= OnSelectionChanged;
    }

    void CreateButtons()
    {
        for (int i = 0; i < towerPrefabs.Length; i++)
        {
            var prefab = towerPrefabs[i];
            var btn = Instantiate(buttonPrefab, buttonParent);
            btn.name = "Btn_Tower_" + prefab.name;

            // Set text if button has a Text child
            var txt = btn.GetComponentInChildren<Text>();
            if (txt != null)
                txt.text = prefab.name;

            // Capture local variable for lambda
            btn.onClick.AddListener(() => OnButtonClicked(prefab));

            spawnedButtons.Add(btn);
        }

        UpdateButtonVisuals();
    }

    void OnButtonClicked(GameObject prefab)
    {
        if (TowerPlacementController.Instance == null)
            return;

        // Toggle selection: if already selected, clear.
        if (TowerPlacementController.Instance.SelectedTowerPrefab == prefab)
            TowerPlacementController.Instance.ClearSelection();
        else
            TowerPlacementController.Instance.SelectTower(prefab);
    }

    void OnSelectionChanged(GameObject selected)
    {
        UpdateButtonVisuals();
    }

    void UpdateButtonVisuals()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            var btn = spawnedButtons[i];
            var prefab = towerPrefabs[i];
            var colors = btn.colors;
            if (TowerPlacementController.Instance != null && TowerPlacementController.Instance.SelectedTowerPrefab == prefab)
            {
                // simple highlight by changing normal color via colors
                colors.normalColor = selectedColor;
            }
            else
            {
                colors.normalColor = normalColor;
            }
            btn.colors = colors;
        }
    }
}
