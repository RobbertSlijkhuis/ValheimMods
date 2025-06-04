using HarmonyLib;

namespace ModularMagic_IceStaffs.Harmony
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPatch(typeof(Attack), "Start")]
        [HarmonyPrefix]
        public static bool AttackStart_Prefix(Humanoid character, Attack __instance)
        {
            if (__instance == null || character == null)
                return true;

            ItemDrop.ItemData weapon = character.GetCurrentWeapon();

            if (weapon == null || weapon.m_dropPrefab == null)
                return true;

            if (weapon.m_dropPrefab.name == ModularMagic_IceStaffs.Instance.prefabs.StaffIceAOEPrefab.name)
            {
                character.GetSEMan().AddStatusEffect(ModularMagic_IceStaffs.Instance.effects.WalkFastSE);
                return true;
            }

            return true;
        }
    }
}
