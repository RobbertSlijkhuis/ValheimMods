using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchCustomStatusEffect : MonoBehaviour
    {
        private readonly List<TwitchStatusEffect> m_statusEffects = new List<TwitchStatusEffect>();
        private readonly float m_animDuration = 1.5f;

        /// <summary>
        /// Get the list of registered status effects
        /// </summary>
        /// <returns></returns>
        public List<TwitchStatusEffect> GetStatusEffects()
        {
            return m_statusEffects;
        }

        /// <summary>
        /// Adds a custom status effect that either executes custom code on start/end and/or persists through death
        /// </summary>
        /// <param name="entry"></param>
        public void AddStatusEffect(TwitchStatusEffect entry)
        {
            if (m_statusEffects.Find(item => item.nameHash == entry.nameHash) != null)
            {
                Jotunn.Logger.LogWarning($"StatusEffect {entry.nameHash} already exists!");
                return;
            }

            m_statusEffects.Add(entry);

            if (entry.onStart == null)
                Player.m_localPlayer.GetSEMan().AddStatusEffect(entry.nameHash);
            else
                entry.onStart(entry);
        }

        /// <summary>
        /// Remove a custom status effect and invoke onEnd callback
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="removeFromPlayer"></param>
        public void RemoveStatusEffect(TwitchStatusEffect entry, bool removeFromPlayer = true)
        {
            if (m_statusEffects.Find(item => item.nameHash == entry.nameHash) == null)
            {
                Jotunn.Logger.LogWarning($"Could not find StatusEffect {entry.nameHash}!");
                return;
            }

            entry.onEnd?.Invoke();
            m_statusEffects.Remove(entry);

            if (removeFromPlayer)
                Player.m_localPlayer.GetSEMan().RemoveStatusEffect(entry.nameHash);
        }

        /// <summary>
        /// Invoke onStart of all registered status effects
        /// </summary>
        public void ReApplyStatusEffects()
        {
            foreach (TwitchStatusEffect entry in m_statusEffects)
            {
                entry.onStart?.Invoke(entry);
            }
        }

        /// <summary>
        /// Makes the player slower and smaller for a set duration
        /// </summary>
        /// <param name="duration"></param>
        public void ShrinkPlayer(TwitchStatusEffect statusEffect)
        {
            StatusEffect miniMe = StatusEffectHelper.CreateSimple(statusEffect.name, statusEffect.duration, WizshBoneTwitchIntegration.Instance.sprites.MiniMeSprite);
            Player.m_localPlayer.GetSEMan().AddStatusEffect(miniMe);
            RedeemHelper.SetPlayerSpeed(0.75f);
            Vector3 newScale = new Vector3(0.45f, 0.45f, 0.45f);

            StartCoroutine(LerpHelper.LerpScale(Player.m_localPlayer.transform, Player.m_localPlayer.transform.localScale, newScale, m_animDuration));
        }

        /// <summary>
        /// Makes the player faster and bigger for a set duration
        /// </summary>
        /// <param name="duration"></param>
        public void GrowPlayer(TwitchStatusEffect statusEffect)
        {
            StatusEffect bigMe = StatusEffectHelper.CreateSimple(statusEffect.name, statusEffect.duration, WizshBoneTwitchIntegration.Instance.sprites.BigMeSprite);
            Player.m_localPlayer.GetSEMan().AddStatusEffect(bigMe);
            RedeemHelper.SetPlayerSpeed(1.25f);
            Vector3 newScale = new Vector3(1.45f, 1.45f, 1.45f);

            StartCoroutine(LerpHelper.LerpScale(Player.m_localPlayer.transform, Player.m_localPlayer.transform.localScale, newScale, m_animDuration));
        }

        /// <summary>
        /// Resets any player height/speed changes
        /// </summary>
        public void ResetPlayer()
        {
            RedeemHelper.ResetPlayerSpeed(Player.m_localPlayer);
            Vector3 newScale = new Vector3(1f, 1f, 1f);

            StartCoroutine(LerpHelper.LerpScale(Player.m_localPlayer.transform, Player.m_localPlayer.transform.localScale, newScale, m_animDuration));
        }
    }
}
