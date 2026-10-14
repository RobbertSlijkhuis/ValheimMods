using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Backs "Mute idle sound" for idle sounds that don't go through BaseAI.DoIdleSound: some
    /// creatures (e.g. the Gjall's idle_shiver clip) spawn an "sfx_..._idle_..." prefab from an
    /// animation event handled by AnimationEffect.Effect, which instantiates it directly.
    /// Local audio only: every client runs the animation and this check itself, no ZDO sync needed.
    /// </summary>
    internal static class AnimationSoundHelper
    {
        public static bool ShouldSkipEffect(AnimationEffect animationEffect, AnimationEvent animationEvent)
        {
            GameObject prefab = animationEvent.objectReferenceParameter as GameObject;

            if (prefab == null || !IsIdleSound(prefab.name))
                return false;

            TwitchCreaturePersistentData persistentData = animationEffect.GetComponentInParent<TwitchCreaturePersistentData>();

            return persistentData != null && persistentData.IsIdleSoundMuted;
        }

        // Only sound prefabs with "idle" in the name: footsteps, attack and other animation
        // effects use the same event handler and must keep playing.
        private static bool IsIdleSound(string prefabName)
        {
            return prefabName.StartsWith("sfx", StringComparison.OrdinalIgnoreCase)
                && prefabName.IndexOf("idle", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
