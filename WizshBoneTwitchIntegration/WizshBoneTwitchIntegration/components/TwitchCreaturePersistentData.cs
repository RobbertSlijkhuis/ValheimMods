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
        private string m_creatureDataString;
        public bool m_ignoreWard;
        public bool m_isFollowing;
        private string m_name;

        private readonly int creatureDataHash = "WTBIPersistentCreatureData".GetStableHashCode();
        private readonly int ignoreWardHash = "WTBIPersistentIgnoreWard".GetStableHashCode();
        private readonly int nameHash = "WTBIPersistentName".GetStableHashCode();

        public void Awake()
        {
            m_netView = gameObject.GetComponent<ZNetView>();

            if (m_netView == null || m_netView.GetZDO() == null)
            {
                Jotunn.Logger.LogError("Could not find ZNetView in persistent data!");
                return;
            }

            m_name = m_netView.GetZDO().GetString(nameHash, "");
            m_ignoreWard = m_netView.GetZDO().GetBool(ignoreWardHash, false);

            if (m_name == "")
                return;

            TwitchCreatureClaim creatureClaim = gameObject.AddComponent<TwitchCreatureClaim>();
            m_creatureDataString = m_netView.GetZDO().GetString(creatureDataHash, "");

            if (m_creatureDataString == "")
            {
                Humanoid humanoid = gameObject.GetComponent<Humanoid>();
                creatureClaim.ReInit(m_name);
                humanoid.m_name = m_name;
                return;
            }

            Jotunn.Logger.LogWarning($"Found data: {m_name}, {m_creatureDataString}");

            CreatureData creatureData = StringToCreatureData(m_creatureDataString);
            creatureClaim.ReInit(m_name, creatureData);

            ApplyHumanoid(m_name, creatureData.name, creatureData.level, creatureData.maxHealth, creatureData.friendly, creatureData.allowDamageStructures, creatureData.rename);
            ApplyMonsterAI(creatureData.aggravatable, creatureData.mistVision);
            ApplyAllowDrops(creatureData.allowDrops);
            ApplyTameable(creatureData.friendly, creatureData.commandable);
        }

        public void Start()
        {
            if (RecolorHelper.CanRecolorCreature(m_name, gameObject.name))
                RecolorHelper.RecolorCreature(m_name, gameObject);
        }

        public void SetData(string name, CreatureData creatureData = null, bool ignoreWard = false)
        {
            m_netView.GetZDO().Set(ignoreWardHash, ignoreWard);
            m_netView.GetZDO().Set(nameHash, name);
            
            m_ignoreWard = ignoreWard;
            m_name = name;

            if (creatureData == null)
                return;

            string creatureDataString = CreatureDataToString(creatureData);
            m_netView.GetZDO().Set(creatureDataHash, creatureDataString);
            m_creatureDataString = creatureDataString;
            m_isFollowing = creatureData.commandable;
            transform.localScale = new Vector3(creatureData.size, creatureData.size, creatureData.size);

            ApplyHumanoid(name, creatureData.name, creatureData.level, creatureData.maxHealth, creatureData.friendly, creatureData.allowDamageStructures, creatureData.rename);
            ApplyMonsterAI(creatureData.aggravatable, creatureData.mistVision);
            ApplyAllowDrops(creatureData.allowDrops);
            ApplyTameable(creatureData.friendly, creatureData.commandable);
        }

        public void ApplyHumanoid(string redeemerName, string name, int level, float maxHealth, bool friendly, bool allowDamageStructures, bool rename)
        {
            Humanoid humanoid = gameObject.GetComponent<Humanoid>();
            humanoid.SetLevel(level);
            // Gotta refactor this, or no damage structure creatures won't attack eachother
            // humanoid.m_group = allowDamageStructures ? WizshBoneTwitchIntegration.HumanoidGroupSpawnEnemy : WizshBoneTwitchIntegration.HumanoidGroupNoDamageStructure;

            if (PluginConfig.configCreaturesSameFaction.Value)
            {
                humanoid.m_faction = Character.Faction.Boss;
                humanoid.m_group = WizshBoneTwitchIntegration.HumanoidGroupSpawnEnemy;
            }

            if (friendly)
            {
                humanoid.m_faction = Character.Faction.Players;
                humanoid.m_group = WizshBoneTwitchIntegration.HumanoidGroupSpawnFriendly;
            }

            if (maxHealth > 0)
                humanoid.SetMaxHealth(maxHealth);

            if (rename)
                humanoid.m_name = name ?? redeemerName;
        }

        public void ApplyMonsterAI(bool aggravatable, bool misVision)
        {
            MonsterAI monserAI = gameObject.GetComponent<MonsterAI>();
            monserAI.m_aggravatable = aggravatable;
            monserAI.m_mistVision = misVision;
        }

        public void ApplyAllowDrops(bool allowDrops)
        {
            if (allowDrops)
                return;

            CharacterDrop characterDrop = gameObject.GetComponent<CharacterDrop>();

            if (characterDrop != null)
                characterDrop.m_drops = new List<CharacterDrop.Drop>();
        }

        public void ApplyTameable(bool friendly, bool commandable)
        {
            if (!friendly)
                return;

            Tameable tameable = gameObject.AddComponent<Tameable>();
            tameable.m_monsterAI.MakeTame();

            if (commandable)
            {
                tameable.m_commandable = true;

                if (m_isFollowing && Player.m_localPlayer != null)
                    tameable.m_monsterAI.SetFollowPlayer(Player.m_localPlayer.gameObject);
            }
        }

        public string CreatureDataToString(CreatureData creatureData)
        {
            return $"{creatureData.prefabName}|{creatureData.level}|{creatureData.amount}|{creatureData.position}|{creatureData.allowDrops}|{creatureData.friendly}|{creatureData.commandable}|{creatureData.aggravatable}|{creatureData.allowDamageStructures}|{creatureData.maxHealth}|{creatureData.mistVision}|{creatureData.rename}|{creatureData.talks}|{creatureData.talkInteract}|{creatureData.talkInterval}|{creatureData.talkMessage}|{creatureData.isHallucination}|{m_isFollowing}";
        }

        public CreatureData StringToCreatureData(string value)
        {
            string[] data = value.Split('|');
            CreatureData creatureData = new CreatureData(data[0], int.Parse(data[1]), int.Parse(data[2]), data[3], bool.Parse(data[4]), bool.Parse(data[5]), bool.Parse(data[6]));
            creatureData.aggravatable = bool.Parse(data[7]);
            creatureData.allowDamageStructures = bool.Parse(data[8]);
            creatureData.maxHealth = float.Parse(data[9]);
            creatureData.mistVision = bool.Parse(data[10]);
            creatureData.rename = bool.Parse(data[11]);
            creatureData.talks = bool.Parse(data[12]);
            creatureData.talkInteract = bool.Parse(data[13]);
            creatureData.talkInterval = int.Parse(data[14]);
            creatureData.talkMessage = data[15];
            creatureData.isHallucination = bool.Parse(data[16]);
            m_isFollowing = bool.Parse(data[17]);

            return creatureData;
        }
    }
}
