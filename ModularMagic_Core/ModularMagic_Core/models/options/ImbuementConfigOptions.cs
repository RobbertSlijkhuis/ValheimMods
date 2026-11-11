namespace ModularMagic_Core.Models
{
    /// <summary>
    /// The options of the config of the rune values of a staff mod, see ImbuementConfig.
    /// </summary>
    public class ImbuementConfigOptions
    {
        public string sectionName = "Imbuement";

        // True when the Attack Speed rune speeds up the attack animation (Fire, Lightning), see AttackSpeedHelper. False when it lowers the
        // burst interval of the attack instead (Ice, Earth)
        public bool attackSpeedAnimation = false;

        // The runes that give a secondary attack, for the description of the eitr reduction entry (for example "Nova and Rain")
        public string secondaryAttackRunes = "Nova and Rain";
    }
}
