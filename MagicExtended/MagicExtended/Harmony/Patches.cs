using HarmonyLib;
using MagicExtended.Configs;
using UnityEngine;

namespace MagicExtended.Harmony
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

            if (__instance.m_drawStaminaDrain == 8900f)
            {
                RandomizeMushroom();
                return true;
            }

            if (__instance.m_drawStaminaDrain != 8901f || ConfigStaffs.staffEarth3SecondaryCooldown.Value == 0)
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

        private static void RandomizeMushroom()
        {
            Transform mushroom = MagicExtended.Instance.projectileMushroomPrefab.transform.Find("visual/Mushroom");
            Transform mushroomBlue = MagicExtended.Instance.projectileMushroomPrefab.transform.Find("visual/MushroomBlue");
            Transform mushroomYellow = MagicExtended.Instance.projectileMushroomPrefab.transform.Find("visual/MushroomYellow");
            Transform branch = MagicExtended.Instance.projectileMushroomPrefab.transform.Find("visual/Branch");
            Transform dandelion = MagicExtended.Instance.projectileMushroomPrefab.transform.Find("visual/Dandelion");
            Transform stone = MagicExtended.Instance.projectileMushroomPrefab.transform.Find("visual/Stone");
            Transform flint = MagicExtended.Instance.projectileMushroomPrefab.transform.Find("visual/Flint");
            Transform bush = MagicExtended.Instance.projectileMushroomPrefab.transform.Find("visual/RaspberryBush");

            mushroom.gameObject.SetActive(false);
            mushroomBlue.gameObject.SetActive(false);
            mushroomYellow.gameObject.SetActive(false);
            branch.gameObject.SetActive(false);
            dandelion.gameObject.SetActive(false);
            stone.gameObject.SetActive(false);
            flint.gameObject.SetActive(false);
            bush.gameObject.SetActive(false);

            int index = Random.Range(0, 8);

            switch (index)
            {
                case 0:
                    mushroom.gameObject.SetActive(true);
                    break;
                case 1:
                    mushroomBlue.gameObject.SetActive(true);
                    break;
                case 2:
                    mushroomYellow.gameObject.SetActive(true);
                    break;
                case 3:
                    branch.gameObject.SetActive(true);
                    break;
                case 4:
                    dandelion.gameObject.SetActive(true);
                    break;
                case 5:
                    stone.gameObject.SetActive(true);
                    break;
                case 6:
                    flint.gameObject.SetActive(true);
                    break;
                case 7:
                    bush.gameObject.SetActive(true);
                    break;
                default:
                    mushroom.gameObject.SetActive(true);
                    break;
            }

        }
    }
}
