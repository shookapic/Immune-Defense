// ScriptableObject defining a single stat modifier with optional filters.
// Create assets via the menu: Game/Modifiers/Stat Modifier

using UnityEngine;

namespace ImmuneDefense.Modifiers
{
    [CreateAssetMenu(fileName = "StatModifier", menuName = "Game/Modifiers/Stat Modifier", order = 0)]
    public class StatModifierSO : ScriptableObject
    {
        [Header("Definition")]
        public StatId stat = StatId.TowerDamage;
        public StatOp op = StatOp.Multiply;

        [Tooltip("If Multiply: use factors like 1.10 for +10%. If Add: use absolute amounts like +10.")]
        public float value = 1.10f;

        [Header("Filters (leave empty for 'any')")]
        [Tooltip("Applies only to this tower type (string match). Leave empty for all.")]
        public string towerTypeFilter;

        [Tooltip("Applies only to this damage type (string match). Leave empty for all.")]
        public string damageTypeFilter;

        [Tooltip("Applies only when this hero is active (string match). Leave empty for all.")]
        public string heroIdFilter;

        public bool AppliesTo(StatContext ctx)
        {
            if (!string.IsNullOrEmpty(towerTypeFilter) && towerTypeFilter != ctx.TowerType) return false;
            if (!string.IsNullOrEmpty(damageTypeFilter) && damageTypeFilter != ctx.DamageType) return false;
            if (!string.IsNullOrEmpty(heroIdFilter) && heroIdFilter != ctx.HeroId) return false;
            return true;
        }
    }
}
