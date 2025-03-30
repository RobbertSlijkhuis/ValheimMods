using HarmonyLib;
using ModularMagic_Utilities.Configs;

namespace MagicExtended.Harmony
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        public static void GetTotalFoodValue_Postfix(ref Player __instance, ref float eitr)
        {
            if (__instance == null)
                return;

            SetEitr(__instance, "MMU_SimpleEitrStatusEffect", ConfigUtilities.simpleSpellbook.eitr.Value, ref eitr);
            SetEitr(__instance, "MMU_AdvancedEitrStatusEffect", ConfigUtilities.advancedSpellbook.eitr.Value, ref eitr);
            SetEitr(__instance, "MMU_MasterEitrStatusEffect", ConfigUtilities.masterSpellbook.eitr.Value, ref eitr);
            SetEitr(__instance, "MMU_MysticEitrStatusEffect", ConfigUtilities.mysticLantern.eitr.Value, ref eitr);
            SetEitr(__instance, "MMU_EverWinterEitrStatusEffect", ConfigUtilities.everWinterLantern.eitr.Value, ref eitr);
            SetEitr(__instance, "MMU_BlackCoreEitrStatusEffect", ConfigUtilities.blackCoreLantern.eitr.Value, ref eitr);
        }

        private static void SetEitr(Player player, string name, float amount, ref float eitr)
        {
            if (player == null)
                return;

            bool hasEffect = player.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(name));

            if (!hasEffect)
                return;

            eitr += amount;
        }
    }
}
