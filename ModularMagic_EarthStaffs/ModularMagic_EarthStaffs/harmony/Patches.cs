using HarmonyLib;
// using ModularMagic_EarthStaffs.Configs;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Harmony
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

            if (weapon.m_dropPrefab.name == ModularMagic_EarthStaffs.Instance.prefabs.staffEarth0Prefab.name)
            {
                RandomizeMushroom();
                return true;
            }

            return true;
            //return CheckForCooldown(character, __instance);
        }

        //private static bool CheckForCooldown(Character character, Attack attack)
        //{
        //    float configCooldownValue = 0f;
        //    float configEitrValue = 0f;
        //    string effectName = "";
        //    bool hasEffect = false;

        //    switch (attack.m_drawStaminaDrain)
        //    {
        //        case 8901f:
        //            configCooldownValue = ConfigStaffs.staffEarth3.secondaryCooldown.Value;
        //            configEitrValue = ConfigStaffs.staffEarth3.useEitrSecondary.Value;
        //            effectName = "StaffEarth3Cooldown_DW";
        //            hasEffect = character.GetSEMan().HaveStatusEffect(StringExtensionMethods.GetStableHashCode(effectName));
        //            break;
        //    }

        //    if (effectName == "" || configCooldownValue == 0f)
        //        return true;

        //    if (!hasEffect)
        //    {
        //        if (!character.HaveEitr(configEitrValue))
        //        {
        //            character.Message(MessageHud.MessageType.Center, "You do not have enough Eitr to perform this action!");
        //            return true;
        //        }

        //        StatusEffect statusEffect = ObjectDB.instance.GetStatusEffect(StringExtensionMethods.GetStableHashCode(effectName));
        //        character.GetSEMan().AddStatusEffect(statusEffect);
        //        return true;
        //    }

        //    character.Message(MessageHud.MessageType.Center, "The staff is still recharging!");
        //    return false;
        //}

        private static void RandomizeMushroom()
        {
            Transform mushroom = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/Mushroom");
            Transform mushroomBlue = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/MushroomBlue");
            Transform mushroomYellow = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/MushroomYellow");
            Transform branch = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/Branch");
            Transform dandelion = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/Dandelion");
            Transform stone = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/Stone");
            Transform flint = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/Flint");
            Transform bush = ModularMagic_EarthStaffs.Instance.prefabs.projectileMushroomPrefab.transform.Find("visual/RaspberryBush");

            mushroom.gameObject.SetActive(false);
            mushroomBlue.gameObject.SetActive(false);
            mushroomYellow.gameObject.SetActive(false);
            branch.gameObject.SetActive(false);
            dandelion.gameObject.SetActive(false);
            stone.gameObject.SetActive(false);
            flint.gameObject.SetActive(false);
            bush.gameObject.SetActive(false);

            int index = UnityEngine.Random.Range(0, 8);

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
