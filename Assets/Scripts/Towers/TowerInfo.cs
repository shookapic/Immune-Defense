using UnityEngine;

// Holds runtime info about a tower instance such as cost or name for future systems (e.g., economy, upgrades).
public class TowerInfo : MonoBehaviour
{
    public string towerName;
    public int cost;

    // Optionally link back to source CardData
    public CardData sourceData;

    [Header("Upgrade Runtime State")] 
    [Tooltip("Reference to upgrade progress component if upgrades are enabled.")] 
    public TowerUpgradeProgress upgradeProgress;

    void Awake()
    {
        if (upgradeProgress == null)
        {
            upgradeProgress = GetComponent<TowerUpgradeProgress>();
        }
    }

    public bool CanUpgrade()
    {
        return upgradeProgress != null && upgradeProgress.HasMoreTiers;
    }

    public int GetNextUpgradeCost()
    {
        return upgradeProgress != null ? upgradeProgress.GetNextUpgradeCost() : -1;
    }

    public bool ApplyUpgrade()
    {
        if (upgradeProgress == null) return false;
        return upgradeProgress.ApplyNextUpgrade();
    }
}
