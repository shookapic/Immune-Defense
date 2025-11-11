using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;


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
    private readonly List<CardData> availableCardData = new List<CardData>();

    [Header("UI")]
    public Button buttonPrefab; // assign a UI Button prefab
    public Transform buttonParent; // container under Canvas where buttons will be instantiated

    // Optional: tint for selected/unselected state
    public Color selectedColor = Color.green;
    public Color normalColor = Color.white;

    private readonly List<Button> spawnedButtons = new List<Button>();
    private List<GameObject> availableTowers = new List<GameObject>();

    void Start()
    {
        if (buttonPrefab == null || buttonParent == null)
        {
            Debug.LogWarning("TowerSelectionUI: buttonPrefab and buttonParent must be assigned in the inspector.");
            return;
        }

        PopulateAvailableTowers();

        if (availableTowers.Count == 0)
        {
            // fallback to inspector array
            if (towerPrefabs != null)
                availableTowers.AddRange(towerPrefabs);
        }

        if (availableTowers.Count == 0)
        {
            Debug.LogWarning("TowerSelectionUI: No towers available (neither deck nor inspector towerPrefabs).");
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

    void PopulateAvailableTowers()
    {
        availableTowers.Clear();
        availableCardData.Clear();
        // Prefer deck selected via repository (cross-scene)
        if (DeckRepository.Instance != null && DeckRepository.Instance.StoredDeck != null && DeckRepository.Instance.StoredDeck.Count > 0)
        {
            foreach (var data in DeckRepository.Instance.StoredDeck)
            {
                if (data == null || data.towerPrefab == null) continue;
                availableTowers.Add(data.towerPrefab);
                availableCardData.Add(data);
            }
            return;
        }

        // Fallback: use inspector-provided prefabs if repository is empty
        if (towerPrefabs != null)
        {
            availableTowers.AddRange(towerPrefabs.Where(p => p != null));
            // no CardData available in this path
        }
    }

    void CreateButtons()
    {
        for (int i = 0; i < availableTowers.Count; i++)
        {
            var prefab = availableTowers[i];
            var btn = Instantiate(buttonPrefab, buttonParent);
            btn.name = "Btn_Tower_" + prefab.name;

            // Set text if button has a Text child
            var txt = btn.GetComponentInChildren<Text>();
            if (txt != null)
            {
                if (i < availableCardData.Count && availableCardData[i] != null)
                {
                    var data = availableCardData[i];
                    txt.text = $"{data.towerName} ({data.cost})";
                }
                else
                {
                    txt.text = prefab.name;
                }
            }

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
            var prefab = i < availableTowers.Count ? availableTowers[i] : null;
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
