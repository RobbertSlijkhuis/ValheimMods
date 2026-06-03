using System.Collections.Generic;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Models
{
    internal class DetonateData : CloneableData
    {
        [EditorLabel("Announce message")]
        [EditorTooltip("A custom message to announce that a detonation is about to occur. {{user}} will be replaced with the player's name.")]
        public string announceMessage;

        [EditorLabel("Damage settings")]
        [EditorTooltip("Custom damage setting for the explosion.")]
        public DamageData damageData;

        [EditorLabel("Radius")]
        [EditorTooltip("The radius around the player it will search for objects to detonate, in meters.")]
        public float radius = 20f;

        [EditorLabel("Type")]
        [EditorTooltip("The type of objects to detonate.")]
        [DropdownOptions(nameof(DetonateType.Creature), nameof(DetonateType.CreatureSpawned), nameof(DetonateType.Fish), nameof(DetonateType.Piece))]
        public string type = DetonateType.Fish;

        [EditorLabel("Values")]
        [EditorTooltip("The object to detonate must contain this value in its name (Fish1, Fish2, Fish13 contains the word \"Fish\".")]
        public List<string> values = new List<string>() { "Fish" };

        public DetonateData() { }
    }
}
