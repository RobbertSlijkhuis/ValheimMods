using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class ProfileSyncHelper
    {
        private const string RpcName = "WBTI_SyncProfile";
        private static CustomRPC m_rpc;

        /// <summary>
        /// Must be called during plugin Awake() so Jotunn registers the RPC in time.
        /// </summary>
        public static void Init()
        {
            m_rpc = NetworkManager.Instance.AddRPC(
                name: RpcName,
                serverReceive: null,
                clientReceive: RPC_ReceiveProfile
            );
        }

        public static void SendActiveProfileToAll()
        {
            if (m_rpc == null)
            {
                Jotunn.Logger.LogError("ProfileSyncHelper: RPC not initialised, call Init() first.");
                return;
            }

            string profileName = ProfileManager.ActiveProfile;
            string yamlPath    = ProfileManager.GetRedeemPath(profileName);

            if (!File.Exists(yamlPath))
            {
                Jotunn.Logger.LogError("ProfileSyncHelper: Cannot sync, redeems.yaml not found.");
                return;
            }

            string yamlContent = File.ReadAllText(yamlPath);

            ZPackage pkg = new ZPackage();
            pkg.Write(profileName);
            pkg.Write(yamlContent);

            List<ZNetPeer> peers = ZNet.instance.GetPeers();

            if (peers.Count == 0)
            {
                Jotunn.Logger.LogWarning("ProfileSyncHelper: No peers connected to sync to.");
                return;
            }

            m_rpc.SendPackage(peers, pkg);
            Jotunn.Logger.LogInfo($"ProfileSyncHelper: Sent profile '{profileName}' to {peers.Count} peer(s).");
        }

        private static IEnumerator RPC_ReceiveProfile(long sender, ZPackage pkg)
        {
            string profileName = pkg.ReadString();
            string yamlContent = pkg.ReadString();

            try
            {
                // Create the profile folder if it doesn't exist yet
                ProfileManager.CreateProfile(profileName);

                string destPath = ProfileManager.GetRedeemPath(profileName);
                File.WriteAllText(destPath, yamlContent);

                ProfileManager.MarkAsSynced(profileName);

                // Hot-reload if this is the currently active profile
                if (profileName == ProfileManager.ActiveProfile)
                    RedeemHelper.Reload();

                Jotunn.Logger.LogInfo($"ProfileSyncHelper: Received and saved synced profile '{profileName}' from peer {sender}.");
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("ProfileSyncHelper: Failed to receive profile: " + e);
            }

            yield break;
        }
    }
}