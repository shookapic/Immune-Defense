using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    public List<Card> deck = new List<Card>();

    [SerializeField]
    private List<CardData> persistentDeck = new List<CardData>();

    public IReadOnlyList<CardData> ExportedDeck => persistentDeck;

    public List<TextMeshPro> deckSlots;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        // CardManager now remains scene-local; it does not persist across scenes.
    }

    private void Start()
    {
        // When a scene with a CardManager loads, try to import any previously stored deck
        // from the DeckRepository (if it exists).
        if (DeckRepository.Instance != null)
        {
            DeckRepository.Instance.ApplyTo(this);
        }
    }

    public void OnCardSelected(Card selectedCard)
    {
        var data = selectedCard != null ? selectedCard.data : null;
        // Fallback to legacy fields if data is not assigned
        string nameForLog = data != null ? data.towerName : selectedCard?.towerName;

        if (data != null)
        {
            if (persistentDeck.Contains(data))
            {
                persistentDeck.Remove(data);
                deck.Remove(selectedCard); // keep scene list in sync if present
                Debug.Log($"Removed {nameForLog} from deck.");
            }
            else if (persistentDeck.Count < 5)
            {
                persistentDeck.Add(data);
                if (!deck.Contains(selectedCard)) deck.Add(selectedCard);
                Debug.Log($"Added {nameForLog} to deck.");
            }
        }
        else
        {
            // Legacy behavior if CardData is not set
            if (deck.Contains(selectedCard))
            {
                deck.Remove(selectedCard);
                Debug.Log($"Removed {nameForLog} from deck.");
            }
            else if (deck.Count < 5)
            {
                deck.Add(selectedCard);
                Debug.Log($"Added {nameForLog} to deck.");
            }
        }

        RefreshDeckUI();
    }

    void RefreshDeckUI()
    {
        // Met à jour les 5 slots
        if (deckSlots == null || deckSlots.Count == 0) return;

        for (int i = 0; i < deckSlots.Count; i++)
        {
            if (i < persistentDeck.Count)
                deckSlots[i].text = $"{persistentDeck[i].towerName} ({persistentDeck[i].cost})";
            else
                deckSlots[i].text = "-"; // slot vide
        }
    }

    public void RegisterDeckSlots(List<TextMeshPro> newSlots)
    {
        deckSlots = newSlots;
        RefreshDeckUI();
    }

    private void OnDestroy()
    {
        // Before this scene unloads and CardManager is destroyed, push the
        // exported deck to the repository (if available) so other scenes can use it.
        if (Instance == this && DeckRepository.Instance != null)
        {
            DeckRepository.Instance.Store(ExportedDeck);
        }
    }

    // Allow repository (or others) to import a deck into this scene-local manager
    public void ImportPersistentDeck(IEnumerable<CardData> cards)
    {
        persistentDeck.Clear();
        if (cards != null)
        {
            foreach (var c in cards)
            {
                if (c == null) continue;
                if (persistentDeck.Count >= 5) break;
                if (!persistentDeck.Contains(c)) persistentDeck.Add(c);
            }
        }
        RefreshDeckUI();
    }
}
