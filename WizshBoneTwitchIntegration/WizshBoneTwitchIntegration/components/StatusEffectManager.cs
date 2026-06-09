using System.Collections.Generic;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    internal class PersistentSERecord
    {
        public string name;
        public float  remainingTime;
    }

    internal class StatusEffectManager : MonoBehaviour
    {
        // Hashes of SEs currently active that should persist through death
        private HashSet<int> m_trackedHashes = new HashSet<int>();

        // Snapshots waiting to be re-applied after respawn
        private List<PersistentSERecord> m_pending = new List<PersistentSERecord>();

        public void TrackStatusEffect(int nameHash)
        {
            m_trackedHashes.Add(nameHash);
        }

        public void UntrackStatusEffect(int nameHash)
        {
            m_trackedHashes.Remove(nameHash);
        }

        public bool IsTracked(int nameHash)
        {
            return m_trackedHashes.Contains(nameHash);
        }

        public void SnapshotForRespawn(int nameHash, string name, float remainingTime)
        {
            m_trackedHashes.Remove(nameHash);
            m_pending.Add(new PersistentSERecord { name = name, remainingTime = remainingTime });
        }

        public void ClearAll()
        {
            m_trackedHashes.Clear();
            m_pending.Clear();
            Jotunn.Logger.LogInfo("StatusEffectManager: cleared all tracked and pending status effects.");
        }

        public void ReApplyPending(Player player)
        {
            if (m_pending.Count == 0)
                return;

            SEMan seman = player.GetSEMan();

            foreach (PersistentSERecord record in m_pending)
            {
                StatusEffect source = ObjectDB.instance.GetStatusEffect(record.name.GetStableHashCode());

                if (source == null)
                {
                    Jotunn.Logger.LogWarning($"StatusEffectManager: could not find SE '{record.name}' on respawn, skipping.");
                    continue;
                }

                int hash = source.NameHash();
                seman.AddStatusEffect(source);

                StatusEffect active = seman.GetStatusEffect(hash);
                if (active != null)
                {
                    active.m_ttl  = record.remainingTime;
                    active.m_time = 0f;
                }

                m_trackedHashes.Add(hash);
                Jotunn.Logger.LogInfo($"StatusEffectManager: re-applied '{record.name}' with {record.remainingTime:F1}s remaining.");
            }

            m_pending.Clear();
        }
    }
}
