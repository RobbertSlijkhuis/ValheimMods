using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Extensions;
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
        private int m_creatureIndex;
        public bool m_ignoreWard;
        public bool m_isFollowing;
        public bool m_allowDamageStructures = true;
        public float m_damageScale = 0;
        public float m_healthScale = 0;

        public void Awake()
        {
            m_netView = gameObject.GetComponent<ZNetView>();

            if (m_netView == null || m_netView.GetZDO() == null)
            {
                Jotunn.Logger.LogError("Could not find ZNetView in persistent data!");
                return;
            }

            string creatureStringData = m_netView.GetZDO().GetString(creatureDataHash, "");

            if (creatureStringData == "")
                return;

            string[] creatureRedeemData = creatureStringData.Split('|');
            m_redeemerName = creatureRedeemData[0];
            m_redeemTitle = creatureRedeemData[1];

            if (creatureRedeemData[2] != "")
                m_creatureIndex = int.Parse(creatureRedeemData[2]);

            m_ignoreWard = bool.Parse(creatureRedeemData[3]);

            TwitchCreatureClaim creatureClaim = gameObject.AddComponent<TwitchCreatureClaim>();

            if (m_redeemTitle == "")
            {
                Humanoid humanoid = gameObject.GetComponent<Humanoid>();
                creatureClaim.ReInit(m_redeemerName);
                humanoid.m_name = m_redeemerName;
                return;
            }

            RedeemData redeem = RedeemHelper.GetRedeemByTitle(m_redeemTitle);
            CreatureData creatureData = redeem.creatureData.list[m_creatureIndex];
            creatureClaim.ReInit(m_redeemerName, creatureData);

            ApplyVariables(creatureData);
            ApplyHumanoid(m_redeemerName, creatureData);
            ApplyMonsterAI(creatureData);
            ApplyAllowDrops(creatureData);
            ApplyTameable(creatureData);
        }

        public void Start()
        {
            if (m_redeemerName != null && RecolorHelper.CanRecolorCreature(m_redeemerName, gameObject.name))
                RecolorHelper.RecolorCreature(m_redeemerName, gameObject);
        }

        public void SetData(string name, CreatureData creatureData = null, string redeemTitle = "", bool ignoreWard = false)
        {
            m_redeemerName = name;
            m_redeemTitle = redeemTitle;
            m_ignoreWard = ignoreWard;

            m_netView.GetZDO().Set(creatureDataHash, $"{name}|{redeemTitle}|{creatureData?.index}|{ignoreWard}");

            if (creatureData == null)
                return;

            ApplyVariables(creatureData);
            ApplyHumanoid(name, creatureData);
            ApplyMonsterAI(creatureData);
            ApplyAllowDrops(creatureData);
            ApplyTameable(creatureData);
        }

        public void ApplyVariables(CreatureData creatureData)
        {
            m_creatureIndex = creatureData.index;
            m_isFollowing = creatureData.commandable;
            m_allowDamageStructures = creatureData.allowDamageStructures;
            m_damageScale = creatureData.damageScale;
            m_healthScale = creatureData.healthScale;
            transform.localScale = new Vector3(creatureData.size, creatureData.size, creatureData.size);
        }

        public void ApplyHumanoid(string redeemerName, CreatureData creatureData)
        {
            Humanoid humanoid = gameObject.GetComponent<Humanoid>();
            humanoid.SetLevel(creatureData.level);
            humanoid.m_bossEvent = creatureData.bossEvent;

            if (PluginConfig.configCreaturesSameFaction.Value)
                humanoid.m_faction = Character.Faction.Boss;

            if (creatureData.friendly)
            {
                humanoid.m_faction = Character.Faction.Players;
                humanoid.m_group = WizshBoneTwitchIntegration.HumanoidGroupSpawnFriendly;
            }

            if (creatureData.group != null)
                humanoid.m_group = creatureData.group;

            if (creatureData.maxHealth > 0)
                humanoid.SetMaxHealth(creatureData.maxHealth);

            if (creatureData.rename)
                humanoid.m_name = creatureData.name ?? redeemerName;

            if (PluginConfig.configCreaturesScaling.Value)
            {
                ValheimCreature creature = CreatureHelper.GetValheimCreature(gameObject.name);
                float playerTier = ProgressionHelper.GetPlayerTier();
                float healthScale = m_healthScale != 0 ? m_healthScale : PluginConfig.configCreaturesHealthScale.Value;
                float scale = CreatureHelper.CalculateScale(playerTier, creature.tier, healthScale);

                //Jotunn.Logger.LogWarning($"Old max health: {humanoid.GetMaxHealth()}");

                humanoid.SetMaxHealth(humanoid.GetMaxHealth() * scale);
                humanoid.SetHealth(humanoid.GetMaxHealth());

                //Jotunn.Logger.LogWarning($"New max health: {humanoid.GetMaxHealth()}");
            }
        }

        public void ApplyMonsterAI(CreatureData creatureData)
        {
            MonsterAI monserAI = gameObject.GetComponent<MonsterAI>();
            monserAI.m_aggravatable = creatureData.aggravatable;
            monserAI.m_mistVision = creatureData.mistVision;
        }

        public void ApplyAllowDrops(CreatureData creatureData)
        {
            if (creatureData.allowDrops)
                return;

            CharacterDrop characterDrop = gameObject.GetComponent<CharacterDrop>();

            if (characterDrop != null)
                characterDrop.m_drops = new List<CharacterDrop.Drop>();
        }

        public void ApplyTameable(CreatureData creatureData)
        {
            if (!creatureData.friendly)
                return;

            Tameable tameable = gameObject.AddComponent<Tameable>();
            tameable.m_monsterAI.MakeTame();

            if (creatureData.commandable)
            {
                tameable.m_commandable = true;

                if (m_isFollowing && Player.m_localPlayer != null)
                    tameable.m_monsterAI.SetFollowPlayer(Player.m_localPlayer.gameObject);
            }
        }
    }
}
