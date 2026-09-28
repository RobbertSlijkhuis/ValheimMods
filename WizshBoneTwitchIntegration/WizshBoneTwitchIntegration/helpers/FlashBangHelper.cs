using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class FlashBangHelper
    {
        public static int width = Screen.width;
        public static int height = Screen.height;

        // Hard backstop for the white-screen-sticks-forever bug: if the effect never reaches its
        // own cleanup (host coroutine killed by a scene change, an exception mid-effect, etc.),
        // this guarantees the overlay and HUD get cleared regardless of what went wrong.
        private const float SafetyTimeoutSeconds = 60f;

        // Ground explosions are placed on a fixed world-space ring around the player (not relative
        // to facing direction), so the effect reads the same no matter which way they're looking.
        private const int GroundExplosionCount = 6;
        private const float GroundExplosionRadius = 3f;

        // flashStartDuration defaults to 0, which snaps the screen to solid white the same frame
        // the ring explosions spawn - before their particle systems ever render a frame. Give the
        // explosions a brief window on screen before the flash takes over.
        private const float GroundExplosionLeadTime = 0.3f;

        private struct QueuedFlashbang
        {
            public FlashBangData data;
            public CustomRewardEvent customRewardEvent;
        }

        private static readonly Queue<QueuedFlashbang> s_queue = new Queue<QueuedFlashbang>();
        private static bool s_isRunning;
        private static GameObject s_activeFlash;

        public static void Enqueue(MonoBehaviour host, FlashBangData flashbangData, CustomRewardEvent customRewardEvent)
        {
            s_queue.Enqueue(new QueuedFlashbang { data = flashbangData, customRewardEvent = customRewardEvent });

            if (!s_isRunning)
                host.StartCoroutine(ProcessQueue(host));
        }

        private static IEnumerator ProcessQueue(MonoBehaviour host)
        {
            s_isRunning = true;
            try
            {
                while (s_queue.Count > 0)
                {
                    QueuedFlashbang next = s_queue.Dequeue();
                    yield return host.StartCoroutine(AttachFlashBang(next.data, next.customRewardEvent));
                }
            }
            finally
            {
                s_queue.Clear();
                s_isRunning = false;
            }
        }

        public static IEnumerator AttachFlashBang(FlashBangData flashbangData, CustomRewardEvent customRewardEvent)
        {
            yield return new WaitForSeconds(flashbangData.delay);

            if (Game.instance == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: Game.instance is null, aborting.");
                yield break;
            }

            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            if (customRewards == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: Could not find TwitchCustomRewards, aborting.");
                yield break;
            }

            if (Player.m_localPlayer == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: Local player is null, aborting.");
                yield break;
            }

            if (customRewards.m_playerIsInSafeZone == true || Player.m_localPlayer.IsTeleporting())
                yield break;

            if (GameCamera.instance == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: GameCamera.instance is null, aborting.");
                yield break;
            }

            SpawnGroundExplosions(Player.m_localPlayer.transform.position, flashbangData.soundVolume);

            yield return new WaitForSeconds(GroundExplosionLeadTime);

            yield return new WaitForEndOfFrame();

            GameObject camera = GameCamera.instance.gameObject;
            GameObject flash = GameObject.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.Flashbang, camera.transform);
            s_activeFlash = flash;
            Game.instance.StartCoroutine(SafetyTimeoutWatchdog(flash));

            Transform canvasAfterImageTransform = flash.transform.Find("Canvas_AfterImage");
            Transform canvasFlashTransform = flash.transform.Find("Canvas_Flash");

            if (canvasAfterImageTransform == null || canvasFlashTransform == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: Could not find canvas transforms on Flashbang prefab, aborting.");
                ZNetViewHelper.Destroy(flash);
                s_activeFlash = null;
                yield break;
            }

            Transform afterImageTransform = canvasAfterImageTransform.Find("AfterImage");
            Transform flashTransform = canvasFlashTransform.Find("Flash");
            Transform soundTransform = flash.transform.Find("SFX");

            if (afterImageTransform == null || flashTransform == null || soundTransform == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: Could not find child transforms on Flashbang prefab, aborting.");
                ZNetViewHelper.Destroy(flash);
                s_activeFlash = null;
                yield break;
            }

            CanvasGroup canvasAfterImageGroup = canvasAfterImageTransform.gameObject.GetComponent<CanvasGroup>();
            CanvasGroup canvasFlashGroup = canvasFlashTransform.gameObject.GetComponent<CanvasGroup>();

            if (canvasAfterImageGroup == null || canvasFlashGroup == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: Could not find CanvasGroups on Flashbang prefab, aborting.");
                ZNetViewHelper.Destroy(flash);
                s_activeFlash = null;
                yield break;
            }

            ZSFX zsfx = soundTransform.gameObject.GetComponent<ZSFX>();
            if (zsfx != null)
            {
                zsfx.m_maxVol = flashbangData.soundVolume;
                zsfx.m_minVol = flashbangData.soundVolume;
            }

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            Sprite img = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);

            Image afterImage = afterImageTransform.gameObject.GetComponent<Image>();
            Image flashImage = flashTransform.gameObject.GetComponent<Image>();

            if (afterImage != null)
                afterImage.sprite = img;

            if (flashImage != null)
            {
                Color color;
                ColorUtility.TryParseHtmlString(flashbangData.flashColor, out color);
                flashImage.color = color;
            }

            // From here on, Hud is hidden and/or the overlay is visible - a C# try/finally (yield
            // return is not allowed inside a try with a catch clause) guarantees that whatever
            // interrupts this coroutine (an exception, the host being torn down mid-yield, etc.)
            // still restores the Hud and clears the overlay instead of leaving the screen stuck.
            bool finishedNormally = false;
            try
            {
                if (flashbangData.flashStartDuration == 0f)
                {
                    canvasFlashGroup.alpha = 1f;
                }
                else
                {
                    Player.m_localPlayer.StartCoroutine(LerpHelper.LerpCanvasGroup(canvasFlashGroup, 0f, 1f, flashbangData.flashStartDuration));
                    yield return new WaitForSeconds(flashbangData.flashStartDuration);
                }

                // flash (and its canvasFlashGroup/canvasAfterImageGroup children) can be destroyed
                // out from under this coroutine while it's suspended - e.g. OnDeath_Postfix/
                // OnSpawned_Postfix call ClearUI() on player death/respawn mid-effect. Touching a
                // destroyed component below throws an uncaught MissingReferenceException (no catch
                // here, only finally), which unwinds through ProcessQueue's finally and silently
                // drops every other flashbang still waiting in s_queue. Bail out cleanly instead.
                if (flash == null)
                    yield break;

                if (EnemyHud.instance != null) EnemyHud.instance.gameObject.SetActive(false);
                if (MessageHud.instance != null) MessageHud.instance.gameObject.SetActive(false);
                if (Hud.instance != null) Hud.instance.gameObject.SetActive(false);

                yield return new WaitForSeconds(flashbangData.flashDuration);

                if (flash == null)
                    yield break;

                canvasAfterImageGroup.alpha = 1f;
                Player.m_localPlayer.StartCoroutine(LerpHelper.LerpCanvasGroup(canvasFlashGroup, 1f, 0f, flashbangData.flashEndDuration));

                yield return new WaitForSeconds(1f);

                if (flash == null)
                    yield break;

                Player.m_localPlayer.StartCoroutine(LerpHelper.LerpCanvasGroup(canvasAfterImageGroup, 1f, 0f, flashbangData.flashEndDuration));
                ZNetViewHelper.Destroy(flash, flashbangData.flashEndDuration + 1f);

                yield return new WaitForSeconds(flashbangData.flashEndDuration / 2);

                if (EnemyHud.instance != null) EnemyHud.instance.gameObject.SetActive(true);
                if (MessageHud.instance != null) MessageHud.instance.gameObject.SetActive(true);
                if (Hud.instance != null) Hud.instance.gameObject.SetActive(true);

                finishedNormally = true;
            }
            finally
            {
                if (!finishedNormally)
                {
                    // Interrupted before the delayed destroy above got scheduled/completed - force
                    // an immediate cleanup instead of leaving the overlay/Hud stuck.
                    Jotunn.Logger.LogWarning("FlashBangHelper: Effect ended abnormally, forcing cleanup.");
                    ZNetViewHelper.Destroy(flash);
                    RestoreHud();
                }

                s_activeFlash = null;
            }
        }

        private static IEnumerator SafetyTimeoutWatchdog(GameObject flash)
        {
            yield return new WaitForSeconds(SafetyTimeoutSeconds);

            if (flash == null)
                yield break;

            Jotunn.Logger.LogWarning("FlashBangHelper: Flashbang UI did not clear within the safety timeout, forcing cleanup.");
            ZNetViewHelper.Destroy(flash);
            RestoreHud();
            s_activeFlash = null;
        }

        // Purely visual/sound - the ring is spectacle for the flashbang, not damage, so any Aoe
        // damage the explosion prefab carries by default is stripped before it can hurt the
        // player standing at the center of the ring. Both prefabs carry their own ZNetView, so
        // ZNetView.Awake() registers and replicates them to every other client regardless of
        // which Instantiate call spawned them - no custom sync needed here.
        private static void SpawnGroundExplosions(Vector3 center, float soundVolume)
        {
            GameObject explosionFX = PrefabManager.Instance.GetPrefab("vfx_BombBlob_explode_frost");
            GameObject explosionSFX = PrefabManager.Instance.GetPrefab("sfx_oozebomb_explode");

            if (explosionFX == null || explosionSFX == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: Could not find explosion prefabs, skipping ground explosions.");
                return;
            }

            bool indoor = Player.m_localPlayer != null && Player.m_localPlayer.InInterior();

            for (int i = 0; i < GroundExplosionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / GroundExplosionCount;
                Vector3 candidate = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * GroundExplosionRadius;

                bool foundGround = TransformHelper.TryGetGroundHeight(candidate, center.y, indoor, out float height);
                if (!foundGround)
                    height = center.y;

                candidate.y = height;

                GameObject explosionInstance = UnityEngine.Object.Instantiate(explosionFX, candidate, Quaternion.identity);
                GameObject soundInstance = UnityEngine.Object.Instantiate(explosionSFX, candidate, Quaternion.identity);

                Aoe[] aoes = explosionInstance.GetComponentsInChildren<Aoe>(true);
                foreach (Aoe aoe in aoes)
                {
                    aoe.m_damage = new HitData.DamageTypes();
                    aoe.m_spawnOnHitTerrain = null;
                }

                ZSFX zsfx = soundInstance.GetComponentInChildren<ZSFX>(true);
                if (zsfx != null)
                {
                    zsfx.m_maxVol = soundVolume;
                    zsfx.m_minVol = soundVolume;
                }
            }
        }

        private static void RestoreHud()
        {
            if (EnemyHud.instance != null) EnemyHud.instance.gameObject.SetActive(true);
            if (MessageHud.instance != null) MessageHud.instance.gameObject.SetActive(true);
            if (Hud.instance != null) Hud.instance.gameObject.SetActive(true);
        }

        public static bool ClearUI()
        {
            bool hadActiveFlash = s_activeFlash != null;

            if (hadActiveFlash)
            {
                ZNetViewHelper.Destroy(s_activeFlash);
                s_activeFlash = null;
            }

            RestoreHud();
            return hadActiveFlash;
        }

        // Called from GameAwake_Postfix so a fresh world load/relog doesn't inherit a queue stuck
        // forever by an abandoned coroutine: AttachFlashBang/ProcessQueue run on the
        // TwitchCustomRewards host attached to Game.instance.gameObject, and if that GameObject is
        // destroyed mid-effect (logout, world change), Unity silently kills the coroutine without
        // ever reaching its finally block - leaving s_isRunning stuck true and every future
        // Enqueue() call permanently skipping StartCoroutine.
        public static void ResetQueue()
        {
            s_queue.Clear();
            s_isRunning = false;
            ClearUI();
        }
    }
}
