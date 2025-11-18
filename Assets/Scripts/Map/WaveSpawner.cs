using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class WaveEntry
    {
        [Header("Enemy Setup")]
        public GameObject enemyPrefab;
        [Min(1)] public int count = 5;
        [Tooltip("Seconds between spawns for this entry")]
        public float spawnInterval = 0.5f;

        [Header("Path (optional)")]
        public PathDefinition pathOverride;

        [Header("Spawn (optional)")]
        public Transform spawnPoint;

        [Header("Spawn Settings")]
        [Tooltip("Offset Y position to prevent enemies from spawning inside ground or rocks")]
        public float spawnHeightOffset = 0.5f;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";
        public List<WaveEntry> entries = new List<WaveEntry>();
        [Tooltip("Countdown time before this wave starts")]
        [Min(0)] public float countdownTime = 5f;
        [Tooltip("Delay after this wave finishes before next wave starts")]
        [Min(0)] public float delayAfterWave = 2f;
    }

    [Header("Waves")]
    public List<Wave> waves = new List<Wave>();

    [Header("Looping")]
    public bool loopWaves = false;
    public float delayBeforeFirstWave = 1f;

    [Header("UI")]
    [Tooltip("Leave empty to auto-create UI at runtime")]
    public WaveCountdownUI countdownUI;
    [Tooltip("Leave empty to auto-create UI at runtime")]
    public WaveProgressUI progressUI;

    private Coroutine runRoutine;
    private int currentWaveEnemyCount;

    void Awake()
    {
        // Auto-create countdown UI if not assigned
        if (countdownUI == null)
        {
            GameObject uiObj = new GameObject("WaveCountdownUI");
            countdownUI = uiObj.AddComponent<WaveCountdownUI>();
            DontDestroyOnLoad(uiObj);
        }

        // Don't create progress UI - removed per user request
    }

    void OnEnable()
    {
        runRoutine = StartCoroutine(RunWaves());
    }

    void OnDisable()
    {
        if (runRoutine != null) StopCoroutine(runRoutine);
    }

    IEnumerator RunWaves()
{
    yield return new WaitForSeconds(delayBeforeFirstWave);

    // ✅ Cache the original prefab references before entering the loop
    Dictionary<WaveEntry, GameObject> originalPrefabs = new Dictionary<WaveEntry, GameObject>();
    foreach (var wave in waves)
    {
        foreach (var entry in wave.entries)
        {
            if (entry.enemyPrefab != null)
                originalPrefabs[entry] = entry.enemyPrefab;
        }
    }

    do
    {
        Debug.Log("[WaveSpawner] Starting wave cycle.");

        foreach (var wave in waves)
        {
            // Show countdown UI before wave starts
            if (countdownUI != null && wave.countdownTime > 0f)
            {
                Debug.Log($"[WaveSpawner] Starting countdown for '{wave.waveName}' ({wave.countdownTime}s)");
                countdownUI.StartCountdown(wave.waveName, wave.countdownTime);
                yield return new WaitForSeconds(wave.countdownTime);
            }

            Debug.Log($"[WaveSpawner] Starting wave: {wave.waveName}");

            // Calculate total enemy count for this wave
            currentWaveEnemyCount = 0;
            foreach (var entry in wave.entries)
            {
                currentWaveEnemyCount += entry.count;
            }

            foreach (var entry in wave.entries)
            {
                // ✅ Restore prefab reference in case Unity lost it
                if (entry == null) continue;
                if (!originalPrefabs.TryGetValue(entry, out GameObject prefab) || prefab == null)
                {
                    Debug.LogWarning($"[WaveSpawner] Prefab reference for wave '{wave.waveName}' was lost. Skipping entry.");
                    continue;
                }

                for (int i = 0; i < entry.count; i++)
                {
                    // ✅ Determine spawn position
                    Vector3 spawnPosition;

                    if (entry.spawnPoint != null)
                        spawnPosition = entry.spawnPoint.position;
                    else
                    {
                        GameObject[] pathObjects = GameObject.FindGameObjectsWithTag("Path");
                        if (pathObjects.Length > 0)
                        {
                            GameObject leftmost = pathObjects[0];
                            foreach (var obj in pathObjects)
                            {
                                if (obj.transform.position.x < leftmost.transform.position.x)
                                    leftmost = obj;
                            }
                            spawnPosition = leftmost.transform.position;
                        }
                        else
                        {
                            spawnPosition = transform.position;
                            Debug.LogWarning("[WaveSpawner] No Path objects found in scene. Using spawner position.");
                        }
                    }

                    spawnPosition.y += entry.spawnHeightOffset;

                    // ✅ Instantiate from *original prefab asset*, never from runtime clone
                    var go = Instantiate(prefab, spawnPosition, Quaternion.identity);

                    if (go == null)
                    {
                        Debug.LogWarning("[WaveSpawner] Instantiate failed (prefab may be missing).");
                        continue;
                    }

                    var enemy = go.GetComponent<EnemyPath>() ?? go.GetComponentInChildren<EnemyPath>();
                    if (enemy == null)
                        Debug.LogWarning($"[WaveSpawner] '{prefab.name}' has no EnemyPath component.");
                    else
                        Debug.Log($"[WaveSpawner] Spawned '{prefab.name}' successfully at {spawnPosition}.");

                    if (entry.spawnInterval > 0f)
                        yield return new WaitForSeconds(entry.spawnInterval);
                    else
                        yield return null;
                }
            }

            Debug.Log($"[WaveSpawner] Completed spawning wave: {wave.waveName}");
            
            // Wait for all enemies to be destroyed before continuing
            yield return StartCoroutine(WaitForAllEnemiesDestroyed());
            
            Debug.Log($"[WaveSpawner] All enemies destroyed for wave: {wave.waveName}");

            if (wave.delayAfterWave > 0f)
                yield return new WaitForSeconds(wave.delayAfterWave);
        }

        if (loopWaves)
        {
            Debug.Log("[WaveSpawner] Wave cycle completed — restarting loop.");
            yield return new WaitForSeconds(1f);
        }

    } while (loopWaves);
}

    IEnumerator WaitForAllEnemiesDestroyed()
    {
        // Wait a frame to ensure all spawned enemies are registered
        yield return null;
        
        while (true)
        {
            // Find all enemies in the scene
            EnemyPath[] enemies = FindObjectsByType<EnemyPath>(FindObjectsSortMode.None);
            
            if (enemies.Length == 0)
            {
                Debug.Log("[WaveSpawner] No more enemies in scene.");
                break;
            }
            
            Debug.Log($"[WaveSpawner] Waiting for {enemies.Length} enemies to be destroyed...");
            yield return new WaitForSeconds(0.5f); // Check every 0.5 seconds
        }
    }

}
