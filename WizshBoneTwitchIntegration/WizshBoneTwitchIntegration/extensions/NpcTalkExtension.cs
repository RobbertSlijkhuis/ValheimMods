using System.Collections;
using TMPro;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class NpcTalkExtension
    {
        public static bool SayForce(this NpcTalk npcTalk, string text, string trigger)
        {
            NpcTalk.m_lastTalkTime = Time.time;
            Chat.instance.SetNpcText(npcTalk.gameObject, Vector3.up * npcTalk.m_offset, ProfileSettingsHelper.Current.chattingCullingRange, npcTalk.m_hideDialogDelay, "", text, large: false);
            if (trigger.Length > 0 && npcTalk.m_animator != null)
            {
                npcTalk.m_animator.SetTrigger(trigger);
            }

            return true;
        }
    }
}
