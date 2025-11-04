// Context passed to the modifiers system when evaluating a stat.
// Leave fields null/empty if unknown; filters only apply when the field is provided.

using UnityEngine;

namespace ImmuneDefense.Modifiers
{
    public struct StatContext
    {
        // Optional: source tower type (e.g., "Archer", "Mage"). Could come from a tag, enum, or ScriptableObject.
        public string TowerType;

        // Optional: damage type (e.g., "Physical", "Fire"). Only relevant for damage-related stats.
        public string DamageType;

        // Optional: current hero identifier (e.g., "Hero_A"). Set by your hero selection flow.
        public string HeroId;

        // Optional: origin object causing the effect (e.g., projectile, tower)
        public GameObject Source;

        // Optional: target object receiving the effect (e.g., enemy for damage, player wallet for gold)
        public GameObject Target;
    }
}
