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

        // A real profile.yaml is a few KB to a few hundred KB; anything near this is not one.
        private const int MaxYamlLength = 2_000_000;

        /// <summary>
        /// Raised (on the main thread) after a synced profile has been written to disk and, if it
        /// is the active one, hot-reloaded - argument is the profile name. The GUI uses it to
        /// refresh whatever it is showing and close an editor that was open on the old data.
        /// </summary>
        public static event System.Action<string> ProfileReceived;

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

            string profileName = ProfileManager.ActiveProfile;
            string yamlPath    = ProfileManager.GetRedeemPath(profileName);

            if (!File.Exists(yamlPath))
            {
                Jotunn.Logger.LogError("ProfileSyncHelper: Cannot sync, profile.yaml not found.");
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

            // The name comes from another player and goes straight into a file path below, so it has
            // to pass the same validation a locally typed profile name does (no path separators,
            // no "..") before anything touches the disk - CreateProfile's own check would only
            // fail the create, not stop the write that follows.
            if (!ProfileManager.IsValidProfileName(profileName, out string nameError))
            {
                Jotunn.Logger.LogWarning($"ProfileSyncHelper: Rejected synced profile from peer {sender}: invalid name ({nameError})");
                yield break;
            }

            if (yamlContent == null || yamlContent.Length > MaxYamlLength)
            {
                Jotunn.Logger.LogWarning($"ProfileSyncHelper: Rejected synced profile '{profileName}' from peer {sender}: empty or too large.");
                yield break;
            }

            try
            {
                // Checked before CreateProfile, which would create a default profile.yaml and make
                // every name look like it already existed. A local (not previously synced) profile
                // with this name is about to be replaced, so keep its file as a one-deep backup.
                string existingPath = ProfileManager.GetRedeemPath(profileName);
                bool replacedLocalProfile = File.Exists(existingPath) && !ProfileManager.IsSyncedProfile(profileName);
                if (replacedLocalProfile)
                    File.Copy(existingPath, existingPath + ".bak", overwrite: true);

                // Create the profile folder if it doesn't exist yet
                ProfileManager.CreateProfile(profileName, out _);

                string destPath = ProfileManager.GetRedeemPath(profileName);
                File.WriteAllText(destPath, yamlContent);

                ProfileManager.MarkAsSynced(profileName);

                // Hot-reload if this is the currently active profile
                if (profileName == ProfileManager.ActiveProfile)
                {
                    RedeemHelper.Reload();
                    ProfileSettingsHelper.Reload();
                }

                Jotunn.Logger.LogInfo($"ProfileSyncHelper: Received and saved synced profile '{profileName}' from peer {sender}.");

                // The active profile's redeems/settings can change under the player mid-game, and
                // the GUI may not even be open - so say so on the HUD as well.
                if (MessageHud.instance != null)
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, replacedLocalProfile
                        ? $"Profile '{profileName}' was replaced by a synced copy (your previous version was kept as profile.yaml.bak)."
                        : $"Profile '{profileName}' was synced and is now read-only.");
                }

                // Its own try/catch so a GUI refresh problem can never stop the relay below.
                try
                {
                    ProfileReceived?.Invoke(profileName);
                }
                catch (System.Exception e)
                {
                    Jotunn.Logger.LogWarning("ProfileSyncHelper: ProfileReceived handler failed: " + e);
                }

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