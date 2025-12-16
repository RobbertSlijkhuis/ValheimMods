using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchCreaturePersistentData : MonoBehaviour
    {
        private ZNetView m_netView;
        private string m_name;
        private string m_creatureDataString;

        private readonly int creatureDataHash = "WTBIPersistentCreatureData".GetStableHashCode();
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

            Jotunn.Logger.LogWarning($"Found data: {m_name}, {m_creatureDataString}");

            SpawnCreatureData creatureData = StringToCreatureData(m_creatureDataString);
            TwitchCreatureClaim creatureClaim = gameObject.AddComponent<TwitchCreatureClaim>();
            creatureClaim.ReInit(m_name, creatureData);

            ApplyData(m_name, creatureData.level, creatureData.rename);
            ApplyAllowDrops(creatureData.allowDrops);
            ApplyTameable(creatureData.isFriendly);
        }

        public void SetData(string name, SpawnCreatureData creatureData)
        {
            string creatureDataString = CreatureDataToString(creatureData);
            m_netView.GetZDO().Set(nameHash, name);
            m_netView.GetZDO().Set(creatureDataHash, creatureDataString);
            m_name = name;
            m_creatureDataString = creatureDataString;

            ApplyData(name, creatureData.level, creatureData.rename);
            ApplyAllowDrops(creatureData.allowDrops);
            ApplyTameable(creatureData.isFriendly);
        }

        public void ApplyData(string name, int level, bool rename = true)
        {
            Humanoid humanoid = gameObject.GetComponent<Humanoid>();
            humanoid.SetLevel(level);
            humanoid.m_faction = Character.Faction.Boss;

            if (rename)
                humanoid.m_name = name;
        }

        public void ApplyAllowDrops(bool allowDrops)
        {
            if (allowDrops)
                return;

            CharacterDrop characterDrop = gameObject.GetComponent<CharacterDrop>();

            if (characterDrop != null)
                characterDrop.m_drops = new List<CharacterDrop.Drop>();
        }

        public void ApplyTameable(bool isFriendly)
        {
            if (!isFriendly)
                return;

            Tameable tameable = gameObject.AddComponent<Tameable>();
            tameable.m_monsterAI.MakeTame();
            tameable.m_commandable = true;
        }

        public string CreatureDataToString(SpawnCreatureData creatureData)
        {
            return $"{creatureData.prefabName}|{creatureData.level}|{creatureData.amount}|{creatureData.position}|{creatureData.allowDrops}|{creatureData.isFriendly}|{creatureData.rename}|{creatureData.talks}|{creatureData.talkMessage}|{creatureData.talkInterval}|{creatureData.isHallucination}";
        }

        public SpawnCreatureData StringToCreatureData(string value)
        {
            string[] data = value.Split('|');
            SpawnCreatureData creatureData = new SpawnCreatureData(data[0], int.Parse(data[1]), int.Parse(data[2]), data[3], bool.Parse(data[4]), bool.Parse(data[5]));
            creatureData.rename = bool.Parse(data[6]);
            creatureData.talks = bool.Parse(data[7]);
            creatureData.talkMessage = data[8];
            creatureData.talkInterval = int.Parse(data[9]);
            creatureData.isHallucination = bool.Parse(data[10]);

            return creatureData;
        }
    }
}
