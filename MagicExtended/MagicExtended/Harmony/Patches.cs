using HarmonyLib;
using MagicExtended.Configs;

namespace MagicExtended.Harmony
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPatch(typeof(Attack), "Start")]
        [HarmonyPrefix]
        public static bool AttackStart_Prefix(Humanoid character, Attack __instance)
        {
            if (__instance == null || character == null || __instance.m_drawStaminaDrain != 8901f)
                return true;

            if (ConfigStaffs.staffEarth3SecondaryCooldown.Value == 0)
                return true;

            bool hasEffect = character.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode("StaffEarth3Cooldown_DW"));

            if (!hasEffect)
            {
                if (!character.HaveEitr(ConfigStaffs.staffEarth3UseEitrSecondary.Value))
                {
                    character.Message(MessageHud.MessageType.Center, "You do not have enough Eitr to perform this action!");
                    return true;
                }

                StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode("StaffEarth3Cooldown_DW"));
                character.GetSEMan().AddStatusEffect(statusEffect);
                return true;
            }

            character.Message(MessageHud.MessageType.Center, "The staff is still recharging!");
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        public static void GetTotalFoodValue_Postfix(ref Player __instance, ref float eitr)
        {
            if (__instance == null)
                return;

            SetEitr(__instance, "SimpleEitrStatusEffect_DW", ConfigSpellbooks.simpleSpellbookEitr.Value, ref eitr);
            SetEitr(__instance, "AdvancedEitrStatusEffect_DW", ConfigSpellbooks.advancedSpellbookEitr.Value, ref eitr);
            SetEitr(__instance, "MasterEitrStatusEffect_DW", ConfigSpellbooks.masterSpellbookEitr.Value, ref eitr);
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
