using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class SpawnAbilityHelper
    {
        /// <summary>
        /// Calculate the damage based on player max health and armor
        /// </summary>
        /// <param name="spawnAbilityData"></param>
        /// <returns></returns>
        public static HitData.DamageTypes CalculateDamageBasedOnMaxHealthAndArmor(SpawnAbilityData spawnAbilityData)
        {
            HitData.DamageTypes damages = DamageHelper.ConvertToDamageTypes(spawnAbilityData.damage);
            float armor = Player.m_localPlayer.GetBodyArmor();
            float maxHealth = Player.m_localPlayer.GetMaxHealth();
            float totalDamage = damages.GetTotalDamage();
            float maxDamage = maxHealth * spawnAbilityData.damage.maxHealthPercentage;

            //Jotunn.Logger.LogWarning("maxHealth: " + maxHealth);
            //Jotunn.Logger.LogWarning("totalDamage: " + totalDamage);
            //Jotunn.Logger.LogWarning("maxDamage: " + maxDamage);
            //Jotunn.Logger.LogWarning("multiplier: " + maxDamage / totalDamage);

            damages.Modify(maxDamage / totalDamage);

            //Jotunn.Logger.LogWarning("newDamage: " + damages.GetTotalDamage());
            //Jotunn.Logger.LogWarning("Armor: " + armor);
            //Jotunn.Logger.LogWarning("newDamage with armor: " + HitData.DamageTypes.ApplyArmor(damages.GetTotalDamage(), armor));

            damages.IncreaseEqually(armor * spawnAbilityData.damage.armorPercentage);

            //Jotunn.Logger.LogWarning("newDamage with armor offset: " + damages.GetTotalDamage());

            return damages;
        }
    }
}
