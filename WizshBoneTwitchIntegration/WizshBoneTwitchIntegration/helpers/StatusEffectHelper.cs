using System.Collections.Generic;
using TwitchSDK.Interop;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class StatusEffectHelper
    {
        public static bool Apply(StatusEffectData data, CustomRewardEvent customRewardEvent)
        {
            if (data == null || data.list == null || data.list.Count == 0)
            {
                Jotunn.Logger.LogWarning("StatusEffectHelper: no status effects configured.");
                return false;
            }

            List<StatusEffectEntry> toApply = new List<StatusEffectEntry>();

            if (data.random)
            {
                int index = UnityEngine.Random.Range(0, data.list.Count);
                toApply.Add(data.list[index]);
            }
            else
            {
                toApply.AddRange(data.list);
            }

            bool anyApplied = false;
            foreach (StatusEffectEntry entry in toApply)
            {
                if (ApplyEntry(entry))
                    anyApplied = true;
            }
            return anyApplied;
        }

        private static bool ApplyEntry(StatusEffectEntry entry)
        {
            if (string.IsNullOrEmpty(entry.name))
            {
                Jotunn.Logger.LogWarning("StatusEffectHelper: entry has no name, skipping.");
                return false;
            }

            Player player = Player.m_localPlayer;

            if (player == null)
            {
                Jotunn.Logger.LogWarning("StatusEffectHelper: no local player.");
                return false;
            }

            // Custom effects handled outside ObjectDB
            if (entry.name == StatusEffectType.PlayerShrink || entry.name == StatusEffectType.PlayerGrow)
            {
                float duration = entry.duration > 0f ? entry.duration : 30f;
                PlayerScaleHelper.Apply(entry.name, duration, entry.playerScale);
                return true;
            }

            StatusEffect source = ObjectDB.instance.GetStatusEffect(entry.name.GetStableHashCode());

            if (source == null)
            {
                Jotunn.Logger.LogWarning($"StatusEffectHelper: could not find SE '{entry.name}' in ObjectDB.");
                return false;
            }

            SEMan seman = player.GetSEMan();
            int   hash  = source.NameHash();

            if (!entry.renew && seman.HaveStatusEffect(hash))
            {
                Jotunn.Logger.LogInfo($"StatusEffectHelper: '{entry.name}' already active and renew is off, skipping.");
                return false;
            }

            seman.AddStatusEffect(source, resetTime: entry.renew);

            if (entry.duration > 0f)
            {
                StatusEffect active = seman.GetStatusEffect(hash);
                if (active != null)
                {
                    active.m_ttl  = entry.duration;
                    active.m_time = 0f;
                }
            }

            if (entry.persistsThroughDeath)
            {
                StatusEffectManager manager = Game.instance.gameObject.GetComponent<StatusEffectManager>();

                if (manager != null)
                    manager.TrackStatusEffect(hash);
                else
                    Jotunn.Logger.LogWarning("StatusEffectHelper: StatusEffectManager not found on Game instance.");
            }

            Jotunn.Logger.LogInfo($"StatusEffectHelper: applied '{entry.name}'" +
                                  $"{(entry.duration > 0f ? $" ({entry.duration}s)" : "")}" +
                                  $"{(entry.persistsThroughDeath ? " [persists]" : "")}.");

            return true;
        }
    }
}
