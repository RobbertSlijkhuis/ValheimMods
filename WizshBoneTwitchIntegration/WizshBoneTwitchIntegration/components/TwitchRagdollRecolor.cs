using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Components
{
    // Gives a recolored creature's ragdoll the same recolor (see DeathEffectHelper). The spawning
    // client stores what the recolor needs once in the ragdoll's ZDO and flags it; every client's
    // TwitchBasePersistentData.Awake() then attaches this component, which reads it back and
    // recolors its own copy in Start() - after Ragdoll.Awake()/Setup() and LevelEffects have set
    // the ragdoll up, so they can't overwrite it. No RPC, same pattern as TwitchScaledSpawn.
    internal class TwitchRagdollRecolor : MonoBehaviour
    {
        private static readonly int s_creatureKeyHash = "WBTI_RagdollRecolor_Key".GetStableHashCode();
        private static readonly int s_redeemerHash = "WBTI_RagdollRecolor_Redeemer".GetStableHashCode();
        private static readonly int s_colorHash = "WBTI_RagdollRecolor_Color".GetStableHashCode();
        private static readonly int s_emissionHash = "WBTI_RagdollRecolor_Emission".GetStableHashCode();
        private static readonly int s_forceHash = "WBTI_RagdollRecolor_Force".GetStableHashCode();

        // Called once, on the client that owns the freshly spawned ragdoll.
        public static void Apply(GameObject ragdoll, string creatureKey, string redeemerName, string colorOverride, string emissionColorOverride, bool forceColor)
        {
            ZNetView netView = ragdoll.GetComponent<ZNetView>();

            if (netView == null || !netView.IsValid() || !netView.IsOwner())
                return;

            ZDO zdo = netView.GetZDO();

            zdo.Set(s_creatureKeyHash, creatureKey);
            zdo.Set(s_redeemerHash, redeemerName);
            zdo.Set(s_colorHash, colorOverride ?? "");
            zdo.Set(s_emissionHash, emissionColorOverride ?? "");
            zdo.Set(s_forceHash, forceColor);
            ragdoll.GetComponent<TwitchBasePersistentData>()?.SetFlag(PersistentComponentFlags.RagdollRecolor, true);

            // The flag only attaches this component on clients that load the ragdoll from its ZDO.
            ragdoll.AddComponent<TwitchRagdollRecolor>();
        }

        public void Start()
        {
            try
            {
                ZNetView netView = gameObject.GetComponent<ZNetView>();

                if (netView == null || !netView.IsValid())
                    return;

                ZDO zdo = netView.GetZDO();
                string creatureKey = zdo.GetString(s_creatureKeyHash, "");

                if (creatureKey == "")
                    return;

                string colorOverride = zdo.GetString(s_colorHash, "");
                string emissionColorOverride = zdo.GetString(s_emissionHash, "");

                RecolorHelper.RecolorRagdoll(
                    creatureKey,
                    gameObject,
                    zdo.GetString(s_redeemerHash, ""),
                    colorOverride == "" ? null : colorOverride,
                    zdo.GetBool(s_forceHash, false),
                    emissionColorOverride == "" ? null : emissionColorOverride);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchRagdollRecolor.Start failed: " + e);
            }
        }
    }
}
