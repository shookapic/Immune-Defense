using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public List<Card> deck = new List<Card>();

    public void OnCardSelected(Card selectedCard)
    {
        if (deck.Contains(selectedCard))
        {
            deck.Remove(selectedCard);
            Debug.Log($"Removed {selectedCard.towerName} from deck.");
        }
        else
        {
            deck.Add(selectedCard);
            Debug.Log($"Added {selectedCard.towerName} to deck.");
        }
    }
}