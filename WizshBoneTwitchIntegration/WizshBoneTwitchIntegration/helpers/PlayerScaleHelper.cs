using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class PlayerScaleHelper
    {
        public const float ScaleDelta   = 0.15f;
        public const float ScaleMin     = 0.45f;
        public const float ScaleMax     = 2f;
        public const float LerpDuration = 0.3f;

        private static float s_currentTargetScale = 1f;

        public static void Apply(string effectName, float duration)
        {
            float delta = effectName == StatusEffectType.PlayerShrink ? -ScaleDelta : ScaleDelta;
            s_currentTargetScale = Mathf.Clamp(s_currentTargetScale + delta, ScaleMin, ScaleMax);

            Player player = Player.m_localPlayer;
            SEMan  seman  = player.GetSEMan();
            int    seHash = "PlayerScale".GetStableHashCode();

            if (!seman.HaveStatusEffect(seHash))
            {
                SE_PlayerScale se = WizshBoneTwitchIntegration.Instance.effects.PlayerScale;
                se.m_ttl = duration;
                seman.AddStatusEffect(se);
            }

            // Lerp to the new target — always start from current actual scale
            WizshBoneTwitchIntegration.Instance.StartCoroutine(
                LerpHelper.LerpScale(player.transform, player.transform.localScale, Vector3.one * s_currentTargetScale, LerpDuration));
        }

        public static void SnapToNormal()
        {
            s_currentTargetScale = 1f;

            if (Player.m_localPlayer != null)
                Player.m_localPlayer.transform.localScale = Vector3.one;
        }

        public static void ResetScale()
        {
            s_currentTargetScale = 1f;
        }
    }
}
