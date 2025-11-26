using UnityEngine;

public class MainMenuUIController : MonoBehaviour
{
    [SerializeField] private GameObject _deckBuilderPanel;
    [SerializeField] private GameObject _levelSelectionPanel;

    public void OnPlayClicked()
    {
        _levelSelectionPanel.SetActive(true);
    }

    public void OnStartGameClicked()
    {
        GameController.Instance.StartGame();
    }

    public void OnBackToMainMenuClicked()
    {
        if (_deckBuilderPanel != null)
            _deckBuilderPanel.SetActive(false);
        if (_levelSelectionPanel != null)
            _levelSelectionPanel.SetActive(false);
    }

    public void OnBuildDeckClicked()
    {
        // _deckBuilderPanel.SetActive(true);
        
        GameController.Instance.OpenDeckBuilder();
    }
    
    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
