using System.Collections.Generic;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchPersistentData : MonoBehaviour
    {
        ZNetView netView;
        public bool m_allowDrops;
        public string m_name;

        public readonly int allowDropsHash = "PersistentAllowDrops".GetStableHashCode();
        public readonly int nameHash = "PersistentName".GetStableHashCode();

        public void Awake()
        {
            netView = gameObject.GetComponent<ZNetView>();

            if (netView != null && netView.GetZDO() != null)
            {
                m_name = netView.GetZDO().GetString(nameHash, "");

                if (m_name == "")
                    return;

                m_allowDrops = netView.GetZDO().GetBool(allowDropsHash, false);

                Jotunn.Logger.LogWarning($"Found data: {m_name}, {m_allowDrops}");
                Humanoid humanoid = gameObject.GetComponent<Humanoid>();
                humanoid.m_name = m_name;
                humanoid.m_faction = Character.Faction.Boss;

                if (!m_allowDrops)
                {
                    CharacterDrop characterDrop = gameObject.GetComponent<CharacterDrop>();

                    if (characterDrop != null)
                        characterDrop.m_drops = new List<CharacterDrop.Drop>();
                }
            }
        }

        public void SetData(string name, bool allowDrops)
        {
            netView.GetZDO().Set(nameHash, name);
            netView.GetZDO().Set(allowDropsHash, allowDrops);
        }
    }
}
