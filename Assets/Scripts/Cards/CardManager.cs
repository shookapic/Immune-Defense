using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class CardManager : MonoBehaviour
{
    public List<Card> deck = new List<Card>();

    public List<TextMeshPro> deckSlots;

    public void OnCardSelected(Card selectedCard)
    {
        if (deck.Contains(selectedCard))
        {
            deck.Remove(selectedCard);
            Debug.Log($"Removed {selectedCard.towerName} from deck.");
        }
        else if (deck.Count < 5)
        {
            deck.Add(selectedCard);
            Debug.Log($"Added {selectedCard.towerName} to deck.");
        }

        RefreshDeckUI();
    }

    void RefreshDeckUI()
    {
        // Met à jour les 5 slots
        for (int i = 0; i < deckSlots.Count; i++)
        {
            if (i < deck.Count)
                deckSlots[i].text = $"{deck[i].towerName} ({deck[i].cost})";
            else
                deckSlots[i].text = "-"; // slot vide
        }
    }
}
