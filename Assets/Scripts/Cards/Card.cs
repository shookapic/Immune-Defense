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

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateCardVisuals();
    }

    void UpdateCardVisuals()
    {
        // Fond de carte rectangulaire
        spriteRenderer.color = new Color(0.95f, 0.9f, 0.8f); // beige clair

        // Texte
        if (nameText) nameText.text = towerName;
        if (costText) costText.text = cost.ToString();

        // Sprite de la tour
        if (towerPrefab != null)
        {
            // SpriteRenderer towerSprite = towerPrefab.GetComponentInChildren<SpriteRenderer>();
            // if (towerSprite != null)
            //     spriteRenderer.sprite = towerSprite.sprite;
        }
    }

    void OnMouseDown()
    {
        Debug.Log($"Carte sélectionnée : {towerName} (coût {cost})");
        // Plus tard → ajout au deck via CardManager
    }
}
