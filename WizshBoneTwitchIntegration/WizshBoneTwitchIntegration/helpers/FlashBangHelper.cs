using Jotunn.Managers;
using System.Collections;
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

            Vector3 spawnPosition = TransformHelper.UpdateSpawnLocation(SpawnPositionType.InFrontOfPlayer, Player.m_localPlayer.transform, new PositionOffsetData() { y = 1f });
            Quaternion spawnRotation = TransformHelper.UpdateSpawnRotation(SpawnPositionType.InFrontOfPlayer, Player.m_localPlayer.transform.rotation);
            GameObject flashBang = GameObject.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.FlashbangVial, spawnPosition, spawnRotation);

            yield return new WaitForSeconds(0.5f);
            yield return new WaitForEndOfFrame();

            if (GameCamera.instance == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: GameCamera.instance is null, aborting.");
                ZNetViewHelper.Destroy(flashBang);
                yield break;
            }

            GameObject camera = GameCamera.instance.gameObject;
            GameObject flash = GameObject.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.Flashbang, camera.transform);
            Transform canvasAfterImageTransform = flash.transform.Find("Canvas_AfterImage");
            Transform canvasFlashTransform = flash.transform.Find("Canvas_Flash");

            if (canvasAfterImageTransform == null || canvasFlashTransform == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: Could not find canvas transforms on Flashbang prefab, aborting.");
                ZNetViewHelper.Destroy(flashBang);
                ZNetViewHelper.Destroy(flash);
                yield break;
            }

            Transform afterImageTransform = canvasAfterImageTransform.Find("AfterImage");
            Transform flashTransform = canvasFlashTransform.Find("Flash");
            Transform soundTransform = flash.transform.Find("SFX");

            if (afterImageTransform == null || flashTransform == null || soundTransform == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: Could not find child transforms on Flashbang prefab, aborting.");
                ZNetViewHelper.Destroy(flashBang);
                ZNetViewHelper.Destroy(flash);
                yield break;
            }

            CanvasGroup canvasAfterImageGroup = canvasAfterImageTransform.gameObject.GetComponent<CanvasGroup>();
            CanvasGroup canvasFlashGroup = canvasFlashTransform.gameObject.GetComponent<CanvasGroup>();

            if (canvasAfterImageGroup == null || canvasFlashGroup == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: Could not find CanvasGroups on Flashbang prefab, aborting.");
                ZNetViewHelper.Destroy(flashBang);
                ZNetViewHelper.Destroy(flash);
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

            if (flashbangData.flashStartDuration == 0f)
            {
                canvasFlashGroup.alpha = 1f;
            }
            else
            {
                Player.m_localPlayer.StartCoroutine(LerpHelper.LerpCanvasGroup(canvasFlashGroup, 0f, 1f, flashbangData.flashStartDuration));
                yield return new WaitForSeconds(flashbangData.flashStartDuration);
            }

            if (EnemyHud.instance != null) EnemyHud.instance.gameObject.SetActive(false);
            if (MessageHud.instance != null) MessageHud.instance.gameObject.SetActive(false);
            if (Hud.instance != null) Hud.instance.gameObject.SetActive(false);
            ZNetViewHelper.Destroy(flashBang);

            yield return new WaitForSeconds(flashbangData.flashDuration);

            canvasAfterImageGroup.alpha = 1f;
            Player.m_localPlayer.StartCoroutine(LerpHelper.LerpCanvasGroup(canvasFlashGroup, 1f, 0f, flashbangData.flashEndDuration));

            yield return new WaitForSeconds(1f);

            Player.m_localPlayer.StartCoroutine(LerpHelper.LerpCanvasGroup(canvasAfterImageGroup, 1f, 0f, flashbangData.flashEndDuration));
            ZNetViewHelper.Destroy(flash, flashbangData.flashEndDuration + 1f);

            yield return new WaitForSeconds(flashbangData.flashEndDuration / 2);

            if (EnemyHud.instance != null) EnemyHud.instance.gameObject.SetActive(true);
            if (MessageHud.instance != null) MessageHud.instance.gameObject.SetActive(true);
            if (Hud.instance != null) Hud.instance.gameObject.SetActive(true);
        }

        public static bool ClearUI()
        {
            if (GameCamera.instance == null)
            {
                Jotunn.Logger.LogWarning("FlashBangHelper: GameCamera.instance is null, cannot clear UI.");
                return false;
            }

            GameObject camera = GameCamera.instance.gameObject;
            Transform flashbangTrans = camera.transform.Find("Flashbang_WBTI");

            if (flashbangTrans == null)
                return false;

            ZNetViewHelper.Destroy(flashbangTrans.gameObject);
            return true;
        }
    }
}
