// Bundles a hero identifier and its active modifiers.
// Create assets via the menu: Game/Heroes/Hero Config

using System.Collections.Generic;
using UnityEngine;

namespace ImmuneDefense.Modifiers
{
    [CreateAssetMenu(fileName = "HeroConfig", menuName = "Game/Heroes/Hero Config", order = 0)]
    public class HeroConfigSO : ScriptableObject
    {
        [Header("Identity")]
        public string heroId = "Hero_Default";
        public string displayName = "Default Hero";
        public Sprite icon;

        [Header("Modifiers")] 
        public List<StatModifierSO> modifiers = new List<StatModifierSO>();
    }
}
