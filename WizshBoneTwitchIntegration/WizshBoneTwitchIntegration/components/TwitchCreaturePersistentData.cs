using System.Collections.Generic;
using UnityEngine;
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

            CreatureData creatureData = StringToCreatureData(m_creatureDataString);
            TwitchCreatureClaim creatureClaim = gameObject.AddComponent<TwitchCreatureClaim>();
            creatureClaim.ReInit(m_name, creatureData);

            ApplyHumanoid(m_name, creatureData.level, creatureData.maxHealth, creatureData.friendly, creatureData.allowDamageStructures, creatureData.rename);
            ApplyMonsterAI(creatureData.aggravatable, creatureData.mistVision);
            ApplyAllowDrops(creatureData.allowDrops);
            ApplyTameable(creatureData.friendly, creatureData.commandable);

            if (RecolorHelper.IsRedeemerSpecialViewer(m_name) && RecolorHelper.IsCreatureInList($"{gameObject.name.Replace("(Clone)", "")}"))
            {
                RecolorHelper.RecolorCreature(gameObject, m_name);
            }
        }

        public void SetData(string name, CreatureData creatureData, bool ignoreWard)
        {
            string creatureDataString = CreatureDataToString(creatureData);
            m_netView.GetZDO().Set(creatureDataHash, creatureDataString);
            m_netView.GetZDO().Set(ignoreWardHash, ignoreWard);
            m_netView.GetZDO().Set(nameHash, name);
            m_creatureDataString = creatureDataString;
            m_ignoreWard = ignoreWard;
            m_name = name;
            m_isFollowing = creatureData.commandable;

            ApplyHumanoid(name, creatureData.level, creatureData.maxHealth, creatureData.friendly, creatureData.allowDamageStructures, creatureData.rename);
            ApplyMonsterAI(creatureData.aggravatable, creatureData.mistVision);
            ApplyAllowDrops(creatureData.allowDrops);
            ApplyTameable(creatureData.friendly, creatureData.commandable);
        }

        public void ApplyHumanoid(string name, int level, float maxHealth, bool friendly, bool allowDamageStructures, bool rename)
        {
            Humanoid humanoid = gameObject.GetComponent<Humanoid>();
            humanoid.SetLevel(level);
            humanoid.m_faction = friendly ? Character.Faction.Players : Character.Faction.Boss;

            if (maxHealth > 0)
                humanoid.SetMaxHealth(maxHealth);

            if (!allowDamageStructures)
                humanoid.m_group = WizshBoneTwitchIntegration.NoDamageStructureGroup;

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
            {
                tameable.m_commandable = true;

                if (m_isFollowing && Player.m_localPlayer != null)
                    tameable.m_monsterAI.SetFollowPlayer(Player.m_localPlayer.gameObject);
            }
        }

        public string CreatureDataToString(CreatureData creatureData)
        {
            return $"{creatureData.prefabName}|{creatureData.level}|{creatureData.amount}|{creatureData.position}|{creatureData.allowDrops}|{creatureData.friendly}|{creatureData.commandable}|{creatureData.aggravatable}|{creatureData.allowDamageStructures}|{creatureData.maxHealth}|{creatureData.mistVision}|{creatureData.rename}|{creatureData.talks}|{creatureData.talkInteract}|{creatureData.talkInterval}|{creatureData.talkMessage}|{creatureData.isHallucination}";
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

            return creatureData;
        }
    }
}
