using System;
using UnityEngine;

// ScriptableObject representing a card's immutable data shared across scenes.
// Create assets via: Create -> ImmuneDefense -> Level Data
[CreateAssetMenu(fileName = "LevelData", menuName = "ImmuneDefense/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [Serializable]
    public struct DifficultySettings
    {
        public int startingLives;
        public int startingResources;
        public float enemySpawnInterval;
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    [Header("Level Info")]
    public string levelName = "SkinBarrier";
    public string sceneName = "scene name";
    public string levelDisplayName = "Level 1: Skin Barrier";
    public string description = "A level all about skin and barriers.";
    public bool completed = false;
    public string[] mapChallenges = new string[] { "No Challenges" };
    
    [Header("Difficulty Settings")]
    public DifficultySettings easy = new DifficultySettings { startingLives = 30, startingResources = 150, enemySpawnInterval = 2.5f };
    public DifficultySettings normal = new DifficultySettings { startingLives = 20, startingResources = 100, enemySpawnInterval = 2.0f };
    public DifficultySettings hard = new DifficultySettings { startingLives = 10, startingResources = 75, enemySpawnInterval = 1.5f };

    //public WaveData[] waves;

    /// <summary>
    /// Get settings for the specified difficulty.
    /// </summary>
    public DifficultySettings GetSettings(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => easy,
            Difficulty.Hard => hard,
            _ => normal
        };
    }

    /// <summary>
    /// Convenience: Get starting lives for difficulty.
    /// </summary>
    public int GetStartingLives(Difficulty difficulty) => GetSettings(difficulty).startingLives;

    /// <summary>
    /// Convenience: Get starting resources for difficulty.
    /// </summary>
    public int GetStartingResources(Difficulty difficulty) => GetSettings(difficulty).startingResources;

    /// <summary>
    /// Convenience: Get enemy spawn interval for difficulty.
    /// </summary>
    public float GetEnemySpawnInterval(Difficulty difficulty) => GetSettings(difficulty).enemySpawnInterval;
}
