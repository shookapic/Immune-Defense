using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Prefab for a level panel (should have Button, Text/TMP components).")]
    [SerializeField] private GameObject levelPanelPrefab;
    
    [Tooltip("Parent transform where level panels will be instantiated.")]
    [SerializeField] private Transform levelPanelParent;
    [SerializeField] private TextMeshProUGUI currentLevelTitleText;
    [SerializeField] private TextMeshProUGUI currentLevelDescriptionText;
    [SerializeField] private TextMeshProUGUI currentLevelResourcesText;
    [SerializeField] private TextMeshProUGUI currentLevelLivesText;
    [SerializeField] private TextMeshProUGUI currentLevelStatusText;
    [SerializeField] private GameObject challengePrefab;
    [SerializeField] private Transform challengeParent;
    
    [Header("Difficulty Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    
    [Header("Difficulty Button Colors")]
    [SerializeField] private Color selectedDifficultyColor = Color.green;
    [SerializeField] private Color normalDifficultyColor = Color.white;
    
    [Header("Settings")]
    [Tooltip("Generate panels on Start.")]
    [SerializeField] private bool generateOnStart = true;

    private readonly List<GameObject> spawnedPanels = new List<GameObject>();
    private readonly List<GameObject> spawnedChallenges = new List<GameObject>();

    void Start()
    {
        SetupDifficultyButtons();
        
        if (generateOnStart)
        {
            GenerateLevelPanels();
        }
        
        RefreshMainPanel(LevelManager.Instance?.CurrentLevel);
    }

    void OnEnable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnCurrentLevelChanged += RefreshMainPanel;
            LevelManager.Instance.OnDifficultyChanged += OnDifficultyChanged;
        }
    }

    void OnDisable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnCurrentLevelChanged -= RefreshMainPanel;
            LevelManager.Instance.OnDifficultyChanged -= OnDifficultyChanged;
        }
    }

    public void GenerateLevelPanels()
    {
        if (levelPanelPrefab == null || levelPanelParent == null)
        {
            Debug.LogWarning("LevelUIManager: levelPanelPrefab and levelPanelParent must be assigned.");
            return;
        }

        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("LevelUIManager: LevelManager.Instance is null. Ensure LevelManager exists in the scene.");
            return;
        }

        if (!LevelManager.Instance.LevelsLoaded)
        {
            Debug.LogWarning("LevelUIManager: Levels not loaded yet. Call LevelManager.LoadLevels() or set loadOnAwake=true.");
            return;
        }

        ClearPanels();

        var levels = LevelManager.Instance.GetAllLevels();
        if (levels == null || levels.Count == 0)
        {
            Debug.LogWarning("LevelUIManager: No levels available from LevelManager.");
            return;
        }

        foreach (var level in levels)
        {
            if (level == null) continue;
            
            var panel = Instantiate(levelPanelPrefab, levelPanelParent);
            panel.name = "Panel_" + level.levelDisplayName;

            // Try to populate the panel with level data
            var levelPanel = panel.GetComponent<LevelPanel>();
            if (levelPanel != null)
            {
                levelPanel.SetLevelData(level);
            }
            else
            {
                Debug.LogWarning($"LevelUIManager: Panel prefab missing LevelPanel component. Add it to customize panel display.");
            }

            spawnedPanels.Add(panel);
        }
    }

    public void ClearPanels()
    {
        foreach (var panel in spawnedPanels)
        {
            if (panel != null)
                Destroy(panel);
        }
        spawnedPanels.Clear();

        // Also clear challenge instances
        foreach (var ch in spawnedChallenges)
        {
            if (ch != null)
                Destroy(ch);
        }
        spawnedChallenges.Clear();
    }

    public void RefreshPanels()
    {
        GenerateLevelPanels();
    }

    public void RefreshMainPanel(LevelData newLevel)
    {
        if (newLevel == null)
        {
            // Clear all fields if no level selected
            if (currentLevelTitleText != null) currentLevelTitleText.text = "No Level Selected";
            if (currentLevelDescriptionText != null) currentLevelDescriptionText.text = "";
            if (currentLevelResourcesText != null) currentLevelResourcesText.text = "";
            if (currentLevelLivesText != null) currentLevelLivesText.text = "";
            if (currentLevelStatusText != null) currentLevelStatusText.text = "";
            return;
        }

        // Get current difficulty settings
        var difficulty = LevelManager.Instance != null ? LevelManager.Instance.CurrentDifficulty : LevelData.Difficulty.Normal;
        var settings = newLevel.GetSettings(difficulty);

        // Update UI fields
        if (currentLevelTitleText != null)
            currentLevelTitleText.text = newLevel.levelDisplayName;

        if (currentLevelDescriptionText != null)
            currentLevelDescriptionText.text = newLevel.description;

        if (currentLevelResourcesText != null)
            currentLevelResourcesText.text = $"E{settings.startingResources}";

        if (currentLevelLivesText != null)
            currentLevelLivesText.text = $"{settings.startingLives} HP";

        if (currentLevelStatusText != null)
        {
            string status = newLevel.completed ? "Completed" : "Not Completed";
            currentLevelStatusText.text = $"{status}";
        }

        UpdateDifficultyButtonVisuals();
        PopulateChallenges(newLevel);
    }

    private void SetupDifficultyButtons()
    {
        if (easyButton != null)
        {
            easyButton.onClick.RemoveAllListeners();
            easyButton.onClick.AddListener(() => OnDifficultyButtonClicked(LevelData.Difficulty.Easy));
        }

        if (normalButton != null)
        {
            normalButton.onClick.RemoveAllListeners();
            normalButton.onClick.AddListener(() => OnDifficultyButtonClicked(LevelData.Difficulty.Normal));
        }

        if (hardButton != null)
        {
            hardButton.onClick.RemoveAllListeners();
            hardButton.onClick.AddListener(() => OnDifficultyButtonClicked(LevelData.Difficulty.Hard));
        }

        UpdateDifficultyButtonVisuals();
    }

    private void OnDifficultyButtonClicked(LevelData.Difficulty difficulty)
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CurrentDifficulty = difficulty;
        }
    }

    private void OnDifficultyChanged(LevelData.Difficulty newDifficulty)
    {
        RefreshMainPanel(LevelManager.Instance?.CurrentLevel);
    }

    private void UpdateDifficultyButtonVisuals()
    {
        if (LevelManager.Instance == null) return;

        var currentDifficulty = LevelManager.Instance.CurrentDifficulty;

        UpdateButtonColor(easyButton, currentDifficulty == LevelData.Difficulty.Easy);
        UpdateButtonColor(normalButton, currentDifficulty == LevelData.Difficulty.Normal);
        UpdateButtonColor(hardButton, currentDifficulty == LevelData.Difficulty.Hard);
    }

    private void UpdateButtonColor(Button button, bool isSelected)
    {
        if (button == null) return;

        var colors = button.colors;
        colors.normalColor = isSelected ? selectedDifficultyColor : normalDifficultyColor;
        button.colors = colors;
    }

    private void PopulateChallenges(LevelData level)
    {
        // Cleanup previous challenges only (not full ClearPanels)
        foreach (var ch in spawnedChallenges)
        {
            if (ch != null) Destroy(ch);
        }
        spawnedChallenges.Clear();

        if (level == null) return;
        if (challengePrefab == null || challengeParent == null) return;
        if (level.mapChallenges == null || level.mapChallenges.Length == 0) return;

        for (int i = 0; i < level.mapChallenges.Length; i++)
        {
            var challenge = level.mapChallenges[i];
            if (string.IsNullOrWhiteSpace(challenge)) continue;
            var go = Instantiate(challengePrefab, challengeParent);
            go.name = "Challenge_" + i; // index-based name for brevity

            // Find first text-type component in children
            TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = challenge;
            }
            else
            {
                var legacy = go.GetComponentInChildren<UnityEngine.UI.Text>();
                if (legacy != null)
                    legacy.text = challenge;
            }

            spawnedChallenges.Add(go);
        }
    }
}
