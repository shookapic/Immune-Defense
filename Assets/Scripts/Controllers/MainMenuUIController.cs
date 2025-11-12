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
        _deckBuilderPanel.SetActive(false);
        _levelSelectionPanel.SetActive(false);
    }

    public void OnBuildDeckClicked()
    {
        _deckBuilderPanel.SetActive(true);
    }
    
    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
