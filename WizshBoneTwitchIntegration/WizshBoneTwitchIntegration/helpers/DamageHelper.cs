using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class DamageHelper
    {
        /// <summary>
        /// Convert DamageData to HitData.DamageTypes
        /// </summary>
        /// <param name="damageData"></param>
        /// <returns></returns>
        public static HitData.DamageTypes ConvertToDamageTypes(DamageData damageData)
        {
            HitData.DamageTypes damages = new HitData.DamageTypes();

            if (damageData.blunt != null) damages.m_blunt = (float)damageData.blunt;
            if (damageData.chop != null) damages.m_chop = (float)damageData.chop;
            if (damageData.damage != null) damages.m_damage = (float)damageData.damage;
            if (damageData.fire != null) damages.m_fire = (float)damageData.fire;
            if (damageData.frost != null) damages.m_frost = (float)damageData.frost;
            if (damageData.lightning != null) damages.m_lightning = (float)damageData.lightning;
            if (damageData.pickaxe != null) damages.m_pickaxe = (float)damageData.pickaxe;
            if (damageData.pierce != null) damages.m_pierce = (float)damageData.pierce;
            if (damageData.poison != null) damages.m_poison = (float)damageData.poison;
            if (damageData.slash != null) damages.m_slash = (float)damageData.slash;
            if (damageData.spirit != null) damages.m_slash = (float)damageData.spirit;

            return damages;
        }

        /// <summary>
        /// Set damage from HitData.DamageTypes onto DamageData
        /// </summary>
        /// <param name="damageData"></param>
        /// <param name="damages"></param>
        public static void SetFromDamageTypes(DamageData damageData, HitData.DamageTypes damages)
        {
            damageData.blunt = damages.m_blunt;
            damageData.chop = damages.m_chop;
            damageData.damage = damages.m_damage;
            damageData.fire = damages.m_fire;
            damageData.frost = damages.m_frost;
            damageData.lightning = damages.m_lightning;
            damageData.pickaxe = damages.m_pickaxe;
            damageData.pierce = damages.m_pierce;
            damageData.poison = damages.m_poison;
            damageData.slash = damages.m_slash;
            damageData.spirit = damages.m_spirit;
        }

        /// <summary>
        /// Calculate damage scaled by the player's max health and body armor.
        /// </summary>
        /// <param name="damageData"></param>
        /// <returns></returns>
        public static HitData.DamageTypes CalculateDamageBasedOnMaxHealthAndArmor(DamageData damageData)
        {
            HitData.DamageTypes damages = ConvertToDamageTypes(damageData);
            float armor = Player.m_localPlayer.GetBodyArmor();
            float maxHealth = Player.m_localPlayer.GetMaxHealth();
            float totalDamage = damages.GetTotalDamage();
            float maxDamage = maxHealth * damageData.maxHealthPercentage;

            damages.Modify(maxDamage / totalDamage);
            damages.IncreaseEqually(armor * damageData.armorPercentage);

            return damages;
        }
    }
}
