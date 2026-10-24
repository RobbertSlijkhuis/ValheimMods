using System.Collections.Generic;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Puts the data of a spawn-ability redeem into the shape its redeem type needs, at redemption time: a dedicated
    /// prefab where the type has only one that fits, the settings that prefab needs, and a damage block that exists.
    /// The editor forms force the same values when a redeem is saved through them, but a redeem saved before that,
    /// or a profile.yaml edited by hand, only gets them here. Changes the live <see cref="SpawnAbilityData"/>.
    /// Door has nothing to normalize; the raw SpawnAbility type has no entry in SpawnAbilityHelper.Normalizers.
    /// </summary>
    internal static class SpawnAbilityNormalizer
    {
        /// <summary>Rain, BoatRain and Meteor only need a damage block.</summary>
        public static void Damage(SpawnAbilityData data)
        {
            EnsureDamage(data);
        }

        public static void LogRain(SpawnAbilityData data)
        {
            data.spawns ??= new List<string>();
            EnsureDamage(data);
        }

        public static void Root(SpawnAbilityData data)
        {
            EnsureDamage(data);

            // No other vanilla prefab fits "roots on the line" - hardcode TentaRoot rather than
            // trusting the generic spawns list, so editing the YAML by hand (or leaving it unset,
            // which previously fell back to SpawnAbilityHelper's fish-rain default) can't point
            // this redeem at an arbitrary/broken prefab.
            data.spawns = new List<string> { "TentaRoot" };
        }

        public static void Smite(SpawnAbilityData data)
        {
            EnsureDamage(data);

            data.accuracy ??= 1f;
            data.groundOffset ??= 0f;
            data.snapToterrain = true;
            data.noSpawnEffect = true;

            // No other vanilla prefab fits "smite" - hardcode lightningAOE rather than
            // trusting the generic spawns list, so editing the YAML by hand can't point this
            // redeem at an arbitrary/broken prefab.
            data.spawns = new List<string> { "lightningAOE" };
        }

        public static void Trap(SpawnAbilityData data)
        {
            EnsureDamage(data);

            // No other vanilla prefab fits "trap field" - hardcode fuling_trap rather than
            // trusting the generic spawns list, so editing the YAML by hand can't point this
            // redeem at an arbitrary/broken prefab.
            data.spawns = new List<string> { "fuling_trap" };
        }

        public static void Windmill(SpawnAbilityData data)
        {
            EnsureDamage(data);

            // Windmill is its own dedicated redeem type, not a name configured through the
            // generic spawns list - always spawn the lean "Windmill_WBTI" clone (see
            // WizshBoneTwitchIntegration.SetupPieces).
            data.spawns = new List<string> { "Windmill_WBTI" };
        }

        // A damage block that exists makes the redeem attach its persistent damage component (see
        // SpawnAbilityExtension), so a redeem saved without one still gets an (empty) block.
        private static void EnsureDamage(SpawnAbilityData data)
        {
            data.damage ??= new DamageData();
        }
    }
}
