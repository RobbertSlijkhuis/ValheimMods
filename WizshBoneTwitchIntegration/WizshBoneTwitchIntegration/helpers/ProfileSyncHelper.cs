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
                serverReceive: RPC_ReceiveProfile,
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

            string profileName   = ProfileManager.ActiveProfile;
            string yamlPath      = ProfileManager.GetRedeemPath(profileName);
            string settingsPath  = ProfileManager.GetSettingsPath(profileName);

            if (!File.Exists(yamlPath))
            {
                Jotunn.Logger.LogError("ProfileSyncHelper: Cannot sync, redeems.yaml not found.");
                return;
            }

            if (!File.Exists(settingsPath))
            {
                Jotunn.Logger.LogError("ProfileSyncHelper: Cannot sync, settings.yaml not found.");
                return;
            }

            string yamlContent     = File.ReadAllText(yamlPath);
            string settingsContent = File.ReadAllText(settingsPath);

            ZPackage pkg = new ZPackage();
            pkg.Write(profileName);
            pkg.Write(yamlContent);
            pkg.Write(settingsContent);

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
            string profileName     = pkg.ReadString();
            string yamlContent     = pkg.ReadString();
            string settingsContent = pkg.ReadString();

            try
            {
                // Create the profile folder if it doesn't exist yet
                ProfileManager.CreateProfile(profileName, out _);

                string destPath = ProfileManager.GetRedeemPath(profileName);
                File.WriteAllText(destPath, yamlContent);

                string settingsDestPath = ProfileManager.GetSettingsPath(profileName);
                File.WriteAllText(settingsDestPath, settingsContent);

                ProfileManager.MarkAsSynced(profileName);

                // Hot-reload if this is the currently active profile
                if (profileName == ProfileManager.ActiveProfile)
                {
                    RedeemHelper.Reload();
                    ProfileSettingsHelper.Reload();
                }

                Jotunn.Logger.LogInfo($"ProfileSyncHelper: Received and saved synced profile '{profileName}' from peer {sender}.");

                // A client only ever has one peer connection (the server), so a client-originated
                // sync can only ever reach the server directly. The server is the only side with a
                // direct connection to every other client, so it relays the sync onward to them -
                // whether it's a dedicated server or a player-hosted one, so the profile still ends
                // up on every connected client either way.
                if (ZNet.instance.IsServer())
                {
                    List<ZNetPeer> otherPeers = ZNet.instance.GetPeers().FindAll(peer => peer.m_uid != sender);

                    if (otherPeers.Count > 0)
                    {
                        ZPackage relayPkg = new ZPackage();
                        relayPkg.Write(profileName);
                        relayPkg.Write(yamlContent);
                        relayPkg.Write(settingsContent);
                        m_rpc.SendPackage(otherPeers, relayPkg);
                        Jotunn.Logger.LogInfo($"ProfileSyncHelper: Relayed profile '{profileName}' to {otherPeers.Count} other peer(s).");
                    }
                }
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("ProfileSyncHelper: Failed to receive profile: " + e);
            }

            yield break;
        }
    }
}