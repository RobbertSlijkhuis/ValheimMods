using System;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchPersistentDamage : MonoBehaviour
    {
        private ZNetView m_netView;
        private string m_redeemerName;
        private string m_redeemTitle;

        public bool m_damageShips = false;
        public bool m_damageStructures = true;
        public bool m_damageBosses = false;

        private readonly int damageDataHash = "PersistentDamage_WBTI".GetStableHashCode();

        private struct HazardHit { public Vector3 pos; public bool protectBosses; public float time; }
        private static readonly List<HazardHit> s_recentHits = new List<HazardHit>();

        public void RegisterHitPosition(Vector3 pos)
        {
            float now = Time.time;
            for (int i = s_recentHits.Count - 1; i >= 0; i--)
            {
                if (now - s_recentHits[i].time > 60f)
                    s_recentHits.RemoveAt(i);
            }
            s_recentHits.Add(new HazardHit { pos = pos, protectBosses = !m_damageBosses, time = now });
        }

        public static bool IsNearRecentHit(Vector3 pos, float radius = 5f)
        {
            float now = Time.time;
            foreach (HazardHit hit in s_recentHits)
            {
                if (now - hit.time <= 60f && hit.protectBosses && Vector3.Distance(hit.pos, pos) <= radius)
                    return true;
            }
            return false;
        }

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                string damageDataString = m_netView.GetZDO().GetString(damageDataHash, "");

                if (damageDataString == "")
                    return;

                string[] data = damageDataString.Split('|');
                m_redeemerName = data[0];
                m_redeemTitle = data[1];

                RedeemData redeem = RedeemHelper.GetRedeemByTitle(m_redeemTitle);

                if (redeem == null)
                {
                    Jotunn.Logger.LogError($"Could not find redeem '{m_redeemTitle}' in TwitchPersistentDamage, skipping setup.");
                    return;
                }

                DamageData damageData = redeem.GetDamageData();

                if (damageData == null)
                {
                    Jotunn.Logger.LogError($"Redeem '{m_redeemTitle}' has no DamageData, skipping setup.");
                    return;
                }

                ApplyDamageData(damageData);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchPersistentDamage.Awake failed: " + e);
            }
        }

        public void SetData(CustomRewardEvent customRewardEvent, DamageData damageData)
        {
            m_redeemerName = customRewardEvent.RedeemerName;
            m_redeemTitle = customRewardEvent.CustomRewardTitle;

            m_netView.GetZDO().Set(damageDataHash, $"{m_redeemerName}|{m_redeemTitle}");

            TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
            baseData?.SetFlag(PersistentComponentFlags.HazardDamage, true);

            ApplyDamageData(damageData);
        }

        public void PropagateToFire(Fire fire)
        {
            if (fire.gameObject.GetComponent<TwitchPersistentDamage>() != null)
                return;

            ZNetView fireNetView = fire.gameObject.GetComponent<ZNetView>();

            if (fireNetView == null || !fireNetView.IsValid())
                return;

            fireNetView.GetZDO().Set(damageDataHash, $"{m_redeemerName}|{m_redeemTitle}");

            TwitchBasePersistentData baseData = fire.gameObject.GetComponent<TwitchBasePersistentData>();
            baseData?.SetFlag(PersistentComponentFlags.HazardDamage, true);

            fire.gameObject.AddComponent<TwitchPersistentDamage>();
        }

        public void ApplyDamageData(DamageData damageData)
        {
            m_damageShips = damageData.damageShips;
            m_damageStructures = damageData.damageStructures;
            m_damageBosses = damageData.damageBosses;

            HitData.DamageTypes damages = damageData.basedOnMaxHealthAndArmor
                ? DamageHelper.CalculateDamageBasedOnMaxHealthAndArmor(damageData)
                : DamageHelper.ConvertToDamageTypes(damageData);

            foreach (Aoe aoe in gameObject.GetComponentsInChildren<Aoe>(true))
                aoe.m_damage = damages;

            ImpactEffect impactEffect = gameObject.GetComponentInChildren<ImpactEffect>(true);

            if (impactEffect != null)
                impactEffect.m_damages = damages;

            Projectile projectile = gameObject.GetComponent<Projectile>();

            if (projectile != null)
                projectile.m_damage = damages;
        }
    }
}
