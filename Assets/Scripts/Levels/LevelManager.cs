using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Load Settings")]
    [Tooltip("Folder under Resources containing LevelData assets.")]
    [SerializeField] private string resourcesFolder = "LevelObjects"; // Ensure a Resources/LevelObjects folder exists
    [Tooltip("Load levels on Awake.")] public bool loadOnAwake = true;
    [Tooltip("Levels assigned directly in the Inspector. If not empty, these will be used instead of loading from Resources.")]
    [SerializeField] private List<LevelData> inspectorLevels = new List<LevelData>();

    [Header("Difficulty Settings")]
    [Tooltip("Current difficulty setting for all levels.")]
    [SerializeField] private LevelData.Difficulty currentDifficulty = LevelData.Difficulty.Normal;

    private readonly List<LevelData> levels = new List<LevelData>();
    private Dictionary<string, LevelData> levelBySceneName = new Dictionary<string, LevelData>();
    public LevelData CurrentLevel { get; private set; }
    public bool LevelsLoaded { get; private set; }
    public LevelData.Difficulty CurrentDifficulty 
    { 
        get => currentDifficulty; 
        set 
        {
            if (currentDifficulty != value)
            {
                currentDifficulty = value;
                OnDifficultyChanged?.Invoke(currentDifficulty);
            }
        }
    }

    // Events
    public event System.Action<IReadOnlyList<LevelData>> OnLevelsLoaded;
    public event System.Action<LevelData> OnCurrentLevelChanged;
    public event System.Action<LevelData.Difficulty> OnDifficultyChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadOnAwake)
        {
            if (inspectorLevels != null && inspectorLevels.Count > 0)
            {
                LoadLevelsFromList(inspectorLevels);
            }
            else
            {
                LoadLevels();
            }
        }

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Auto-set current level if scene name matches a referenceSceneName
        if (!LevelsLoaded) return;
        var match = levels.FirstOrDefault(l => l.sceneName == scene.name);
        if (match != null)
        {
            SetCurrentLevel(match);
        }
    }

    public void LoadLevels()
    {
        levels.Clear();
        levelBySceneName.Clear();

        var loaded = Resources.LoadAll<LevelData>(resourcesFolder);
        if (loaded == null || loaded.Length == 0)
        {
            Debug.LogWarning($"LevelManager: No LevelData assets found in Resources/{resourcesFolder}. Create them via Create -> ImmuneDefense -> Level Data.");
            LevelsLoaded = false;
            return;
        }

        levels.AddRange(loaded);
        foreach (var ld in levels)
        {
            if (!string.IsNullOrEmpty(ld.sceneName) && !levelBySceneName.ContainsKey(ld.sceneName))
            {
                levelBySceneName.Add(ld.sceneName, ld);
            }
        }
        LevelsLoaded = true;
        OnLevelsLoaded?.Invoke(levels);
    }

    /// <summary>
    /// Initialize from a provided list (e.g., assigned in Inspector).
    /// </summary>
    public void LoadLevelsFromList(IEnumerable<LevelData> list)
    {
        levels.Clear();
        levelBySceneName.Clear();

        if (list == null)
        {
            Debug.LogWarning("LevelManager: Provided level list is null.");
            LevelsLoaded = false;
            return;
        }

        foreach (var ld in list)
        {
            if (ld == null) continue;
            levels.Add(ld);
        }

        if (levels.Count == 0)
        {
            Debug.LogWarning("LevelManager: No LevelData assets assigned in Inspector list.");
            LevelsLoaded = false;
            return;
        }

        foreach (var ld in levels)
        {
            if (!string.IsNullOrEmpty(ld.sceneName) && !levelBySceneName.ContainsKey(ld.sceneName))
            {
                levelBySceneName.Add(ld.sceneName, ld);
            }
        }

        LevelsLoaded = true;
        OnLevelsLoaded?.Invoke(levels);
    }

    /// <summary>
    /// Programmatically set levels and rebuild indices/events.
    /// </summary>
    public void SetLevels(IEnumerable<LevelData> newLevels)
    {
        LoadLevelsFromList(newLevels);
    }

    public IReadOnlyList<LevelData> GetAllLevels() => levels;

    public LevelData GetLevelBySceneName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return null;
        levelBySceneName.TryGetValue(sceneName, out var ld);
        return ld;
    }

    public void SetCurrentLevel(LevelData level)
    {
        if (level == null)
        {
            Debug.LogWarning("LevelManager: Attempted to set current level to null.");
            return;
        }
        if (CurrentLevel == level) return;
        CurrentLevel = level;
        OnCurrentLevelChanged?.Invoke(CurrentLevel);
    }

    public bool SetCurrentLevelBySceneName(string sceneName)
    {
        var ld = GetLevelBySceneName(sceneName);
        if (ld == null)
        {
            Debug.LogWarning($"LevelManager: No level found for scene '{sceneName}'.");
            return false;
        }
        SetCurrentLevel(ld);
        return true;
    }

    public void MarkLevelCompleted(LevelData level)
    {
        if (level == null)
        {
            Debug.LogWarning("LevelManager: Cannot mark null level as completed.");
            return;
        }
        level.completed = true;
    }

    public void MarkCurrentLevelCompleted()
    {
        if (CurrentLevel == null)
        {
            Debug.LogWarning("LevelManager: No current level to mark completed.");
            return;
        }
        CurrentLevel.completed = true;
    }

    public LevelData GetNextUncompletedLevel()
    {
        return levels.FirstOrDefault(l => !l.completed);
    }

    public bool AllLevelsCompleted() => levels.Count > 0 && levels.All(l => l.completed);

    // Optional: reload level definitions (e.g., if dynamic changes)
    public void ReloadLevels()
    {
        if (inspectorLevels != null && inspectorLevels.Count > 0)
        {
            LoadLevelsFromList(inspectorLevels);
        }
        else
        {
            LoadLevels();
        }
    }

    /// <summary>
    /// Convenience: set completion by scene name.
    /// </summary>
    public bool SetCompleted(string sceneName, bool completed = true)
    {
        var ld = GetLevelBySceneName(sceneName);
        if (ld == null) return false;
        ld.completed = completed;
        return true;
    }

    /// <summary>
    /// Get current difficulty settings for a level.
    /// </summary>
    public LevelData.DifficultySettings GetCurrentSettings(LevelData level)
    {
        if (level == null) return default;
        return level.GetSettings(CurrentDifficulty);
    }

    /// <summary>
    /// Get current difficulty settings for the current level.
    /// </summary>
    public LevelData.DifficultySettings GetCurrentLevelSettings()
    {
        return GetCurrentSettings(CurrentLevel);
    }
}
