using Glimuleikar.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections;

namespace Glimuleikar.Network
{
    internal static class ConfigRPC
    {
        private static CustomRPC _rpc;

        // Prevents SettingChanged from sending another RPC while applying received values
        public static bool IsApplyingSync = false;

        public static void Init()
        {
            _rpc = NetworkManager.Instance.AddRPC(
                "ConfigSync",
                OnServerReceive,
                OnClientReceive
            );

            PluginConfig.configDamageStructuresEnable.SettingChanged += OnSettingChanged;
            PluginConfig.configDamagePlayersEnable.SettingChanged += OnSettingChanged;
            PluginConfig.configDamageFallEnable.SettingChanged += OnSettingChanged;
            PluginConfig.configDamageMonstersEnable.SettingChanged += OnSettingChanged;
            PluginConfig.configTamedLoxDamageFriendliesEnable.SettingChanged += OnSettingChanged;
            PluginConfig.configTamedLoxSpeedThreshold.SettingChanged += OnSettingChanged;
            PluginConfig.configTamedLoxTurningSpeed.SettingChanged += OnSettingChanged;
            PluginConfig.configTamedLoxDamageBoxPosition.SettingChanged += OnSettingChanged;
            PluginConfig.configTamedLoxDamageBoxScale.SettingChanged += OnSettingChanged;
        }

        private static void OnSettingChanged(object sender, EventArgs e)
        {
            if (IsApplyingSync) return;
            if (ZNet.instance == null || ZNet.instance.IsServer()) return;

            Jotunn.Logger.LogInfo("[ConfigRPC] Setting changed on client, sending to server.");
            _rpc.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), BuildPackage());
        }

        private static ZPackage BuildPackage()
        {
            ZPackage pkg = new ZPackage();
            pkg.Write(PluginConfig.configDamageStructuresEnable.Value);
            pkg.Write(PluginConfig.configDamagePlayersEnable.Value);
            pkg.Write(PluginConfig.configDamageFallEnable.Value);
            pkg.Write(PluginConfig.configDamageMonstersEnable.Value);
            pkg.Write(PluginConfig.configTamedLoxDamageFriendliesEnable.Value);
            pkg.Write((double)PluginConfig.configTamedLoxSpeedThreshold.Value);
            pkg.Write((double)PluginConfig.configTamedLoxTurningSpeed.Value);
            pkg.Write(PluginConfig.configTamedLoxDamageBoxPosition.Value);
            pkg.Write(PluginConfig.configTamedLoxDamageBoxScale.Value);
            return pkg;
        }

        private static void ApplyPackage(ZPackage pkg)
        {
            PluginConfig.configDamageStructuresEnable.Value = pkg.ReadBool();
            PluginConfig.configDamagePlayersEnable.Value = pkg.ReadBool();
            PluginConfig.configDamageFallEnable.Value = pkg.ReadBool();
            PluginConfig.configDamageMonstersEnable.Value = pkg.ReadBool();
            PluginConfig.configTamedLoxDamageFriendliesEnable.Value = pkg.ReadBool();
            PluginConfig.configTamedLoxSpeedThreshold.Value = (float)pkg.ReadDouble();
            PluginConfig.configTamedLoxTurningSpeed.Value = (float)pkg.ReadDouble();
            PluginConfig.configTamedLoxDamageBoxPosition.Value = pkg.ReadVector3();
            PluginConfig.configTamedLoxDamageBoxScale.Value = pkg.ReadVector3();
        }

        // Server receives updated values from an admin client, applies them, then broadcasts to all clients
        private static IEnumerator OnServerReceive(long sender, ZPackage package)
        {
            Jotunn.Logger.LogInfo($"[ConfigRPC] Received config update from peer {sender}, applying and broadcasting.");
            IsApplyingSync = true;
            try
            {
                ApplyPackage(package);
                _rpc.SendPackage(ZNet.instance.m_peers, BuildPackage());
            }
            finally
            {
                IsApplyingSync = false;
            }
            yield break;
        }

        // Client receives the broadcast from the server and applies the values
        private static IEnumerator OnClientReceive(long sender, ZPackage package)
        {
            Jotunn.Logger.LogInfo("[ConfigRPC] Received config sync from server, applying.");
            IsApplyingSync = true;
            try
            {
                ApplyPackage(package);
            }
            finally
            {
                IsApplyingSync = false;
            }
            yield break;
        }
    }
}
