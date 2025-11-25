using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent, cross-scene storage for the selected deck.
// Lives independently of CardManager, which can remain scene-local for UI.
public class DeckRepository : MonoBehaviour
{
    public static DeckRepository Instance { get; private set; }

    [SerializeField]
    private List<CardData> storedDeck = new List<CardData>();

    [Header("Fallback")]
    [Tooltip("If the deck is empty when a new scene loads, this CardData will be auto-added.")]
    [SerializeField] private CardData fallbackCard;

    public IReadOnlyList<CardData> StoredDeck => storedDeck;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // Store a copy of the selected deck (clamped to 5, deduped).
    public void Store(IEnumerable<CardData> exported)
    {
        storedDeck.Clear();
        if (exported == null) return;
        foreach (var data in exported)
        {
            if (data == null) continue;
            if (storedDeck.Count >= 5) break;
            if (!storedDeck.Contains(data)) storedDeck.Add(data);
        }
        EnsureFallbackIfEmpty();
    }

    // Apply the stored deck to a CardManager in the current scene (if any).
    public void ApplyTo(CardManager manager)
    {
        if (manager == null) return;
        EnsureFallbackIfEmpty();
        manager.ImportPersistentDeck(storedDeck);
    }

    // Scene load hook: ensure a fallback card exists if deck is empty moving into a new scene.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureFallbackIfEmpty();
    }

    // Adds the fallbackCard if deck is empty and fallbackCard is assigned.
    private void EnsureFallbackIfEmpty()
    {
        if (storedDeck.Count == 0 && fallbackCard != null)
        {
            storedDeck.Add(fallbackCard);
        }
    }
}
