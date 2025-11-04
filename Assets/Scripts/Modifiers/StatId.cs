// Core stat identifiers that can be modified by the modifiers system.
// Extend this enum to add more stats (e.g., AttackSpeed, Range, CritChance, etc.).

namespace ImmuneDefense.Modifiers
{
    public enum StatId
    {
        // Final damage dealt by towers/projectiles to enemies
        TowerDamage = 0,

        // Final gold amount credited to the player (e.g., on kill, on wave end)
        GoldGain = 1,
    }
}
