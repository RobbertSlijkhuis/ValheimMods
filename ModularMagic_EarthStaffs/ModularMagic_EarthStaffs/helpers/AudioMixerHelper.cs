using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ModularMagic_EarthStaffs.Helpers
{
    /// <summary>
    /// Jötunn does not resolve mocked AudioMixerGroups, so AudioSources in the bundle keep pointing at the
    /// bundled JVLmock_MasterMixer and ignore the game's SFX volume. This re-routes them to the game's real groups.
    /// </summary>
    internal static class AudioMixerHelper
    {
        private const string MockMixerPrefix = "JVLmock_";
        private const string DefaultGroupName = "SFX";

        /// <summary>
        /// Replaces mock mixer groups (matched by group name) and missing groups (assigned the real SFX group)
        /// on every AudioSource under the given prefabs. Must run after AudioMan exists.
        /// </summary>
        public static void FixMockedGroups(IEnumerable<GameObject> roots, string tag)
        {
            AudioMixer master = AudioMan.instance != null ? AudioMan.instance.m_masterMixer : null;
            if (master == null)
            {
                Jotunn.Logger.LogWarning($"[{tag}] AudioMixerHelper: AudioMan/master mixer not ready, skipping");
                return;
            }

            Dictionary<string, AudioMixerGroup> realGroups = GetGroupsByName(master);
            Jotunn.Logger.LogWarning($"[{tag}] AudioMixerHelper: real groups in '{master.name}': {string.Join(", ", realGroups.Keys)}");

            foreach (GameObject root in roots)
            {
                if (root == null)
                    continue;

                foreach (AudioSource source in root.GetComponentsInChildren<AudioSource>(true))
                {
                    AudioMixerGroup group = source.outputAudioMixerGroup;
                    string wantedName;

                    if (group == null)
                        wantedName = DefaultGroupName;
                    else if (group.audioMixer != null && group.audioMixer.name.StartsWith(MockMixerPrefix))
                        wantedName = group.name;
                    else
                        continue;

                    if (realGroups.TryGetValue(wantedName, out AudioMixerGroup realGroup))
                    {
                        source.outputAudioMixerGroup = realGroup;
                        Jotunn.Logger.LogWarning($"[{tag}] AudioMixerHelper: {root.name}/{source.gameObject.name} -> real group '{wantedName}'");
                    }
                    else
                    {
                        Jotunn.Logger.LogWarning($"[{tag}] AudioMixerHelper: no real group '{wantedName}' for {root.name}/{source.gameObject.name}, left unchanged");
                    }
                }
            }
        }

        // FindMatchingGroups matches on the group path, so query a few ways and de-duplicate by name
        private static Dictionary<string, AudioMixerGroup> GetGroupsByName(AudioMixer mixer)
        {
            Dictionary<string, AudioMixerGroup> groups = new Dictionary<string, AudioMixerGroup>();

            foreach (string subPath in new[] { string.Empty, "Master", "Master/" })
            {
                foreach (AudioMixerGroup group in mixer.FindMatchingGroups(subPath))
                {
                    if (!groups.ContainsKey(group.name))
                        groups[group.name] = group;
                }
            }

            return groups;
        }
    }
}
