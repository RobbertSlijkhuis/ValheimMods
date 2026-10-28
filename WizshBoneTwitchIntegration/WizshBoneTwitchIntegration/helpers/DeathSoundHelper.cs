using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// Loads the audio files in the deathsounds folder and plays a random one when the local
    /// player dies (trigger: <c>DeathSoundPatchesWBTI</c>). A purely per-client effect - nothing
    /// is networked, other players never hear it.
    ///
    /// Volume: the sound plays through an AudioSource routed to AudioMan's GUI mixer group.
    /// Vanilla's AudioMan.SetSFXVolume sets that group's "GuiVol" together with "SfxVol" to
    /// master * sfx, so Valheim's own volume sliders apply automatically; the profile's
    /// deathSoundVolume is a multiplier on top of that.
    /// </summary>
    internal static class DeathSoundHelper
    {
        public const float MaxPlaySeconds = 10f;

        private static readonly string[] SupportedExtensions = { ".wav", ".ogg", ".mp3" };

        private static List<AudioClip> s_clips = new List<AudioClip>();
        private static int s_generation;

        public static string FolderPath => ConfigPathHelper.GetEffectiveRoot() + "/deathsounds";

        public static int LoadedCount => s_clips.Count;

        /// <summary>Raised on the main thread each time a (re)load finishes.</summary>
        public static event Action OnReloaded;

        public static void EnsureFolderExists()
        {
            try
            {
                Directory.CreateDirectory(FolderPath);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Failed to create the death sounds folder '{FolderPath}': {e.Message}");
            }
        }

        /// <summary>
        /// Rescans the folder and replaces the loaded clips. Safe to call while the game runs: the
        /// old clips stay playable until the new list is complete, and a reload started while
        /// another is still running makes the older one discard its result.
        /// </summary>
        public static void Reload()
        {
            WizshBoneTwitchIntegration.Instance.StartCoroutine(LoadAll(++s_generation));
        }

        private static IEnumerator LoadAll(int generation)
        {
            List<AudioClip> loaded = new List<AudioClip>();

            string[] files = FindFiles();
            foreach (string file in files)
            {
                AudioType type = GetAudioType(file);

                using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(file).AbsoluteUri, type))
                {
                    yield return request.SendWebRequest();

                    if (generation != s_generation)
                    {
                        DestroyClips(loaded);
                        yield break;
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Jotunn.Logger.LogWarning($"[WBTI] Could not load death sound '{Path.GetFileName(file)}': {request.error}");
                        continue;
                    }

                    AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                    if (clip == null || clip.length <= 0f)
                    {
                        Jotunn.Logger.LogWarning($"[WBTI] Death sound '{Path.GetFileName(file)}' is empty or unreadable, skipping it.");
                        continue;
                    }

                    clip.name = Path.GetFileName(file);
                    loaded.Add(clip);
                }
            }

            // A clip may still be playing from the old list - give it the max play time before it is destroyed.
            foreach (AudioClip old in s_clips)
                UnityEngine.Object.Destroy(old, MaxPlaySeconds);

            s_clips = loaded;

            Jotunn.Logger.LogWarning($"[WBTI] Loaded {loaded.Count} death sound(s) from '{FolderPath}'.");
            OnReloaded?.Invoke();
        }

        private static string[] FindFiles()
        {
            try
            {
                if (!Directory.Exists(FolderPath))
                    return new string[0];

                return Directory.GetFiles(FolderPath)
                    .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Failed to read the death sounds folder '{FolderPath}': {e.Message}");
                return new string[0];
            }
        }

        private static AudioType GetAudioType(string file)
        {
            switch (Path.GetExtension(file).ToLowerInvariant())
            {
                case ".ogg": return AudioType.OGGVORBIS;
                case ".mp3": return AudioType.MPEG;
                default: return AudioType.WAV;
            }
        }

        /// <summary>Plays a random loaded sound, if enabled.</summary>
        public static void PlayRandom()
        {
            ProfileSettingsData settings = ProfileSettingsHelper.Current;
            if (!settings.deathSoundsEnabled || s_clips.Count == 0 || AudioMan.instance == null)
                return;

            AudioClip clip = s_clips[UnityEngine.Random.Range(0, s_clips.Count)];

            // Standalone object, not parented to the player: the player GameObject is destroyed a
            // few seconds after death, which would cut the sound off.
            GameObject go = new GameObject("WBTI_DeathSound");
            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.outputAudioMixerGroup = AudioMan.instance.m_guiMixer;
            source.spatialBlend = 0f;
            source.playOnAwake = false;
            source.volume = Mathf.Clamp01(settings.deathSoundVolume);
            source.Play();

            UnityEngine.Object.Destroy(go, Mathf.Min(clip.length, MaxPlaySeconds));
        }

        private static void DestroyClips(List<AudioClip> clips)
        {
            foreach (AudioClip clip in clips)
                UnityEngine.Object.Destroy(clip);
        }
    }
}
