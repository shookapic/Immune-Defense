using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    public bool IsPaused { get; private set; }

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

    public void StartGame()
    {
        LoadScene("SkinLvlWithTowerPlacement");
    }

    public void PauseGame()
    {
        if (IsPaused) return;

        Time.timeScale = 0f;
        IsPaused = true;
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;

        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else PauseGame();
    }

    public void LoadScene(string sceneName)
    {
        // when changing scenes, make sure time runs
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void OpenDeckBuilder()
    {
        LoadScene("DeckCreator");
    }
}
