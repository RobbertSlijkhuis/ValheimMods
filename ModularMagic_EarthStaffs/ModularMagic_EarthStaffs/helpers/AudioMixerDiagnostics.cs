using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace ModularMagic_EarthStaffs.Helpers
{
    /// <summary>
    /// Temporary diagnostics: logs which mixer group each AudioSource in the mod's prefabs is routed to,
    /// and whether that group belongs to the game's real master mixer or to the mock mixer from the bundle.
    /// </summary>
    internal static class AudioMixerDiagnostics
    {
        public static void Log(AssetBundle bundle, string tag)
        {
            if (bundle == null)
            {
                Jotunn.Logger.LogWarning($"[{tag}] AudioMixerDiagnostics: bundle is null");
                return;
            }

            Log(bundle.LoadAllAssets<GameObject>(), tag);
        }

        public static void Log(IEnumerable<GameObject> roots, string tag)
        {
            AudioMixer master = AudioMan.instance != null ? AudioMan.instance.m_masterMixer : null;
            Jotunn.Logger.LogWarning($"[{tag}] AudioMixerDiagnostics: real master mixer = {(master != null ? master.name : "<AudioMan not ready>")}");

            // One line per distinct (group, mixer) combination to keep the output readable
            Dictionary<string, List<string>> routes = new Dictionary<string, List<string>>();
            int sourceCount = 0;

            foreach (GameObject root in roots)
            {
                if (root == null)
                    continue;

                foreach (AudioSource source in root.GetComponentsInChildren<AudioSource>(true))
                {
                    sourceCount++;
                    AudioMixerGroup group = source.outputAudioMixerGroup;
                    string groupName = group != null ? group.name : "<none>";
                    string mixerName = group != null && group.audioMixer != null ? group.audioMixer.name : "<none>";
                    string isReal = group != null && master != null && group.audioMixer == master ? "REAL" : "NOT-REAL";
                    string key = $"group='{groupName}' mixer='{mixerName}' {isReal}";

                    if (!routes.TryGetValue(key, out List<string> users))
                    {
                        users = new List<string>();
                        routes[key] = users;
                    }
                    users.Add($"{root.name}/{source.gameObject.name}(vol={source.volume:0.##})");
                }
            }

            Jotunn.Logger.LogWarning($"[{tag}] AudioMixerDiagnostics: {sourceCount} AudioSources");
            foreach (KeyValuePair<string, List<string>> route in routes)
            {
                Jotunn.Logger.LogWarning($"[{tag}]   {route.Key} x{route.Value.Count} e.g. {string.Join(", ", route.Value.Take(3))}");
            }
        }
    }
}
