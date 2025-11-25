using UnityEngine;

// ScriptableObject representing a card's immutable data shared across scenes.
// Create assets via: Create -> ImmuneDefense -> Card Data
[CreateAssetMenu(fileName = "CardData", menuName = "ImmuneDefense/Card Data", order = 0)]
public class CardData : ScriptableObject
{
    public string towerName = "Basic Tower";
    public GameObject towerPrefab;
    public int cost = 50;
}
