using UnityEngine;

public class UIManagerButton : MonoBehaviour
{
    public void ShowSettings()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowSettings();
    }

    public void HideSettings()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.HideSettings();
    }

    public void ShowPauseMenu()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowPauseMenu();
    }

    public void HidePauseMenu()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.HidePauseMenu();
    }
}
