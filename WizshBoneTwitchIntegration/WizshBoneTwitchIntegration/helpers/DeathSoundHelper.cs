using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace WizshBoneTwitchIntegration.Helpers
{
    // SAPHONETTE-CLEANUP: temporary hack for Saphonette's stream, not a permanent feature. Plays a
    // custom sound (TEMP/UUH.wav) whenever the local player dies. Triggered from a postfix on the
    // private Player.CreateDeathEffects() (see harmony/MiscPatchesWBTI.cs) - the same method that
    // fires ragdoll/vanilla death effects, and which Player.OnDeath() only ever calls on the owning
    // client, so this needs no IsOwner check of its own. Hooking the real firing point (rather than
    // baking an extra entry into player.m_deathEffects back at Player.Awake) means Enabled is read
    // fresh at the moment of death, so "!toggle deathsound" takes effect immediately - no
    // respawn/relog required. Delete this file, its call site in harmony/MiscPatchesWBTI.cs, and the
    // TEMP/UUH.wav build/publish wiring in the .csproj and publish.ps1, once the bit is over.
    internal static class DeathSoundHelper
    {
        private const string FileName = "UUH.wav";
        private static GameObject s_effectPrefab;
        private static AudioClip s_clip;
        private static bool s_loading;

        // SAPHONETTE-CLEANUP: runtime-only toggle, flipped via SpecialRedeemHelper's "!toggle
        // deathsound" chat command - see TryHandleToggleCommand. Reset to enabled on every launch.
        // Read live at the moment of death (see PlayIfEnabled/PlayWhenReady), so toggling takes
        // effect immediately rather than only on the next respawn.
        public static bool Enabled = true;

        // SAPHONETTE-CLEANUP: separate runtime toggle for the "!nik" chat command (anyone can type
        // it, see SpecialRedeemHelper.TryHandleNikCommand) - kept independent from Enabled so the
        // real death-sound and the chat-triggered one can be switched off separately. Defaults to
        // off - must be turned on with "!toggle nik" each launch. Remove alongside the rest of the
        // death sound bit.
        public static bool NikCommandEnabled = false;

        public static void PlayIfEnabled(Player player)
        {
            if (!Enabled || player == null)
                return;

            WizshBoneTwitchIntegration.Instance.StartCoroutine(PlayWhenReady(player, () => Enabled));
        }

        // SAPHONETTE-CLEANUP: "!nik" chat command - plays the same sound at the local player's
        // position, gated by NikCommandEnabled instead of Enabled. Remove alongside the rest of the
        // death sound bit.
        public static void PlayForNikCommand()
        {
            if (!NikCommandEnabled || Player.m_localPlayer == null)
                return;

            WizshBoneTwitchIntegration.Instance.StartCoroutine(PlayWhenReady(Player.m_localPlayer, () => NikCommandEnabled));
        }

        private static IEnumerator PlayWhenReady(Player player, Func<bool> isEnabled)
        {
            if (s_effectPrefab == null && !s_loading)
                yield return WizshBoneTwitchIntegration.Instance.StartCoroutine(LoadClip());
            else
                while (s_loading)
                    yield return null;

            // Re-check both - the clip can take a moment to load (only matters for the very first
            // play after launch), and isEnabled() may have been toggled off again in the meantime.
            if (s_effectPrefab == null || player == null || !isEnabled())
                yield break;

            GameObject instance = UnityEngine.Object.Instantiate(s_effectPrefab, player.transform.position, Quaternion.identity);

            // Nothing else cleans this clone up (EffectList.Create doesn't either - it just
            // Instantiates and leaves lifecycle to the prefab) - self-destroy once playback's done.
            UnityEngine.Object.Destroy(instance, (s_clip != null ? s_clip.length : 5f) + 1f);
        }

        private static IEnumerator LoadClip()
        {
            s_loading = true;

            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(pluginFolder, FileName);

            if (!File.Exists(path))
            {
                Jotunn.Logger.LogWarning($"[WBTI] DeathSoundHelper: sound file not found at {path}");
                s_loading = false;
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Jotunn.Logger.LogWarning($"[WBTI] DeathSoundHelper: failed to load {FileName}: {request.error}");
                    s_loading = false;
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

                GameObject prefab = new GameObject("WBTI_DeathSound");
                UnityEngine.Object.DontDestroyOnLoad(prefab);

                AudioSource audioSource = prefab.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D - always audible regardless of camera/listener position

                ZSFX zsfx = prefab.AddComponent<ZSFX>();
                zsfx.m_audioClips = new[] { clip };
                zsfx.m_playOnAwake = false; // stays off through the template's own one-time Start() below

                // The template itself is a live GameObject (not a real asset-database prefab), so it
                // runs its own Awake/Start once just like any clone would - let that happen harmlessly
                // with playback still disarmed, then arm it for every clone instantiated afterwards
                // (Instantiate() copies m_playOnAwake off the template at clone time).
                yield return null;
                zsfx.m_playOnAwake = true;

                s_effectPrefab = prefab;
                s_clip = clip;
            }

            s_loading = false;
        }
    }
}
