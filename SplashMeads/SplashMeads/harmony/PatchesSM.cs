using HarmonyLib;
using Jotunn.Managers;
using SplashMeads.Configs;
using SplashMeads.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using static EnemyHud;

namespace SplashMeads.Harmony
{
    [HarmonyPatch]
    public class PatchesSM
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyHud), "Awake")]
        public static void AwakeEnemyHud_Postfix(ref EnemyHud __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                SplashMeads.Instance.prefabs.BarlyWineSplashHudIcon.transform.SetParent(__instance.m_baseHud.transform, false);
                SplashMeads.Instance.prefabs.FrostResistSplashHudIcon.transform.SetParent(__instance.m_baseHud.transform, false);
                SplashMeads.Instance.prefabs.PoisonResistSplashHudIcon.transform.SetParent(__instance.m_baseHud.transform, false);
                SplashMeads.Instance.prefabs.RatatoskSplashHudIcon.transform.SetParent(__instance.m_baseHud.transform, false);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in AwakeEnemyHud_Postfix: " + e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyHud), "UpdateHuds")]
        public static bool UpdateHuds_Prefix(ref EnemyHud __instance, Player player, Sadle sadle, float dt)
        {
            try
            {
                Camera mainCamera = Utils.GetMainCamera();
                if (!mainCamera)
                {
                    return false;
                }

                Character character = (sadle ? sadle.GetCharacter() : null);
                Character character2 = (player ? player.GetHoverCreature() : null);
                Character character3 = null;
                foreach (KeyValuePair<Character, HudData> hud in __instance.m_huds)
                {
                    HudData value = hud.Value;
                    if (!value.m_character || !__instance.TestShow(value.m_character, isVisible: true) || value.m_character == character)
                    {
                        if (character3 == null)
                        {
                            character3 = value.m_character;
                            UnityEngine.Object.Destroy(value.m_gui);
                        }

                        continue;
                    }

                    if (value.m_character == character2)
                    {
                        value.m_hoverTimer = 0f;
                    }

                    value.m_hoverTimer += dt;
                    float healthPercentage = value.m_character.GetHealthPercentage();
                    if (value.m_character.IsPlayer() || value.m_character.IsBoss() || value.m_isMount || value.m_hoverTimer < __instance.m_hoverShowDuration)
                    {
                        value.m_gui.SetActive(value: true);
                        int level = value.m_character.GetLevel();
                        if ((bool)value.m_level2)
                        {
                            value.m_level2.gameObject.SetActive(level == 2);
                        }

                        if ((bool)value.m_level3)
                        {
                            value.m_level3.gameObject.SetActive(level == 3);
                        }

                        value.m_name.text = Localization.instance.Localize(value.m_character.GetHoverName());
                        if (!value.m_character.IsBoss() && !value.m_character.IsPlayer())
                        {
                            bool flag = value.m_character.GetBaseAI().HaveTarget();
                            bool flag2 = value.m_character.GetBaseAI().IsAlerted();
                            value.m_alerted.gameObject.SetActive(flag2);
                            value.m_aware.gameObject.SetActive(!flag2 && flag);

                            if (value.m_character.IsTamed())
                            {
                                RectTransform iconBarley = (RectTransform)value.m_gui.transform.Find("SplashBarleyWine");
                                RectTransform iconFrost = (RectTransform)value.m_gui.transform.Find("SplashFrostResist");
                                RectTransform iconPoison = (RectTransform)value.m_gui.transform.Find("SplashPoisonResist");
                                RectTransform iconRatatosk = (RectTransform)value.m_gui.transform.Find("SplashRatatosk");
                                // RectTransform iconRatatosk = (RectTransform)value.m_gui.transform.Find("SplashVananidir");
                                float x = -35;
                                float y = level == 1 ? -13 : -30;

                                if (value.m_character.GetSEMan().HaveStatusEffect(SplashMeads.barleyWineSplashHash))
                                {
                                    if (iconBarley != null)
                                    {
                                        iconBarley.gameObject.SetActive(true);
                                        iconBarley.anchoredPosition = new Vector2(x, y);
                                        IconPositionResult newPos = CalculatePosition(x, y);
                                        x = newPos.x;
                                        y = newPos.y;

                                    }
                                }
                                else
                                {
                                    if (iconBarley != null)
                                        iconBarley.gameObject.SetActive(false);
                                }

                                if (value.m_character.GetSEMan().HaveStatusEffect(SplashMeads.frostResistSplashHash))
                                {
                                    if (iconFrost != null)
                                    {
                                        iconFrost.gameObject.SetActive(true);
                                        iconFrost.anchoredPosition = new Vector2(x, y);
                                        IconPositionResult newPos = CalculatePosition(x, y);
                                        x = newPos.x;
                                        y = newPos.y;
                                    }
                                }
                                else
                                {
                                    if (iconFrost != null)
                                        iconFrost.gameObject.SetActive(false);
                                }

                                if (value.m_character.GetSEMan().HaveStatusEffect(SplashMeads.poisonResistSplashHash))
                                {
                                    if (iconPoison != null) 
                                    {
                                        iconPoison.gameObject.SetActive(true);
                                        iconPoison.anchoredPosition = new Vector2(x, y);
                                        IconPositionResult newPos = CalculatePosition(x, y);
                                        x = newPos.x;
                                        y = newPos.y;
                                    }
                                }
                                else
                                {
                                    if (iconPoison != null)
                                        iconPoison.gameObject.SetActive(false);
                                }

                                if (value.m_character.GetSEMan().HaveStatusEffect(SplashMeads.ratatoskSplashHash))
                                {
                                    if (iconRatatosk != null)
                                    {
                                        iconRatatosk.gameObject.SetActive(true);
                                        iconRatatosk.anchoredPosition = new Vector2(x, y);
                                        IconPositionResult newPos = CalculatePosition(x, y);
                                        x = newPos.x;
                                        y = newPos.y;
                                    }
                                }
                                else
                                {
                                    if (iconRatatosk != null)
                                        iconRatatosk.gameObject.SetActive(false);
                                }
                            }
                        }
                    }
                    else
                    {
                        value.m_gui.SetActive(value: false);
                    }

                    value.m_healthSlow.SetValue(healthPercentage);
                    if ((bool)value.m_healthFastFriendly)
                    {
                        bool flag3 = !player || BaseAI.IsEnemy(player, value.m_character);
                        value.m_healthFast.gameObject.SetActive(flag3);
                        value.m_healthFastFriendly.gameObject.SetActive(!flag3);
                        value.m_healthFast.SetValue(healthPercentage);
                        value.m_healthFastFriendly.SetValue(healthPercentage);
                    }
                    else
                    {
                        value.m_healthFast.SetValue(healthPercentage);
                    }

                    if (value.m_isMount)
                    {
                        float stamina = sadle.GetStamina();
                        float maxStamina = sadle.GetMaxStamina();
                        value.m_stamina.SetValue(stamina / maxStamina);
                        value.m_healthText.text = Mathf.CeilToInt(value.m_character.GetHealth()).ToString();
                        value.m_staminaText.text = Mathf.CeilToInt(stamina).ToString();
                    }

                    if (!value.m_character.IsBoss() && value.m_gui.activeSelf)
                    {
                        UnityEngine.Vector3 zero = Vector3.zero;
                        zero = (value.m_character.IsPlayer() ? (value.m_character.GetHeadPoint() + Vector3.up * 0.3f) : ((!value.m_isMount) ? value.m_character.GetTopPoint() : (player.transform.position - player.transform.up * 0.5f)));
                        Vector3 position = mainCamera.WorldToScreenPointScaled(zero);
                        if (position.x < 0f || position.x > (float)Screen.width || position.y < 0f || position.y > (float)Screen.height || position.z > 0f)
                        {
                            value.m_gui.transform.position = position;
                            value.m_gui.SetActive(value: true);
                        }
                        else
                        {
                            value.m_gui.SetActive(value: false);
                        }
                    }
                }

                if (character3 != null)
                {
                    __instance.m_huds.Remove(character3);
                }

                return false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in UpdateHuds_Prefix: " + e);
                return true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "ApplyDamage")]
        public static void ApplyDamage_Postfix(ref Character __instance, HitData hit)
        {
            try
            {
                if (__instance == null || hit == null)
                    return;

                RefreshSplash(__instance, hit.m_statusEffectHash);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in ApplyDamage_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "ConsumeItem")]
        public static void ConsumeItem_Postfix(ref Player __instance, ItemDrop.ItemData item)
        {
            try
            {
                if (__instance == null || item == null)
                    return;

                string itemName = item.m_shared.m_consumeStatusEffect?.name;

                if (itemName == null)
                    return;

                int effectHash = itemName.GetStableHashCode();

                if (effectHash == 0)
                    return;

                RemoveSplashWhenConsumeNormal(__instance, effectHash);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in ConsumeItem_Postfix: " + e);
            }
        }

        private static void RefreshSplash(Character character, int effectHash)
        {
            if (effectHash == SplashMeads.barleyWineSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.barleyWineSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.barleyWineSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.barleyWineHash, PluginConfig.mead1.duration.Value, true))
                    character.GetSEMan().AddStatusEffect(SplashMeads.barleyWineSplashHash);
            }

            else if (effectHash == SplashMeads.frostResistSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.frostResistSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.frostResistSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.frostResistHash, PluginConfig.mead2.duration.Value, true))
                    character.GetSEMan().AddStatusEffect(SplashMeads.frostResistSplashHash);
            }

            else if (effectHash == SplashMeads.poisonResistSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.poisonResistSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.poisonResistSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.poisonResistHash, PluginConfig.mead3.duration.Value, true))
                    character.GetSEMan().AddStatusEffect(SplashMeads.poisonResistSplashHash);
            }

            else if (effectHash == SplashMeads.ratatoskSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.ratatoskSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.ratatoskSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.ratatoskHash, PluginConfig.mead4.duration.Value, true))
                    character.GetSEMan().AddStatusEffect(SplashMeads.ratatoskSplashHash);
            }
        }

        private static void RemoveSplashWhenConsumeNormal(Player player, int effectHash)
        {
            Jotunn.Logger.LogWarning("effectHash: " + effectHash);
            if (effectHash == SplashMeads.barleyWineHash && player.GetSEMan().HaveStatusEffect(SplashMeads.barleyWineSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.barleyWineSplashHash);

            else if (effectHash == SplashMeads.frostResistHash && player.GetSEMan().HaveStatusEffect(SplashMeads.frostResistSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.frostResistSplashHash);

            else if (effectHash == SplashMeads.poisonResistHash && player.GetSEMan().HaveStatusEffect(SplashMeads.poisonResistSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.poisonResistSplashHash);

            else if (effectHash == SplashMeads.ratatoskHash && player.GetSEMan().HaveStatusEffect(SplashMeads.ratatoskSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.ratatoskSplashHash);
        }

        private static bool ShouldSplashRefresh(Character character, int originalEffectHash, int splashDuration, bool removeOriginal = false)
        {
            if (!character.GetSEMan().HaveStatusEffect(originalEffectHash))
                return true;

            StatusEffect currentOriginal = character.GetSEMan().GetStatusEffect(originalEffectHash);

            if (currentOriginal.GetRemaningTime() < splashDuration)
            {
                if (removeOriginal)
                    character.GetSEMan().RemoveStatusEffect(currentOriginal);

                return true;
            }

            return false;
        }

        private static IconPositionResult CalculatePosition(float valueX, float valueY)
        {
            float x = valueX + 35;
            float y = valueY;

            if (x > 35) {
                x = -35;
                y = valueY + -30;
            }

            return new IconPositionResult(x, y);
        }
    }
}
