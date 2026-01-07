using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchCreaturePersistentData : MonoBehaviour
    {
        private ZNetView m_netView;
        private string m_creatureDataString;
        public bool m_ignoreWard;
        private string m_name;

        private readonly int creatureDataHash = "WTBIPersistentCreatureData".GetStableHashCode();
        private readonly int ignoreWardHash = "WTBIPersistentIgnoreWard".GetStableHashCode();
        private readonly int nameHash = "WTBIPersistentName".GetStableHashCode();

        public void Awake()
        {
            m_netView = gameObject.GetComponent<ZNetView>();

            if (m_netView == null || m_netView.GetZDO == null)
            {
                Jotunn.Logger.LogError("Could not find ZNetView in persistent data!");
                return;
            }

            m_name = m_netView.GetZDO().GetString(nameHash, "");

            if (m_name == "")
                return;

            m_creatureDataString = m_netView.GetZDO().GetString(creatureDataHash, "");

            if (m_creatureDataString == "")
            {
                Jotunn.Logger.LogError("Could not find creature data string in persistent data!");
                return;
            }

            m_ignoreWard = m_netView.GetZDO().GetBool(ignoreWardHash, false);

            Jotunn.Logger.LogWarning($"Found data: {m_name}, {m_creatureDataString}");

            SpawnCreatureData creatureData = StringToCreatureData(m_creatureDataString);
            TwitchCreatureClaim creatureClaim = gameObject.AddComponent<TwitchCreatureClaim>();
            creatureClaim.ReInit(m_name, creatureData);

            ApplyHumanoid(m_name, creatureData.level, creatureData.rename);
            ApplyMonsterAI(creatureData.aggravatable, creatureData.mistVision);
            ApplyAllowDrops(creatureData.allowDrops);
            ApplyTameable(creatureData.friendly, creatureData.commandable);
        }

        public void SetData(string name, SpawnCreatureData creatureData, bool ignoreWard)
        {
            string creatureDataString = CreatureDataToString(creatureData);
            m_netView.GetZDO().Set(creatureDataHash, creatureDataString);
            m_netView.GetZDO().Set(ignoreWardHash, ignoreWard);
            m_netView.GetZDO().Set(nameHash, name);
            m_creatureDataString = creatureDataString;
            m_ignoreWard = ignoreWard;
            m_name = name;

            ApplyHumanoid(name, creatureData.level, creatureData.rename);
            ApplyMonsterAI(creatureData.aggravatable, creatureData.mistVision);
            ApplyAllowDrops(creatureData.allowDrops);
            ApplyTameable(creatureData.friendly, creatureData.commandable);
        }

        public void ApplyHumanoid(string name, int level, bool rename = true)
        {
            Humanoid humanoid = gameObject.GetComponent<Humanoid>();
            humanoid.SetLevel(level);
            humanoid.m_faction = Character.Faction.Boss;

            if (rename)
                humanoid.m_name = name;
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
                tameable.m_commandable = true;
        }

        public string CreatureDataToString(SpawnCreatureData creatureData)
        {
            return $"{creatureData.prefabName}|{creatureData.level}|{creatureData.amount}|{creatureData.position}|{creatureData.allowDrops}|{creatureData.friendly}|{creatureData.commandable}|{creatureData.aggravatable}|{creatureData.mistVision}|{creatureData.rename}|{creatureData.talks}|{creatureData.talkInteract}|{creatureData.talkInterval}|{creatureData.talkMessage}|{creatureData.isHallucination}";
        }

        public SpawnCreatureData StringToCreatureData(string value)
        {
            string[] data = value.Split('|');
            SpawnCreatureData creatureData = new SpawnCreatureData(data[0], int.Parse(data[1]), int.Parse(data[2]), data[3], bool.Parse(data[4]), bool.Parse(data[5]), bool.Parse(data[6]));
            creatureData.aggravatable = bool.Parse(data[7]);
            creatureData.mistVision = bool.Parse(data[8]);
            creatureData.rename = bool.Parse(data[9]);
            creatureData.talks = bool.Parse(data[10]);
            creatureData.talkInteract = bool.Parse(data[11]);
            creatureData.talkInterval = int.Parse(data[12]);
            creatureData.talkMessage = data[13];
            creatureData.isHallucination = bool.Parse(data[14]);

            return creatureData;
        }
    }
}
