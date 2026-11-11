using HarmonyLib;
using ModularMagic_Core.Helpers;
using static ItemDrop;

namespace ModularMagic_Core.Harmony
{
    /// <summary>
    /// The patches for the staff mods. A staff mod registers with the helpers, these patches only call them.
    /// </summary>
    [HarmonyPatch]
    public class PatchesStaffs
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Start")]
        public static void GameStart_Postfix()
        {
            ImbuedStaffHelper.GameIsReady = true;
        }

        // The runes are applied to the item data of a staff each time it is equipped, see ImbuedStaffHelper
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(Humanoid __instance, ItemData item)
        {
            ImbuedStaffHelper.OnEquipped(__instance, item);
        }

        // The Attack Speed rune speeds up the animation of the normal attack, see AttackSpeedHelper
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "StartAttack")]
        public static void StartAttack_Postfix(Humanoid __instance, bool secondaryAttack, bool __result)
        {
            if (__result)
                AttackSpeedHelper.OnAttackStarted(__instance, secondaryAttack);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "UpdateAttack")]
        public static void UpdateAttack_Postfix(Humanoid __instance)
        {
            AttackSpeedHelper.OnAttackUpdated(__instance);
        }

        // The projectiles of the Rain rune are spawned without hit data, so they get the damage of the staff here, see SpawnedProjectileHelper
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnAbility), "SetupProjectile")]
        public static void SetupProjectile_Postfix(Projectile projectile, ItemData ___m_weapon)
        {
            SpawnedProjectileHelper.OnProjectileSpawned(projectile, ___m_weapon);
        }

        // The game calls this once for every creature a SpawnAbility spawns (owner is the creature, null for a projectile), so a summoned
        // creature gets its strength from the staff here, see SpawnedCreatureHelper
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpawnAbility), "SetupAoe")]
        public static void SetupAoe_Postfix(Character owner, ItemData ___m_weapon)
        {
            SpawnedCreatureHelper.OnCreatureSpawned(owner, ___m_weapon);
        }

        // A custom terrain op must be in the table of every ObjectDB, see TerrainOpRegistry
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ObjectDB), "UpdateRegisters")]
        public static void ObjectDBUpdateRegisters_Postfix(ObjectDB __instance)
        {
            TerrainOpRegistry.AddToTable(__instance);
        }
    }
}
