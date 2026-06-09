using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Components
{
    internal class SE_PlayerScale : StatusEffect
    {
        public override void Stop()
        {
            base.Stop();

            Player player = Player.m_localPlayer;

            if (player == null)
            {
                PlayerScaleHelper.SnapToNormal();
                return;
            }

            if (IsDone())
            {
                WizshBoneTwitchIntegration.Instance.StartCoroutine(
                    LerpHelper.LerpScale(player.transform, player.transform.localScale, Vector3.one, PlayerScaleHelper.LerpDuration));
                PlayerScaleHelper.ResetScale();
            }
            else
            {
                PlayerScaleHelper.SnapToNormal();
            }
        }
    }
}
