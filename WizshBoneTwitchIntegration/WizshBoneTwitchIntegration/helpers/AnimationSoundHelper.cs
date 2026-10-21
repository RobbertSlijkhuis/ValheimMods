using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Backs "Mute sounds" for idle/alert sounds that don't go through
    /// BaseAI.DoIdleSound / m_alertedEffects: some creatures (e.g. the Gjall's idle_shiver clip)
    /// spawn an "sfx_..._idle_..." (or "..._alert_...") prefab from an animation event handled by
    /// AnimationEffect.Effect, which instantiates it directly.
    /// Local audio only: every client runs the animation and this check itself, no ZDO sync needed.
    /// </summary>
    internal static class AnimationSoundHelper
    {
        public static bool ShouldSkipEffect(AnimationEffect animationEffect, AnimationEvent animationEvent)
        {
            GameObject prefab = animationEvent.objectReferenceParameter as GameObject;

            if (prefab == null || !IsIdleOrAlertSound(prefab.name))
                return false;

            TwitchCreaturePersistentData persistentData = animationEffect.GetComponentInParent<TwitchCreaturePersistentData>();

            return persistentData != null && persistentData.IsIdleSoundMuted;
        }

        // Only sound prefabs with "idle" or "alert" in the name: footsteps, attack and other
        // animation effects use the same event handler and must keep playing.
        private static bool IsIdleOrAlertSound(string prefabName)
        {
            return prefabName.StartsWith("sfx", StringComparison.OrdinalIgnoreCase)
                && (prefabName.IndexOf("idle", StringComparison.OrdinalIgnoreCase) >= 0
                    || prefabName.IndexOf("alert", StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
