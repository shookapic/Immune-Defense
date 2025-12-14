using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    // NOUVEAU CHAMP : La destination finale pour tous les ennemis
    [Header("Agent Destination")]
    [Tooltip("The Transform that all spawned enemies will move towards.")]
    public Transform globalTarget; 

    // COMPTEUR pour le suivi des ennemis actifs
    [HideInInspector] 
    public int activeEnemyCount = 0; 
    
    // Méthode appelée par l'ennemi lorsqu'il est détruit (arrivée ou mort)
    public void DecrementActiveEnemyCount()
    {
        activeEnemyCount--;
    }

    [System.Serializable]
    public class WaveEntry
    {
        [Header("Enemy Setup")]
        public GameObject enemyPrefab;
        [Min(1)] public int count = 5;
        [Tooltip("Seconds between spawns for this entry")]
        public float spawnInterval = 0.5f;
        [Tooltip("Movement speed for this enemy (0 = use prefab default)")]
        [Min(0)] public float speed = 0f;

        // [Header("Path (optional)")]
        // public PathDefinition pathOverride; // Conservé pour la complétude si vous l'utilisez ailleurs

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

    void Awake()
    {
        // Auto-create countdown UI if not assigned
        if (countdownUI == null)
        {
            GameObject uiObj = new GameObject("WaveCountdownUI");
            countdownUI = uiObj.AddComponent<WaveCountdownUI>();
            DontDestroyOnLoad(uiObj);
        }
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
        if (globalTarget == null)
        {
            Debug.LogError("[WaveSpawner] ERROR: Global Target is not assigned. Please assign the EndPoint Transform.");
            yield break;
        }

        yield return new WaitForSeconds(delayBeforeFirstWave);
        
        // Caching des prefabs
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
            foreach (var wave in waves)
            {
                // Gestion du DÉCOMPTE AVANT LA VAGUE
                if (countdownUI != null && wave.countdownTime > 0f)
                {
                    countdownUI.StartCountdown(wave.waveName, wave.countdownTime);
                    yield return new WaitForSeconds(wave.countdownTime);
                }

                // Initialisation du compteur d'ennemis
                int totalEnemiesToSpawn = 0;
                foreach (var entry in wave.entries)
                {
                    totalEnemiesToSpawn += entry.count;
                }
                activeEnemyCount = totalEnemiesToSpawn; 

                foreach (var entry in wave.entries)
                {
                    if (entry == null) continue;
                    
                    GameObject prefab;
                    if (!originalPrefabs.TryGetValue(entry, out prefab) || prefab == null)
                    {
                        Debug.LogWarning($"[WaveSpawner] Prefab reference for wave was lost. Skipping entry.");
                        continue;
                    }
                    
                    for (int i = 0; i < entry.count; i++)
                    {
                        Vector3 spawnPosition;
                        if (entry.spawnPoint != null)
                            spawnPosition = entry.spawnPoint.position;
                        else
                            spawnPosition = transform.position; 
                        
                        spawnPosition.y += entry.spawnHeightOffset; 

                        var go = Instantiate(prefab, spawnPosition, Quaternion.identity);

                        var agentScript = go.GetComponent<agentFollow>();

                        if (agentScript == null)
                        {
                            // CRITIQUE : L'ennemi n'a pas le script de suivi -> Échec du spawn
                            Debug.LogError($"[WaveSpawner] ERROR: Prefab '{prefab.name}' is missing agentFollow script. Decrementing active count.");
                            DecrementActiveEnemyCount(); 
                            Destroy(go);
                            continue;
                        }
                        
                        // 1. ASSIGNATION DE LA RÉFÉRENCE AU SPAWNER
                        agentScript.spawner = this;      

                        // 2. CONFIGURATION ET DÉMARRAGE DU MOUVEMENT via la nouvelle méthode
                        // Si entry.speed est à 0 ou négatif, utiliser la valeur par défaut du prefab
                        float finalSpeed = (entry.speed > 0) ? entry.speed : 2f; // Default to 2 if not specified
                        agentScript.ConfigureAndStart(finalSpeed, globalTarget);
                        
                        if (entry.spawnInterval > 0f)
                            yield return new WaitForSeconds(entry.spawnInterval);
                        else
                            yield return null;
                    }
                }
                
                Debug.Log($"[WaveSpawner] Completed spawning wave: {wave.waveName}");
                
                // ATTENDRE QUE TOUS LES ENNEMIS SOIENT DÉTRUITS
                yield return StartCoroutine(WaitForAllEnemiesDestroyed());
                
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
    
    // COROUTINE POUR ATTENDRE QUE activeEnemyCount ATTEIGNE ZÉRO
    IEnumerator WaitForAllEnemiesDestroyed()
    {
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log($"[WaveSpawner] Waiting for {activeEnemyCount} enemies to be destroyed...");
        
        float timeout = 60f; // Timeout après 60 secondes pour éviter blocage infini
        float elapsed = 0f;
        
        while (activeEnemyCount > 0)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
            
            if (elapsed >= timeout)
            {
                Debug.LogError($"[WaveSpawner] TIMEOUT! {activeEnemyCount} enemies still active after {timeout}s. Force continuing to next wave.");
                activeEnemyCount = 0; // Force reset
                break;
            }
        }
        Debug.Log("[WaveSpawner] All enemies destroyed. Starting next phase/wave.");
    }
}