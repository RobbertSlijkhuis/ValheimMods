using System;
using System.IO;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Resolves the effective root directory for this mod's custom config (profiles,
    /// banned.txt, viewers.yaml) - normally WizshBoneTwitchIntegration.customConfigPath,
    /// resolved against Environment.CurrentDirectory (the Valheim install dir for a Steam
    /// launch).
    ///
    /// Falls back to the location Windows' legacy-app File System Virtualization would have
    /// silently redirected writes to (%LOCALAPPDATA%\VirtualStore\...) when the primary folder
    /// isn't found but a virtualized copy is - e.g. because the game folder sits under
    /// Program Files without write access for the current user. See CLAUDE.md's "Live redeem
    /// profiles" section for background.
    /// </summary>
    internal static class ConfigPathHelper
    {
        private static string m_resolvedRoot;

        /// <summary>
        /// Resolved once and cached for the session, so the Directory.Exists checks (and the
        /// fallback warning, if any) only happen on first use - every caller after that agrees
        /// on the same effective root.
        /// </summary>
        public static string GetEffectiveRoot()
        {
            if (m_resolvedRoot != null)
                return m_resolvedRoot;

            string primary = Path.GetFullPath(WizshBoneTwitchIntegration.customConfigPath);

            if (Directory.Exists(primary))
                return m_resolvedRoot = primary;

            string fallback = GetVirtualStoreFallback(primary);

            if (fallback != null && Directory.Exists(fallback))
            {
                Jotunn.Logger.LogWarning(
                    $"[WBTI] Config folder not found at '{primary}' - found a copy under '{fallback}' " +
                    "(Windows likely redirected writes there because the game folder isn't writable). " +
                    "Using it instead. Consider giving your Valheim install folder write access to avoid this.");
                return m_resolvedRoot = fallback;
            }

            return m_resolvedRoot = primary;
        }

        /// <summary>
        /// Windows' virtualization mapping for a redirected path: the drive letter/colon is
        /// stripped and the rest of the path is preserved under %LOCALAPPDATA%\VirtualStore.
        /// Returns null for a path that isn't drive-rooted (e.g. a UNC path), which virtualization
        /// doesn't apply to anyway.
        /// </summary>
        private static string GetVirtualStoreFallback(string primaryFullPath)
        {
            if (primaryFullPath.Length < 3 || primaryFullPath[1] != ':')
                return null;

            string withoutDrive = primaryFullPath.Substring(3);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            return Path.Combine(localAppData, "VirtualStore", withoutDrive);
        }
    }
}
