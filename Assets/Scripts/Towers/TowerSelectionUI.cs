using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;


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
    [Tooltip("If true, will use existing children of buttonParent as slots instead of instantiating new buttons.")]
    public bool useExistingCells = true;

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

        // Decide whether to use existing cells or instantiate
        if (useExistingCells && buttonParent.childCount > 0)
        {
            CreateButtonsFromExistingCells();
        }
        else
        {
            CreateButtons();
        }

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

    void CreateButtonsFromExistingCells()
    {
        spawnedButtons.Clear();

        int cellCount = buttonParent.childCount;
        if (availableTowers.Count > cellCount)
        {
            Debug.LogWarning($"TowerSelectionUI: Not enough cells ({cellCount}) for towers ({availableTowers.Count}). Extra towers will be ignored.");
        }
        if (cellCount > availableTowers.Count)
        {
            // Optional info about unused cells
            Debug.Log($"TowerSelectionUI: {cellCount - availableTowers.Count} unused cells (no tower assigned).");
        }

        int assignCount = Mathf.Min(availableTowers.Count, cellCount);
        for (int i = 0; i < assignCount; i++)
        {
            var prefab = availableTowers[i];
            Transform cell = buttonParent.GetChild(i);
            
            // Ensure cell is active/visible
            cell.gameObject.SetActive(true);
            
            Button btn = cell.GetComponent<Button>();
            if (btn == null)
            {
                btn = cell.gameObject.AddComponent<Button>();
                // Copy selectable colors from prefab if available
                if (buttonPrefab != null)
                {
                    btn.transition = buttonPrefab.transition;
                    btn.colors = buttonPrefab.colors;
                    btn.navigation = buttonPrefab.navigation;
                }
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnButtonClicked(prefab));
            btn.name = "Cell_Tower_" + prefab.name;

            // Try to set text (supports legacy Text or TMP if present)
            var legacyText = btn.GetComponentInChildren<Text>();
            if (legacyText != null)
            {
                if (i < availableCardData.Count && availableCardData[i] != null)
                {
                    var data = availableCardData[i];
                    legacyText.text = $"{data.towerName} ({data.cost})";
                }
                else
                {
                    legacyText.text = prefab.name;
                }
            }
            
            var nameText = btn.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var costText = btn.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();

            if (nameText != null)
            {
                Debug.Log("Setting NameText TMP");
                if (i < availableCardData.Count && availableCardData[i] != null)
                {
                    var data = availableCardData[i];
                    nameText.text = data.towerName.ToString().Replace("Tower", "");
                }
                else
                {
                    nameText.text = prefab.name;
                }
            }
            if (costText != null)
            {
                if (i < availableCardData.Count && availableCardData[i] != null)
                {
                    var data = availableCardData[i];
                    costText.text = $"{data.cost}";
                }
                else
                {
                    costText.text = prefab.name;
                }
            }
            spawnedButtons.Add(btn);
        }

        // Hide unused cells
        for (int i = assignCount; i < cellCount; i++)
        {
            Transform cell = buttonParent.GetChild(i);
            cell.gameObject.SetActive(false);
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
