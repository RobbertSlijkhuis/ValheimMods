using System;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchCreaturePersistentData : MonoBehaviour
    {
        private ZNetView m_netView;
        private readonly int creatureDataHash = "CreatureData_WBTI".GetStableHashCode();

        private string m_redeemerName;
        private string m_redeemTitle;
        private string m_savedPrefabName;
        private string m_colorOverride;
        private bool m_forceColorOverride;
        public bool m_ignoreWard;
        public bool m_isFollowing;
        public bool m_allowDamageStructures = true;
        public float m_damageScale = 0;
        public float m_healthScale = 0;

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                string creatureStringData = m_netView.GetZDO().GetString(creatureDataHash, "");

                if (creatureStringData == "")
                    return;

                string[] data = creatureStringData.Split('|');
                m_redeemerName    = data[0];
                m_redeemTitle     = data[1];
                m_savedPrefabName = data[2];
                m_ignoreWard      = bool.Parse(data[3]);
                m_isFollowing     = data.Length > 4 && bool.Parse(data[4]);

                TwitchCreatureClaim creatureClaim = gameObject.AddComponent<TwitchCreatureClaim>();

                if (m_redeemTitle == "")
                {
                    Humanoid humanoid = gameObject.GetComponent<Humanoid>();
                    creatureClaim.ReInit(m_redeemerName);
                    humanoid.m_name = m_redeemerName;
                    return;
                }

                RedeemData redeem = RedeemHelper.GetRedeemByTitle(m_redeemTitle);

                if (redeem == null)
                {
                    Jotunn.Logger.LogError($"Could not find redeem '{m_redeemTitle}' in TwitchCreaturePersistentData, skipping setup.");
                    return;
                }

                if (m_savedPrefabName == "")
                {
                    // SpawnAbility redeems (e.g. biome-prefab showers like "Roots on the line!") use a
                    // placeholder CreatureData with no prefabName, so there's nothing to look up or reapply.
                    creatureClaim.ReInit(m_redeemerName);
                    return;
                }

                List<CreatureData> resolvedList = RedeemHelper.GetResolvedCreatureList(redeem.creatureData);
                CreatureData creatureData = resolvedList.Find(c => c.prefabName == m_savedPrefabName);

                if (creatureData == null)
                {
                    Jotunn.Logger.LogError($"Could not find creature '{m_savedPrefabName}' in resolved list for redeem '{m_redeemTitle}'");
                    return;
                }

                creatureClaim.ReInit(m_redeemerName, creatureData);
                m_colorOverride = creatureData.color;
                m_forceColorOverride = creatureData.forceColor;

                ApplyVariables(creatureData);
                ApplyHumanoid(m_redeemerName, creatureData);
                ApplyMonsterAI(creatureData);
                ApplyAllowDrops(creatureData);
                ApplyTameable(creatureData);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError($"Error in TwitchCreaturePersistentData: {e}");
                Jotunn.Logger.LogError($"{m_redeemerName}, {m_redeemTitle}, {m_savedPrefabName}");
            }
        }

        public void Start()
        {
            try
            {
                if (m_redeemerName != null && RecolorHelper.CanRecolorCreature(m_redeemerName, gameObject.name, m_colorOverride))
                    RecolorHelper.RecolorCreature(m_redeemerName, gameObject, m_colorOverride, m_forceColorOverride);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchCreaturePersistentData.Start failed: " + e);
            }
        }

        public void SetData(string name, CreatureData creatureData = null, string redeemTitle = "", bool ignoreWard = false)
        {
            m_redeemerName    = name;
            m_redeemTitle     = redeemTitle;
            m_ignoreWard      = ignoreWard;
            m_savedPrefabName = creatureData?.prefabName ?? "";
            m_colorOverride   = creatureData?.color;
            m_forceColorOverride = creatureData?.forceColor ?? false;

            // isFollowing defaults to commandable for new spawns
            m_isFollowing = creatureData?.commandable ?? false;

            WriteZDO();

            TwitchBasePersistentData baseData = gameObject.GetComponent<TwitchBasePersistentData>();
            baseData?.SetFlag(PersistentComponentFlags.Creature, true);

            if (creatureData == null)
                return;

            ApplyVariables(creatureData);
            ApplyHumanoid(name, creatureData);
            ApplyMonsterAI(creatureData);
            ApplyAllowDrops(creatureData);
            ApplyTameable(creatureData);
        }

        public void SetFollowing(bool following)
        {
            m_isFollowing = following;
            WriteZDO();
        }

        private void WriteZDO()
        {
            m_netView.GetZDO().Set(creatureDataHash, $"{m_redeemerName}|{m_redeemTitle}|{m_savedPrefabName}|{m_ignoreWard}|{m_isFollowing}");
        }

        private void ApplyVariables(CreatureData creatureData)
        {
            // m_isFollowing is managed via SetData/SetFollowing — not overwritten here
            m_allowDamageStructures = creatureData.allowDamageStructures;
            m_damageScale           = creatureData.damageScale;
            m_healthScale           = creatureData.healthScale;
            transform.localScale    = new Vector3(creatureData.size, creatureData.size, creatureData.size);
        }

        private void ApplyHumanoid(string redeemerName, CreatureData creatureData)
        {
            Humanoid humanoid = gameObject.GetComponent<Humanoid>();

            if (humanoid == null)
            {
                Jotunn.Logger.LogWarning("Humanoid component is null on " + gameObject.name);
                return;
            }

            humanoid.SetLevel(creatureData.level);
            humanoid.m_boss               = creatureData.isBoss;
            humanoid.m_bossEvent          = creatureData.bossEvent;
            humanoid.m_defeatSetGlobalKey = "";

            if (PluginConfig.configCreaturesSameFaction.Value)
                humanoid.m_faction = Character.Faction.Boss;

            if (creatureData.friendly)
            {
                humanoid.m_faction = Character.Faction.Players;
                humanoid.m_group   = WizshBoneTwitchIntegration.HumanoidGroupSpawnFriendly;
            }

            if (creatureData.group != null)
                humanoid.m_group = creatureData.group;

            if (creatureData.maxHealth > 0)
                humanoid.SetMaxHealth(creatureData.maxHealth);

            if (creatureData.rename)
                humanoid.m_name = creatureData.name ?? redeemerName;

            if (PluginConfig.configCreaturesScaling.Value)
            {
                ValheimCreature valheimCreature = CreatureHelper.GetValheimCreature(gameObject.name);

                if (valheimCreature == null)
                {
                    Jotunn.Logger.LogWarning($"No ValheimCreature entry found for {gameObject.name}, skipping health scaling.");
                    return;
                }

                float playerTier  = ProgressionHelper.GetPlayerTier();
                float healthScale = m_healthScale != 0 ? m_healthScale : PluginConfig.configCreaturesHealthScale.Value;
                float scale       = CreatureHelper.CalculateScale(playerTier, valheimCreature.tier, healthScale);

                humanoid.SetMaxHealth(humanoid.GetMaxHealth() * scale);
                humanoid.SetHealth(humanoid.GetMaxHealth());
            }
        }

        private void ApplyMonsterAI(CreatureData creatureData)
        {
            MonsterAI monsterAI = gameObject.GetComponent<MonsterAI>();

            if (monsterAI == null)
            {
                Jotunn.Logger.LogWarning("MonsterAI component is null on " + gameObject.name);
                return;
            }

            monsterAI.m_aggravatable = creatureData.aggravatable;
            monsterAI.m_mistVision   = creatureData.mistVision;
        }

        private void ApplyAllowDrops(CreatureData creatureData)
        {
            if (creatureData.allowDrops)
                return;

            CharacterDrop characterDrop = gameObject.GetComponent<CharacterDrop>();

            if (characterDrop == null)
            {
                Jotunn.Logger.LogWarning("CharacterDrop component is null on " + gameObject.name);
                return;
            }

            characterDrop.m_drops = new List<CharacterDrop.Drop>();
        }

        private void ApplyTameable(CreatureData creatureData)
        {
            if (!creatureData.friendly)
                return;

            Tameable tameable = gameObject.GetComponent<Tameable>();

            if (tameable == null)
                tameable = gameObject.AddComponent<Tameable>();

            tameable.m_monsterAI.MakeTame();

            if (creatureData.commandable)
            {
                tameable.m_commandable = true;

                if (m_isFollowing && Player.m_localPlayer != null)
                    tameable.m_monsterAI.SetFollowTarget(Player.m_localPlayer.gameObject);
            }
        }
    }
}
