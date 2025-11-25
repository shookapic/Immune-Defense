using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Prefabs (for auto-instantiation)")]
    [SerializeField] private GameObject settingsPanelPrefab;
    [SerializeField] private GameObject pauseMenuPanelPrefab;


    private GameObject settingsPanel;
    private GameObject pauseMenuPanel;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowSettings()
    {
        EnsureSettingsPanelExists();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            if (GameController.Instance != null)
                GameController.Instance.PauseGame();
        }
    }

    public void HideSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            if (GameController.Instance != null)
                GameController.Instance.ResumeGame();
        }
    }

    public void ShowPauseMenu()
    {
        EnsurePauseMenuPanelExists();
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            if (GameController.Instance != null)
                GameController.Instance.PauseGame();
        }
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            if (GameController.Instance != null)
                GameController.Instance.ResumeGame();
        }
    }

    private void EnsureSettingsPanelExists()
    {
        if (settingsPanel != null) return;

        // Try to find existing panel in scene
        var canvas = FindCanvasInScene();
        if (canvas != null)
        {
            var existingPanel = canvas.transform.Find("SettingsPanel");
            if (existingPanel != null)
            {
                settingsPanel = existingPanel.gameObject;
                return;
            }
        }

        // Instantiate from prefab if available
        if (settingsPanelPrefab != null && canvas != null)
        {
            settingsPanel = Instantiate(settingsPanelPrefab, canvas.transform);
            settingsPanel.name = "SettingsPanel";
            settingsPanel.SetActive(false);
        }
        else if (settingsPanelPrefab == null)
        {
            Debug.LogWarning("UIManager: settingsPanelPrefab is not assigned and no SettingsPanel found in scene.");
        }
    }

    private void EnsurePauseMenuPanelExists()
    {
        if (pauseMenuPanel != null) return;

        // Try to find existing panel in scene
        var canvas = FindCanvasInScene();
        if (canvas != null)
        {
            var existingPanel = canvas.transform.Find("PauseMenuPanel");
            if (existingPanel != null)
            {
                pauseMenuPanel = existingPanel.gameObject;
                return;
            }
        }

        // Instantiate from prefab if available
        if (pauseMenuPanelPrefab != null && canvas != null)
        {
            pauseMenuPanel = Instantiate(pauseMenuPanelPrefab, canvas.transform);
            pauseMenuPanel.name = "PauseMenuPanel";
            WirePauseMenuButtons();
            pauseMenuPanel.SetActive(false);
        }
        else if (pauseMenuPanelPrefab == null)
        {
            Debug.LogWarning("UIManager: pauseMenuPanelPrefab is not assigned and no PauseMenuPanel found in scene.");
        }
    }

    private void WirePauseMenuButtons()
    {
        if (pauseMenuPanel == null) return;

        // Get all buttons in children
        var buttons = pauseMenuPanel.GetComponentsInChildren<Button>(true);

        foreach (var btn in buttons)
        {
            if (btn.name.Contains("Exit"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => Application.Quit());
                #if UNITY_EDITOR
                btn.onClick.AddListener(() => UnityEditor.EditorApplication.isPlaying = false);
                #endif
            }
            else if (btn.name.Contains("Continue"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(HidePauseMenu);
            }
        }
    }

    private Canvas FindCanvasInScene()
    {
        // Try to find Canvas in current scene
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("UIManager: No Canvas found in scene. Cannot instantiate UI panels.");
        }
        return canvas;
    }
}
