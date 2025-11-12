using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject pauseMenuPanel;
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
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
            GameController.Instance.PauseGame();
    }

    public void HideSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            GameController.Instance.ResumeGame();
    }

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
            GameController.Instance.PauseGame();
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            GameController.Instance.ResumeGame();
    }
}
