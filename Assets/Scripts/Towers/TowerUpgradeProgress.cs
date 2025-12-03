using UnityEngine;

/// <summary>
/// Handles runtime upgrade progression for an individual tower instance.
/// Attach alongside the Tower component. It uses the source CardData (via TowerInfo or manual assignment)
/// to apply upgrade tier modifications.
/// </summary>
[RequireComponent(typeof(Tower))]
public class TowerUpgradeProgress : MonoBehaviour
{
    [Tooltip("Reference to the CardData this tower was spawned from.")]
    public CardData sourceData;

    [Tooltip("Current applied upgrade level (index into upgradeTiers). -1 means base.")] 
    [SerializeField] private int currentTierIndex = -1;

    private Tower tower;

    // Store original base values so upgrades are relative
    private float baseRange;
    private float baseAttackSpeed;

    void Awake()
    {
        tower = GetComponent<Tower>();
        if (tower != null)
        {
            baseRange = tower.range;
            baseAttackSpeed = tower.attackSpeed;
        }
    }

    public int CurrentTierIndex => currentTierIndex;
    public bool HasMoreTiers => sourceData != null && sourceData.upgradeTiers != null && currentTierIndex + 1 < sourceData.upgradeTiers.Length;

    public TowerUpgradeTier GetNextTier()
    {
        if (!HasMoreTiers) return null;
        return sourceData.upgradeTiers[currentTierIndex + 1];
    }

    public int GetNextUpgradeCost()
    {
        var next = GetNextTier();
        return next != null ? next.additionalCost : -1;
    }

    /// <summary>
    /// Apply the next upgrade tier if available. Returns true if successful.
    /// Does not perform any currency/resource deduction (leave that to external systems).
    /// </summary>
    public bool ApplyNextUpgrade()
    {
        var next = GetNextTier();
        if (next == null) return false;
        currentTierIndex++;

        // If this tier provides a full tower prefab replacement, swap now
        if (next.overrideTowerPrefab != null)
        {
            ReplaceTowerRoot(next.overrideTowerPrefab);
            return true; // Replacement handles stat recalculation on new instance
        }

        // If only a visual override is provided, swap visuals (non-destructive)
        if (next.overrideVisualPrefab != null)
        {
            ApplyVisualOverride(next.overrideVisualPrefab);
        }

        RecalculateStats();
        return true;
    }

    private void ReplaceTowerRoot(GameObject newRootPrefab)
    {
        if (newRootPrefab == null) return;
        var parent = transform.parent;
        var position = transform.position;
        var rotation = transform.rotation;
        bool wasSelected = TowerSelectionManager.Instance != null && TowerSelectionManager.Instance.SelectedTower == gameObject;

        var newRoot = Instantiate(newRootPrefab, position, rotation, parent);
        var newTower = newRoot.GetComponent<Tower>();
        var newProgress = newRoot.GetComponent<TowerUpgradeProgress>();
        var newInfo = newRoot.GetComponent<TowerInfo>();

        if (newTower == null || newProgress == null)
        {
            Debug.LogWarning("TowerUpgradeProgress: overrideTowerPrefab missing required Tower/TowerUpgradeProgress components.");
        }

        // Transfer data
        if (newProgress != null)
        {
            newProgress.sourceData = sourceData;
            // Set its tier index to current and force recalculation
            newProgress.SetTierIndexFromReplacement(currentTierIndex);
        }
        if (newInfo != null)
        {
            newInfo.sourceData = sourceData;
            newInfo.towerName = sourceData != null ? sourceData.towerName : newInfo.towerName;
            newInfo.cost = sourceData != null ? sourceData.cost : newInfo.cost;
        }

        // If this tower was selected, update the selection manager to point to the new tower
        if (wasSelected && TowerSelectionManager.Instance != null)
        {
            TowerSelectionManager.Instance.SelectTower(newRoot);
        }

        Destroy(gameObject);
    }

    private void ApplyVisualOverride(GameObject visualPrefab)
    {
        if (visualPrefab == null) return;
        // Destroy existing children (optional: could keep shootPoint; here we assume shootPoint on root)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        var vis = Instantiate(visualPrefab, transform);
        vis.transform.localPosition = Vector3.zero;
        vis.transform.localRotation = Quaternion.identity;
    }

    // Allow replacement instance to set tier index and then recalc stats
    public void SetTierIndexFromReplacement(int tierIndex)
    {
        currentTierIndex = tierIndex;
        // Need to re-capture base values for this new tower
        tower = GetComponent<Tower>();
        if (tower != null)
        {
            baseRange = tower.range;
            baseAttackSpeed = tower.attackSpeed;
        }
        RecalculateStats();
    }

    /// <summary>
    /// Recalculate tower stats based on base values and all applied tiers.
    /// </summary>
    private void RecalculateStats()
    {
        if (tower == null || sourceData == null || sourceData.upgradeTiers == null) return;

        float totalRange = baseRange;
        float totalAttackSpeedMultiplier = 1f;
        GameObject bulletOverride = null;
        float totalDamageBonus = 0f;

        for (int i = 0; i <= currentTierIndex && i < sourceData.upgradeTiers.Length; i++)
        {
            var tier = sourceData.upgradeTiers[i];
            if (tier == null) continue;
            totalRange += tier.rangeBonus;
            totalAttackSpeedMultiplier *= tier.attackSpeedMultiplier;
            totalDamageBonus += tier.damageBonus;
            if (tier.overrideBulletPrefab != null)
                bulletOverride = tier.overrideBulletPrefab;
        }

        tower.range = totalRange;
        tower.attackSpeed = baseAttackSpeed * totalAttackSpeedMultiplier;
        if (bulletOverride != null)
            tower.bulletPrefab = bulletOverride;

        // If Bullet script supports external damage bonus, you could propagate it here.
        // For now we store it locally for potential future use.
        accumulatedDamageBonus = totalDamageBonus;
    }

    // Example place to store aggregated damage bonus for external bullet integration.
    public float accumulatedDamageBonus { get; private set; }
}
