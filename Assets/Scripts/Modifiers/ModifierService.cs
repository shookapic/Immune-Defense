// Central service that evaluates active modifiers and computes final stat values.
// Place one instance in your initial scene (or add via bootstrap) and mark it DontDestroyOnLoad.

using System.Collections.Generic;
using UnityEngine;

namespace ImmuneDefense.Modifiers
{
    public class ModifierService : MonoBehaviour
    {
        public static ModifierService Instance { get; private set; }

        [Header("Active Modifiers")]
        [Tooltip("Modifiers that are always active (difficulty, auras, events, etc.)")]
        [SerializeField] private List<StatModifierSO> globalModifiers = new List<StatModifierSO>();

        [Tooltip("Modifiers tied to the currently selected hero.")]
        [SerializeField] private List<StatModifierSO> heroModifiers = new List<StatModifierSO>();

        [Header("State")] 
        [SerializeField] private string currentHeroId;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public string CurrentHeroId { get { return currentHeroId; } }

        // Set the active hero and its modifiers (call when the player selects a hero)
        public void SetHero(string heroId, List<StatModifierSO> heroMods)
        {
            currentHeroId = heroId;
            heroModifiers = heroMods != null ? heroMods : new List<StatModifierSO>();
        }

        // Convenience overload to set hero directly from a HeroConfigSO asset
        public void SetHero(HeroConfigSO hero)
        {
            if (hero == null)
            {
                currentHeroId = null;
                heroModifiers = new List<StatModifierSO>();
                return;
            }
            currentHeroId = hero.heroId;
            heroModifiers = hero.modifiers != null ? hero.modifiers : new List<StatModifierSO>();
        }

        // Optionally manage global modifiers at runtime
        public void SetGlobalModifiers(List<StatModifierSO> mods)
        {
            globalModifiers = mods != null ? mods : new List<StatModifierSO>();
        }

        public void AddGlobalModifier(StatModifierSO mod)
        {
            if (mod == null) return;
            if (globalModifiers == null) globalModifiers = new List<StatModifierSO>();
            if (!globalModifiers.Contains(mod)) globalModifiers.Add(mod);
        }

        public void RemoveGlobalModifier(StatModifierSO mod)
        {
            if (mod == null || globalModifiers == null) return;
            globalModifiers.Remove(mod);
        }

        // Primary API: compute final value from baseValue and active modifiers.
        // Policy: all multiplicative effects are combined multiplicatively; all additive effects are summed and applied after multiplication.
        public float Modify(StatId stat, float baseValue, StatContext ctx)
        {
            float add = 0f;
            float mul = 1f;

            ApplyList(heroModifiers, stat, ctx, ref add, ref mul);
            ApplyList(globalModifiers, stat, ctx, ref add, ref mul);

            return baseValue * mul + add;
        }

        // Safe static helper: evaluate with Instance when available.
        public static float Evaluate(StatId stat, float baseValue, StatContext ctx)
        {
            if (Instance == null) return baseValue;
            return Instance.Modify(stat, baseValue, ctx);
        }

        private static void ApplyList(List<StatModifierSO> list, StatId stat, StatContext ctx, ref float add, ref float mul)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var m = list[i];
                if (m == null || m.stat != stat) continue;
                if (!m.AppliesTo(ctx)) continue;

                if (m.op == StatOp.Multiply) mul *= m.value;
                else add += m.value;
            }
        }
    }
}
