using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] Button quitButton;
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject hostPanel;
    [SerializeField] GameObject joinPanel;

    private void Start()
    {
        hostButton.onClick.AddListener(Host);
        joinButton.onClick.AddListener(Join);
        quitButton.onClick.AddListener(Quit);
    }

    void Host()
    {
        DisableMainPanel();
        if (hostPanel != null) hostPanel.SetActive(true);
    }

    void Join()
    {
        DisableMainPanel();
        if (joinPanel != null) joinPanel.SetActive(true);
    }

    void Quit()
    {
#if UNITY_EDITOR
        // Stop Play Mode if running in the Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // Quit the built application
    Application.Quit();
#endif
    }

    private void DisableMainPanel()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);
    }

}
