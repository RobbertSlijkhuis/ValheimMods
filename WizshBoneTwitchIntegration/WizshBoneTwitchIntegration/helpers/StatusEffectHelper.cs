using System.Collections.Generic;
using System.Linq;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Exceptions;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class StatusEffectHelper
    {
        /// <summary>
        /// Apply status effects to the player
        /// </summary>
        /// <param name="statusEffectData"></param>
        /// <param name="customRewardEvent"></param>
        /// <param name="m_chat"></param>
        /// <exception cref="RedeemException"></exception>
        public static void ApplyStatusEffects(List<StatusEffectData> statusEffectData, CustomRewardEvent customRewardEvent, TwitchChat m_chat)
        {
            TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
            List<string> availableStatusEffects = StatusEffectHelper.GetAvailableStatusEffects();

            foreach (StatusEffectData statusEffect in statusEffectData)
            {
                statusEffect.Init();

                if (!availableStatusEffects.Contains(statusEffect.name))
                {
                    Jotunn.Logger.LogWarning("Not a valid status effect to apply, skipping...");
                    continue;
                }

                if (!statusEffect.renew && Player.m_localPlayer.GetSEMan().HaveStatusEffect(statusEffect.nameHash))
                {
                    m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer already has the {statusEffect.name} StatusEffect! {(PluginConfig.configAutoResolveRedeems.Value ? TwitchCustomRewards.m_refundAutoResolveOn : TwitchCustomRewards.m_refundAutoResolveOff)}");
                    throw new RedeemException($"Player already has the {statusEffect.name} status effect", ExceptionType.Warning);
                }

                if (statusEffect.name == "PlayerShrink")
                {
                    //Jotunn.Logger.LogWarning("Found shrink");
                    statusEffect.onStart = customStatusEffect.PlayerShrink;
                    statusEffect.onEnd = customStatusEffect.PlayerSizeReset;
                }
                else if (statusEffect.name == "PlayerGrow")
                {
                    //Jotunn.Logger.LogWarning("Found grow");
                    statusEffect.onStart = customStatusEffect.PlayerGrow;
                    statusEffect.onEnd = customStatusEffect.PlayerSizeReset;
                }
                else if (statusEffect.name == "WindInBack")
                {
                    //Jotunn.Logger.LogWarning("Found WindInback");
                    statusEffect.onStart = customStatusEffect.WindInTheBack;
                }

                bool success = customStatusEffect.AddStatusEffect(statusEffect);
                //Jotunn.Logger.LogWarning("AddStatusEffect: " + success);
            }
        }

        /// <summary>
        /// Apply a random status effect to the player
        /// </summary>
        /// <param name="statusEffectData"></param>
        /// <param name="customRewardEvent"></param>
        /// <param name="m_chat"></param>
        /// <exception cref="RedeemException"></exception>
        public static void ApplyRandomStatusEffects(List<StatusEffectData> statusEffectData, CustomRewardEvent customRewardEvent, TwitchChat m_chat)
        {
            TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
            List<string> availableStatusEffects = StatusEffectHelper.GetAvailableStatusEffects();
            StatusEffectData random = StatusEffectHelper.GetRandomStatusEffect(statusEffectData);
            random.Init();

            if (!availableStatusEffects.Contains(random.name))
            {
                Jotunn.Logger.LogWarning("Not a valid status effect to apply, skipping...");
                throw new RedeemException($"{random.name} is not a valid status effect", ExceptionType.Warning);
            }

            if (!random.renew && Player.m_localPlayer.GetSEMan().HaveStatusEffect(random.nameHash))
            {
                m_chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer already has the {random.name} StatusEffect! {(PluginConfig.configAutoResolveRedeems.Value ? TwitchCustomRewards.m_refundAutoResolveOn : TwitchCustomRewards.m_refundAutoResolveOff)}");
                throw new RedeemException($"Player already has the {random.name} status effect", ExceptionType.Warning);
            }

            if (random.name == "PlayerShrink")
            {
                //Jotunn.Logger.LogWarning("Found shrink");
                random.onStart = customStatusEffect.PlayerShrink;
                random.onEnd = customStatusEffect.PlayerSizeReset;
            }
            else if (random.name == "PlayerGrow")
            {
                //Jotunn.Logger.LogWarning("Found grow");
                random.onStart = customStatusEffect.PlayerGrow;
                random.onEnd = customStatusEffect.PlayerSizeReset;
            }
            else if (random.name == "WindInBack")
            {
                //Jotunn.Logger.LogWarning("Found WindInback");
                random.onStart = customStatusEffect.WindInTheBack;
            }

            bool success = customStatusEffect.AddStatusEffect(random);
            //Jotunn.Logger.LogWarning("AddRandomStatusEffect: " + success);
        }

        /// <summary>
        /// Reset the timer on a statuseffect on the player
        /// </summary>
        /// <param name="statusEffect"></param>
        public static void ResetTimer(StatusEffectData statusEffect)
        {
            StatusEffect currentStatusEffect = Player.m_localPlayer.GetSEMan().GetStatusEffect(statusEffect.nameHash);

            if (currentStatusEffect == null)
                return;

            currentStatusEffect.m_ttl = statusEffect.duration;
            currentStatusEffect.ResetTime();
        }

        /// <summary>
        /// Create a simple status effect instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="duration"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static StatusEffect CreateSimple(string name, float duration, Sprite icon = null)
        {
            StatusEffect statusEffect = ScriptableObject.CreateInstance<StatusEffect>();
            statusEffect.name = name;
            statusEffect.m_name = name;
            statusEffect.m_icon = icon;
            statusEffect.m_ttl = duration;

            return statusEffect;
        }

        /// <summary>
        /// Get a list of all available status effects
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAvailableStatusEffects()
        {
            return typeof(StatusEffectType).GetProperties().Select(x => x.GetValue(null).ToString()).ToList();
        }

        /// <summary>
        /// Get the status effect id via mead name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetByMeadID(string name)
        {
            switch (name)
            {
                case "MeadBugRepellent": return StatusEffectType.AntiSting;
                case "BarleyWine": return StatusEffectType.BarlyWine;
                case "MeadTamer": return StatusEffectType.BrewOfAnimalWispers;
                case "MeadBzerker": return StatusEffectType.Bzerker;
                case "MeadFrostResist": return StatusEffectType.FrostResist;
                case "MeadLightfoot": return StatusEffectType.LightFoot;
                case "MeadTrollPheromones": return StatusEffectType.LovePotion;
                case "MeadPoisonResist": return StatusEffectType.PoisonResist;
                case "MeadHasty": return StatusEffectType.Ratatosk;
                case "MeadStrength": return StatusEffectType.TrollStrength;
                case "MeadSwimmer": return StatusEffectType.Vananidir;

                case "MeadEitrMinor": return StatusEffectType.EitrMinor;
                case "MeadEitrLingering": return StatusEffectType.EitrLingering;
                case "MeadHealthMinor": return StatusEffectType.HealingMinor;
                case "MeadHealthMediumr": return StatusEffectType.HealingMedium;
                case "MeadHealthMajor": return StatusEffectType.HealingMajor;
                case "MeadHealthLingering": return StatusEffectType.HealingLingering;
                case "MeadStaminaMinor": return StatusEffectType.StaminaMinor;
                case "MeadStaminaMedium": return StatusEffectType.StaminaMedium;
                case "MeadStaminaLingering": return StatusEffectType.StaminaLingering;

                default: return "";
            }
        }

        /// <summary>
        /// Get a random status effect from a list
        /// </summary>
        /// <returns></returns>
        public static StatusEffectData GetRandomStatusEffect(List<StatusEffectData> statusEffects)
        {
            int index = Random.Range(0, statusEffects.Count);
            return statusEffects[index];
        }
    }
}
