using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Card : MonoBehaviour
{
    [Header("Card Data (shared across scenes)")]
    public CardData data;

    [Header("Legacy (used if data is null)")]
    public GameObject towerPrefab;
    public string towerName = "Basic Tower";
    public int cost = 50;

    [Header("References")]
    public TextMeshPro nameText;
    public TextMeshPro costText;

    private CardManager cardManager;

    void Start()
    {
        cardManager = CardManager.Instance != null ? CardManager.Instance : FindFirstObjectByType<CardManager>();
        UpdateCardVisuals();
    }

    void UpdateCardVisuals()
    {
        string displayName = data != null ? data.towerName : towerName;
        int displayCost = data != null ? data.cost : cost;

        if (nameText) nameText.text = displayName;
        if (costText) costText.text = displayCost.ToString();
    }

    void OnMouseDown()
    {
        if (cardManager != null)
            cardManager.OnCardSelected(this);
    }
}
