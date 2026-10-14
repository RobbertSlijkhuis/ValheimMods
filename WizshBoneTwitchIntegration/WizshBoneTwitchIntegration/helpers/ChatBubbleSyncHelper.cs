using System.Text.RegularExpressions;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Shows a claimed creature's chat bubble on every client, not just the streamer's. Viewer chat only
    // ever reaches the Twitch-logged-in client (TwitchChat), and Chat.SetNpcText only builds a HUD element
    // on the local client - so the streamer's client broadcasts each bubble it shows and the others
    // render it themselves. A one-off event, so an RPC rather than a ZDO field. Scripted talk bubbles
    // (talkMessage) are NOT sent: every client already runs those itself from the claim's ZDO data.
    internal static class ChatBubbleSyncHelper
    {
        private const string BubbleRpcName = "WBTI_ChatBubble";

        // Twitch's own message limit - anything longer didn't come from chat.
        private const int MaxTextLength = 500;

        private static readonly Regex s_richTextTags = new Regex("<[^>]*>", RegexOptions.Compiled);

        // See FlashBangHelper.RegisterRpc - ZRoutedRpc.Register throws on a duplicate name and the
        // instance is recreated per session, so remember which one we already registered on.
        private static ZRoutedRpc s_registeredRpc;

        // Called from GameStart_Postfix (ZRoutedRpc is created in ZNet.Awake, too late for Game.Awake).
        public static void RegisterRpc()
        {
            ZRoutedRpc rpc = ZRoutedRpc.instance;

            if (rpc == null || ReferenceEquals(rpc, s_registeredRpc))
                return;

            rpc.Register<ZDOID, string>(BubbleRpcName, RPC_ChatBubble);
            s_registeredRpc = rpc;
        }

        public static void Broadcast(ZDOID creatureId, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            ZRoutedRpc.instance?.InvokeRoutedRPC(ZRoutedRpc.Everybody, BubbleRpcName, creatureId, text);
        }

        // Runs on every client including the sender, who already showed the bubble locally. The sender
        // can be a modified client and bubbles render as TMP rich text, so the text is sanitized here.
        private static void RPC_ChatBubble(long sender, ZDOID creatureId, string text)
        {
            try
            {
                if (sender == ZDOMan.GetSessionID() || ZNetScene.instance == null || Chat.instance == null || string.IsNullOrEmpty(text))
                    return;

                // A creature that isn't loaded on this client has nothing to anchor a bubble to.
                GameObject creature = ZNetScene.instance.FindInstance(creatureId);

                if (creature == null)
                    return;

                text = s_richTextTags.Replace(text, "");

                if (text.Length > MaxTextLength)
                    text = text.Substring(0, MaxTextLength);

                TwitchCreatureClaim claim = creature.GetComponent<TwitchCreatureClaim>();

                if (claim != null)
                {
                    claim.ShowRemoteBubble(text);
                    return;
                }

                // The claim hasn't reached this client yet (its watcher picks it up within a second).
                // Show the bubble anyway at the default creature height - no talker limit applies as
                // there's no assignment to rank this creature by.
                Chat.instance.SetNpcText(creature, Vector3.up * 2f, ProfileSettingsHelper.Current.chattingCullingRange, 10f, "", text, large: false);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("ChatBubbleSyncHelper.RPC_ChatBubble failed: " + e);
            }
        }
    }
}
