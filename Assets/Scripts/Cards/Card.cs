using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Card : MonoBehaviour
{
    [Header("Tower Data")]
    public GameObject towerPrefab;
    public string towerName = "Basic Tower";
    public int cost = 50;

    [Header("References")]
    public TextMeshPro nameText;
    public TextMeshPro costText;

    private CardManager cardManager;

    void Start()
    {
        cardManager = FindFirstObjectByType<CardManager>();
        UpdateCardVisuals();
    }

    void UpdateCardVisuals()
    {
        if (nameText) nameText.text = towerName;
        if (costText) costText.text = cost.ToString();
    }

    void OnMouseDown()
    {
        if (cardManager != null)
            cardManager.OnCardSelected(this);
    }
}
