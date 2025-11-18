using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Component for individual level panel UI. Displays level info and handles interaction.
/// Attach to your level panel prefab and assign UI references.
/// </summary>
public class LevelPanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component for level name.")]
    [SerializeField] private TextMeshProUGUI levelNameText;
    
    [Tooltip("Text component for level description.")]
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Tooltip("Text component for starting resources display.")]
    [SerializeField] private TextMeshProUGUI resourcesText;
    
    [Tooltip("Text component for starting lives display.")]
    [SerializeField] private TextMeshProUGUI livesText;
    
    [Tooltip("Visual indicator for completed state (optional).")]
    [SerializeField] private GameObject completedIndicator;
    [SerializeField] private TextMeshProUGUI completedText;
    
    [Tooltip("Button to start/load the level.")]
    [SerializeField] private Button playButton;

    private LevelData levelData;

    void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
    }

    public void SetLevelData(LevelData data)
    {
        levelData = data;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (levelData == null) return;

        if (levelNameText != null)
            levelNameText.text = levelData.levelDisplayName;

        if (descriptionText != null)
            descriptionText.text = levelData.description;

        if (completedIndicator != null)
            completedIndicator.GetComponent<Image>().color = new Color(1f, 1f, 1f, levelData.completed ? 1f : 0);

        if (completedText != null)
            completedText.text = levelData.completed ? "Completed" : "Not Completed";
            completedText.color = levelData.completed ? Color.green : Color.red;
    }

    private void OnPlayButtonClicked()
    {
        if (levelData == null)
        {
            Debug.LogWarning("LevelPanel: No level data assigned.");
            return;
        }

        // Set as current level in LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetCurrentLevel(levelData);
        }
    }

    public LevelData GetLevelData() => levelData;

    /// <summary>
    /// Refresh the display (useful if level data changed externally).
    /// </summary>
    public void Refresh()
    {
        UpdateDisplay();
    }
}
