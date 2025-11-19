using GameSystems;
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
        if (LevelManager.Instance == null || LevelManager.Instance.CurrentLevel == null)
        {
            Debug.LogError("Cannot start game: No current level set in LevelManager.");
            return;
        }

        string sceneName = LevelManager.Instance.CurrentLevel.sceneName;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Cannot start game: Current level has no associated scene name.");
            return;
        }
        
        LoadScene(sceneName);

        Debug.Log("Starting Lives: " + LevelManager.Instance.CurrentLevel.GetStartingLives(LevelManager.Instance.CurrentDifficulty));
        ResourceManager.Instance.SetHealthPoints(LevelManager.Instance.CurrentLevel.GetStartingLives(LevelManager.Instance.CurrentDifficulty));
        ResourceManager.Instance.SetBalance(LevelManager.Instance.CurrentLevel.GetStartingResources(LevelManager.Instance.CurrentDifficulty));
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
