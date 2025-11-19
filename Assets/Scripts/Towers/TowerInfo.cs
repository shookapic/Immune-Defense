using UnityEngine;

// Holds runtime info about a tower instance such as cost or name for future systems (e.g., economy, upgrades).
public class TowerInfo : MonoBehaviour
{
    public string towerName;
    public int cost;

    // Optionally link back to source CardData
    public CardData sourceData;
}
