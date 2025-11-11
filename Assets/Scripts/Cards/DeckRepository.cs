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
    }

    // Apply the stored deck to a CardManager in the current scene (if any).
    public void ApplyTo(CardManager manager)
    {
        if (manager == null) return;
        manager.ImportPersistentDeck(storedDeck);
    }
}
