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
                WizshBoneTwitchIntegration.Instance.StartCoroutine(PlayerScaleHelper.LerpVisualScale(player, player.transform.localScale.x, 1f, PlayerScaleHelper.LerpDuration));
                PlayerScaleHelper.ResetScale();
            }
            else
            {
                PlayerScaleHelper.SnapToNormal();
            }
        }
    }
}
