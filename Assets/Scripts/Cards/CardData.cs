using UnityEngine;
using System;

// ScriptableObject representing a card's immutable data shared across scenes.
// Create assets via: Create -> ImmuneDefense -> Card Data
[CreateAssetMenu(fileName = "CardData", menuName = "ImmuneDefense/Card Data", order = 0)]
public class CardData : ScriptableObject
{
    [Header("Base Tower Info")]
    public string towerName = "Basic Tower";
    public GameObject towerPrefab;
    public int cost = 50;

    [Header("Upgrade Tiers")]
    public TowerUpgradeTier[] upgradeTiers; // define successive upgrades (optional)
}

[Serializable]
public class TowerUpgradeTier
{
    public string tierName = "Tier";            // Display name e.g. "Tier 2"
    public int additionalCost = 25;              // Cost to upgrade from previous tier
    public float rangeBonus = 0f;                // Additive bonus to range
    public float attackSpeedMultiplier = 1f;     // Multiplicative bonus to attack speed
    public float damageBonus = 0f;               // Additive damage bonus if bullets read this
    public GameObject overrideBulletPrefab;      // Optional bullet prefab override
    public GameObject overrideTowerPrefab;       // Optional full tower prefab replacement for this tier
    public GameObject overrideVisualPrefab;      // Optional child visual variant (use instead of full replacement)
}
