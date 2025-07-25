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
        [HarmonyPatch(typeof(Character), "ApplyDamage")]
        public static void ApplyDamage_Postfix(ref Character __instance, HitData hit)
        {
            try
            {
                if (__instance == null || hit == null)
                    return;

                RefreshSplash(__instance, hit.m_statusEffectHash);
            }
            catch (Exception e)
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
                SplashMeads.Instance.prefabs.VananidirSplashHudIcon.transform.SetParent(__instance.m_baseHud.transform, false);
                SplashMeads.Instance.prefabs.AntiStingSplashHudIcon.transform.SetParent(__instance.m_baseHud.transform, false);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in AwakeEnemyHud_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MonsterAI), "PheromoneFleeCheck")]
        public static void PheromoneFleeCheck_Postfix(ref MonsterAI __instance, ref bool __result,  Character target)
        {
            try
            {
                if (__instance == null)
                    return;

                foreach (StatusEffect statusEffect in target.GetSEMan().GetStatusEffects())
                {
                    if (statusEffect is SE_Stats sE_Stats && sE_Stats.m_pheromoneFlee)
                    {
                        Character component = sE_Stats.m_pheromoneTarget.GetComponent<Character>();
                        if ((object)component != null && component.m_name == __instance.m_character.m_name)
                        {
                            __result = true;
                            return;
                        }
                    }
                }

                __result = false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in PheromoneFleeCheck_Postfix: " + e);
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
                                RectTransform iconVananidir = (RectTransform)value.m_gui.transform.Find("SplashVananidir");
                                RectTransform iconAntiSting = (RectTransform)value.m_gui.transform.Find("SplashAntiSting");
                                IconPosition iconPos = new IconPosition(-35, level == 1 ? -15 : -30);

                                iconPos = UpdateHudIcon(value.m_character, iconBarley, SplashMeads.barleyWineSplashHash, iconPos);
                                iconPos = UpdateHudIcon(value.m_character, iconFrost, SplashMeads.frostResistSplashHash, iconPos);
                                iconPos = UpdateHudIcon(value.m_character, iconPoison, SplashMeads.poisonResistSplashHash, iconPos);
                                iconPos = UpdateHudIcon(value.m_character, iconRatatosk, SplashMeads.ratatoskSplashHash, iconPos);
                                iconPos = UpdateHudIcon(value.m_character, iconVananidir, SplashMeads.vananidirSplashHash, iconPos);
                                iconPos = UpdateHudIcon(value.m_character, iconAntiSting, SplashMeads.antiStingSplashHash, iconPos);
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

        private static void RefreshSplash(Character character, int effectHash)
        {
            if (effectHash == SplashMeads.barleyWineSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.barleyWineSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.barleyWineSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.barleyWineHash, PluginConfig.mead1.duration.Value, true))
                {
                    ApplySkinnedMeshToFX(SplashMeads.Instance.prefabs.BarlyWineSplashFX, character);
                    character.GetSEMan().AddStatusEffect(SplashMeads.barleyWineSplashHash);
                }
            }

            else if (effectHash == SplashMeads.frostResistSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.frostResistSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.frostResistSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.frostResistHash, PluginConfig.mead2.duration.Value, true))
                {
                    ApplySkinnedMeshToFX(SplashMeads.Instance.prefabs.FrostResistSplashFX, character);
                    character.GetSEMan().AddStatusEffect(SplashMeads.frostResistSplashHash);
                }
            }

            else if (effectHash == SplashMeads.poisonResistSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.poisonResistSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.poisonResistSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.poisonResistHash, PluginConfig.mead3.duration.Value, true))
                {
                    ApplySkinnedMeshToFX(SplashMeads.Instance.prefabs.PoisonResistSplashFX, character);
                    character.GetSEMan().AddStatusEffect(SplashMeads.poisonResistSplashHash);
                }
            }

            else if (effectHash == SplashMeads.ratatoskSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.ratatoskSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.ratatoskSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.ratatoskHash, PluginConfig.mead4.duration.Value, true))
                {
                    ApplySkinnedMeshToFX(SplashMeads.Instance.prefabs.RatatoskSplashFX, character);

                    if (character.gameObject.name == "Lox(Clone)")
                    {
                        GameObject particles = SplashMeads.Instance.prefabs.RatatoskSplashFX.transform.Find("lingering_effects/particles").gameObject;
                        GameObject particlesLox = SplashMeads.Instance.prefabs.RatatoskSplashFX.transform.Find("lingering_effects/particles_lox").gameObject;
                        particles.SetActive(false);
                        particlesLox.SetActive(true);

                        character.GetSEMan().AddStatusEffect(SplashMeads.ratatoskSplashHash);

                        particles.SetActive(true);
                        particlesLox.SetActive(false);
                        return;
                    }

                    character.GetSEMan().AddStatusEffect(SplashMeads.ratatoskSplashHash);
                }
            }

            else if (effectHash == SplashMeads.vananidirSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.vananidirSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.vananidirSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.vananidirHash, PluginConfig.mead5.duration.Value, true))
                {
                    ApplySkinnedMeshToFX(SplashMeads.Instance.prefabs.VananidirSplashFX, character);
                    character.GetSEMan().AddStatusEffect(SplashMeads.vananidirSplashHash);
                }
            }

            else if (effectHash == SplashMeads.antiStingSplashHash && character.GetSEMan().HaveStatusEffect(SplashMeads.antiStingSplashHash))
            {
                character.GetSEMan().RemoveStatusEffect(SplashMeads.antiStingSplashHash);

                if (ShouldSplashRefresh(character, SplashMeads.antiStingHash, PluginConfig.mead6.duration.Value, true))
                {
                    ApplySkinnedMeshToFX(SplashMeads.Instance.prefabs.AntiStingSplashFX, character);

                    if (character.gameObject.name == "Lox(Clone)")
                    {
                        GameObject particles = SplashMeads.Instance.prefabs.RatatoskSplashFX.transform.Find("lingering_effects/particles").gameObject;
                        GameObject particlesLox = SplashMeads.Instance.prefabs.RatatoskSplashFX.transform.Find("lingering_effects/particles_lox").gameObject;
                        particles.SetActive(false);
                        particlesLox.SetActive(true);

                        character.GetSEMan().AddStatusEffect(SplashMeads.antiStingSplashHash);

                        particles.SetActive(true);
                        particlesLox.SetActive(false);
                        return;
                    }

                    character.GetSEMan().AddStatusEffect(SplashMeads.antiStingSplashHash);
                }
            }
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

        private static void ApplySkinnedMeshToFX(GameObject prefab, Character character)
        {
            if (!PluginConfig.showParticles.Value)
                return;

            SkinnedMeshRenderer[] renderers = character.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            SkinnedMeshRenderer skinnedMesh = GetBodyRenderer(renderers);

            if (skinnedMesh == null)
                return;

            Transform flareTrans = prefab.transform.Find("lingering_effects/flare");
            Transform flakesTrans = prefab.transform.Find("lingering_effects/flakes_up");
            Transform particlesTrans = prefab.transform.Find("lingering_effects/particles");

            if (character.IsPlayer() && !PluginConfig.showParticlesOnPlayers.Value)
            {
                flareTrans?.gameObject.SetActive(false);
                flakesTrans?.gameObject.SetActive(false);
                particlesTrans?.gameObject.SetActive(false);
                return;
            }

            flareTrans?.gameObject.SetActive(true);
            flakesTrans?.gameObject.SetActive(true);
            particlesTrans?.gameObject.SetActive(true);

            if (particlesTrans == null)
                return;

            ParticleSystem particleSystem = particlesTrans.GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.skinnedMeshRenderer = skinnedMesh;
        }

        private static SkinnedMeshRenderer GetBodyRenderer(SkinnedMeshRenderer[] renderers)
        {
            // Make this list configurable
            List<string> allowedNames = new List<string>()
            {
                "poly art boar",
                "body",
                "m",
                "lavaneck.001",
            };

            if (renderers.Length == 1)
                return renderers[0];

            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                if (allowedNames.Contains(renderer.name.ToLower()))
                    return renderer;
            }

            return null;
        }

        private static void RemoveSplashWhenConsumeNormal(Player player, int effectHash)
        {
            if (effectHash == SplashMeads.barleyWineHash && player.GetSEMan().HaveStatusEffect(SplashMeads.barleyWineSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.barleyWineSplashHash);

            else if (effectHash == SplashMeads.frostResistHash && player.GetSEMan().HaveStatusEffect(SplashMeads.frostResistSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.frostResistSplashHash);

            else if (effectHash == SplashMeads.poisonResistHash && player.GetSEMan().HaveStatusEffect(SplashMeads.poisonResistSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.poisonResistSplashHash);

            else if (effectHash == SplashMeads.ratatoskHash && player.GetSEMan().HaveStatusEffect(SplashMeads.ratatoskSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.ratatoskSplashHash);

            else if (effectHash == SplashMeads.vananidirHash && player.GetSEMan().HaveStatusEffect(SplashMeads.vananidirSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.vananidirSplashHash);

            else if (effectHash == SplashMeads.antiStingHash && player.GetSEMan().HaveStatusEffect(SplashMeads.antiStingSplashHash))
                player.GetSEMan().RemoveStatusEffect(SplashMeads.antiStingSplashHash);
        }

        private static IconPosition UpdateHudIcon(Character character, RectTransform icon, int splashHash, IconPosition iconPos)
        {
            if (!PluginConfig.showHudIcons.Value)
            {
                icon.gameObject.SetActive(false);
                return iconPos;
            }

            if (character.GetSEMan().HaveStatusEffect(splashHash))
            {
                icon.gameObject.SetActive(true);
                icon.anchoredPosition = new Vector2(iconPos.x, iconPos.y);
                return CalcIconPos(iconPos);
            }
            else
            {
                icon.gameObject.SetActive(false);
                return iconPos;
            }
        }

        private static IconPosition CalcIconPos(IconPosition value)
        {
            float x = value.x + 35;
            float y = value.y;

            if (x > 35) {
                x = -35;
                y = value.y + -30;
            }

            return new IconPosition(x, y);
        }
    }
}
