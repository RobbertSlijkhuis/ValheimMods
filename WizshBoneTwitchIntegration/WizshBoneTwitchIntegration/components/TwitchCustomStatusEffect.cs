using System;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchCustomStatusEffect : MonoBehaviour
    {
        private readonly List<StatusEffectData> m_statusEffects = new List<StatusEffectData>();
        private readonly float m_animDuration = 1.5f;

        /// <summary>
        /// Get the list of registered status effects
        /// </summary>
        /// <returns></returns>
        public List<StatusEffectData> GetStatusEffects()
        {
            return m_statusEffects;
        }

        /// <summary>
        /// Adds a custom status effect that either executes custom code on start/end and/or persists through death
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool AddStatusEffect(StatusEffectData entry)
        {
            try
            {
                SEMan seMan = Player.m_localPlayer.GetSEMan();

                if (!entry.renew && seMan.HaveStatusEffect(entry.nameHash))
                {
                    Jotunn.Logger.LogWarning($"StatusEffect {entry.name} is already active!");
                    return false;
                }

                foreach (string block in entry.blockedBy)
                {
                    if (seMan.HaveStatusEffect(block.GetStableHashCode()))
                    {
                        Jotunn.Logger.LogWarning($"StatusEffect {entry.name} is blocked by {block}");
                        return false;
                    }
                }

                // We already checked if renew is allowed
                if (entry.persistsThroughDeath)
                {
                    StatusEffectData exists = m_statusEffects.Find(item => item.nameHash == entry.nameHash);

                    if (exists != null)
                        m_statusEffects.Remove(exists);

                    m_statusEffects.Add(entry);
                }

                entry.onStart(entry, false);
                return true;
            }
            catch(Exception e)
            {
                Jotunn.Logger.LogWarning("Something went wrong while adding the StatusEffect: "+ e);
                return false;
            }
        }

        /// <summary>
        /// Remove a custom status effect and invoke onEnd callback
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="removeFromPlayer"></param>
        /// <returns></returns>
        public bool RemoveStatusEffect(StatusEffectData entry, bool removeFromPlayer = true)
        {
            try
            { 
                if (entry.persistsThroughDeath && m_statusEffects.Find(item => item.nameHash == entry.nameHash) == null)
                {
                    Jotunn.Logger.LogWarning($"Could not find StatusEffect with name {entry.name}!");
                    return false;
                }

                entry.onEnd?.Invoke();
                m_statusEffects.Remove(entry);

                if (removeFromPlayer)
                    Player.m_localPlayer.GetSEMan().RemoveStatusEffect(entry.nameHash);

                return true;
            }
            catch(Exception e)
            {
                Jotunn.Logger.LogWarning("Something went wrong while removing the StatusEffect: " + e);
                return false;
            }
        }

        /// <summary>
        /// Invoke onStart of all registered status effects
        /// </summary>
        public void ReApplyStatusEffects()
        {
            foreach (StatusEffectData entry in m_statusEffects)
            {
                if (entry.persistsThroughDeath && entry.durationRemaining > 5f)
                    entry.onStart?.Invoke(entry, true);
            }
        }

        public void WindInTheBack(StatusEffectData statusEffect, bool restartFromDeath)
        {
            StatusEffect original = ObjectDB.instance.GetStatusEffect("GP_Moder".GetStableHashCode());
            StatusEffect clone = StatusEffectHelper.CreateSimple(statusEffect.name, statusEffect.durationRemaining, original.m_icon);
            Player.m_localPlayer.GetSEMan().AddStatusEffect(clone);

            // EnvMan.instance.SetTargetWind(new Vector3(), 1f);
        }

        /// <summary>
        /// Makes the player slower and smaller for a set duration
        /// </summary>
        /// <param name="duration"></param>
        public void PlayerShrink(StatusEffectData statusEffect, bool restartFromDeath)
        {
            if (statusEffect.renew)
            {
                StatusEffect currentStatusEffect = Player.m_localPlayer.GetSEMan().GetStatusEffect(statusEffect.nameHash);

                if (currentStatusEffect != null)
                {
                    currentStatusEffect.m_ttl = statusEffect.duration;
                    currentStatusEffect.ResetTime();
                    return;
                }
            }

            StatusEffect clone = StatusEffectHelper.CreateSimple(statusEffect.name, restartFromDeath ? statusEffect.durationRemaining : statusEffect.duration, WizshBoneTwitchIntegration.Instance.sprites.MiniMeSprite);
            Player.m_localPlayer.GetSEMan().AddStatusEffect(clone);
            RedeemHelper.SetPlayerSpeed(0.75f);
            Vector3 newScale = new Vector3(0.45f, 0.45f, 0.45f);

            StartCoroutine(LerpHelper.LerpScale(Player.m_localPlayer.transform, Player.m_localPlayer.transform.localScale, newScale, m_animDuration));
        }

        /// <summary>
        /// Makes the player faster and bigger for a set duration
        /// </summary>
        /// <param name="duration"></param>
        public void PlayerGrow(StatusEffectData statusEffect, bool restartFromDeath)
        {
            if (statusEffect.renew)
            {
                StatusEffect currentStatusEffect = Player.m_localPlayer.GetSEMan().GetStatusEffect(statusEffect.nameHash);

                if (currentStatusEffect != null)
                {
                    currentStatusEffect.m_ttl = statusEffect.duration;
                    currentStatusEffect.ResetTime();
                    return;
                }
            }

            StatusEffect clone = StatusEffectHelper.CreateSimple(statusEffect.name, restartFromDeath ? statusEffect.durationRemaining : statusEffect.duration, WizshBoneTwitchIntegration.Instance.sprites.BigMeSprite);
            Player.m_localPlayer.GetSEMan().AddStatusEffect(clone);
            RedeemHelper.SetPlayerSpeed(1.25f);
            Vector3 newScale = new Vector3(1.45f, 1.45f, 1.45f);

            StartCoroutine(LerpHelper.LerpScale(Player.m_localPlayer.transform, Player.m_localPlayer.transform.localScale, newScale, m_animDuration));
        }

        /// <summary>
        /// Resets any player height/speed changes
        /// </summary>
        public void PlayerSizeReset()
        {
            RedeemHelper.ResetPlayerSpeed(Player.m_localPlayer);
            Vector3 newScale = new Vector3(1f, 1f, 1f);

            StartCoroutine(LerpHelper.LerpScale(Player.m_localPlayer.transform, Player.m_localPlayer.transform.localScale, newScale, m_animDuration));
        }
    }
}
