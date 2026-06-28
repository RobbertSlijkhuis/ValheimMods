using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Models
{
    internal class DamageData : CloneableData
    {
        [EditorLabel("Blunt damage")]
        public float? blunt;

        [EditorLabel("Chop damage")]
        [EditorTooltip("This damage is only applied to trees and structures.")]
        public float? chop;

        [EditorHidden]
        public float? damage;

        [EditorLabel("Fire damage")]
        public float? fire;

        [EditorLabel("Frost damage")]
        public float? frost;

        [EditorLabel("Lightning damage")]
        public float? lightning;

        [EditorLabel("Pickaxe damage")]
        [EditorTooltip("This damage is only applied to minable objects, like stone, ore etc. (StoneGolem too)")]
        public float? pickaxe;

        [EditorLabel("Pierce damage")]
        public float? pierce;

        [EditorLabel("Poison damage")]
        public float? poison;

        [EditorLabel("Slash damage")]
        public float? slash;
        
        [EditorLabel("Spirit damage")]
        [EditorTooltip("This damage is only applied to undead")]
        public float? spirit;

        [EditorLabel("Based on max health and armor")]
        [EditorTooltip("If enabled, the damage will be calculated based on the player's max health and armor. Does require one of the damage(s) to be set!")]
        public bool basedOnMaxHealthAndArmor = false;

        [EditorLabel("Armor percentage")]
        [EditorTooltip("Percentage of the player's armor to use for damage calculation. Only if 'Based on max health and armor' is enabled.")]
        public float armorPercentage = 0.8f;

        [EditorLabel("Max health percentage")]
        [EditorTooltip("Percentage of the player's max health to use for damage calculation. Only if 'Based on max health and armor' is enabled.")]
        public float maxHealthPercentage = 0.45f;

        [EditorLabel("Damage ships")]
        public bool damageShips = false;

        [EditorLabel("Damage structures")]
        public bool damageStructures = true;

        [EditorLabel("Damage bosses")]
        public bool damageBosses = false;

        public DamageData() { }
    }
}
