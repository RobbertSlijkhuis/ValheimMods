using Jotunn.Managers;
using System.Collections;
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

        public static IEnumerator AttachFlashBang(FlashBangData flashbangData)
        {
            yield return new WaitForSeconds(flashbangData.delay);

            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            if (customRewards.m_playerIsInSafeZone == true || Player.m_localPlayer.IsTeleporting())
                yield break;

            GameObject bombBlobFrost = PrefabManager.Instance.GetPrefab("BombBlob_Frost_projectile");
            GameObject flashBang = GameObject.Instantiate(bombBlobFrost, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation);
            flashBang.transform.localPosition = TransformHelper.UpdateSpawnLocation(SpawnPositionType.InFrontOfPlayer, flashBang.transform, new PositionOffsetData() { y = 1f });
            flashBang.transform.localRotation = TransformHelper.UpdateSpawnRotation(SpawnPositionType.InFrontOfPlayer, flashBang.transform);
            flashBang.transform.localScale = new Vector3(2f, 2f, 2f);
            Projectile projectile = flashBang.GetComponent<Projectile>();
            projectile.m_spawnOnHit = null;

            yield return new WaitForSeconds(0.5f);
            yield return new WaitForEndOfFrame();

            GameObject camera = GameCamera.instance.gameObject;
            GameObject flash = GameObject.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.Flashbang, camera.transform);
            Transform canvasAfterImageTransform = flash.transform.Find("Canvas_AfterImage");
            Transform canvasFlashTransform = flash.transform.Find("Canvas_Flash");
            Transform afterImageTransform = canvasAfterImageTransform.transform.Find("AfterImage");
            Transform flashTransform = canvasFlashTransform.transform.Find("Flash");
            Transform soundTransform = flash.transform.Find("SFX");
            CanvasGroup canvasAfterImageGroup = canvasAfterImageTransform.gameObject.GetComponent<CanvasGroup>();
            CanvasGroup canvasFlashGroup = canvasFlashTransform.gameObject.GetComponent<CanvasGroup>();

            ZSFX zsfx = soundTransform.gameObject.GetComponent<ZSFX>();
            zsfx.m_maxVol = flashbangData.soundVolume;
            zsfx.m_minVol = flashbangData.soundVolume;

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            Sprite img = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);

            Image afterImage = afterImageTransform.gameObject.GetComponent<Image>();
            Image flashImage = flashTransform.gameObject.GetComponent<Image>();
            afterImage.sprite = img;

            Color color;
            ColorUtility.TryParseHtmlString(flashbangData.flashColor, out color);
            flashImage.color = color;

            if (flashbangData.flashStartDuration == 0f)
            {
                canvasFlashGroup.alpha = 1f;
            }
            else
            {
                Player.m_localPlayer.StartCoroutine(LerpHelper.LerpCanvasGroup(canvasFlashGroup, 0f, 1f, flashbangData.flashStartDuration));
                yield return new WaitForSeconds(flashbangData.flashStartDuration);
            }

            EnemyHud.instance.gameObject.SetActive(false);
            MessageHud.instance.gameObject.SetActive(false);
            Hud.instance.gameObject.SetActive(false);
            GameObject.Destroy(flashBang);

            yield return new WaitForSeconds(flashbangData.flashDuration);

            canvasAfterImageGroup.alpha = 1f;
            Player.m_localPlayer.StartCoroutine(LerpHelper.LerpCanvasGroup(canvasFlashGroup, 1f, 0f, flashbangData.flashEndDuration));

            yield return new WaitForSeconds(1f);

            Player.m_localPlayer.StartCoroutine(LerpHelper.LerpCanvasGroup(canvasAfterImageGroup, 1f, 0f, flashbangData.flashEndDuration));
            GameObject.Destroy(flash, flashbangData.flashEndDuration + 1f);

            yield return new WaitForSeconds(flashbangData.flashEndDuration / 2);

            EnemyHud.instance.gameObject.SetActive(true);
            MessageHud.instance.gameObject.SetActive(true);
            Hud.instance.gameObject.SetActive(true);
        }

        public static bool ClearUI()
        {
            GameObject camera = GameCamera.instance.gameObject;
            Transform flashbangTrans = camera.transform.Find("Flashbang_WBTI");

            if (flashbangTrans == null)
                return false;

            GameObject.Destroy(flashbangTrans.gameObject);
            return true;
        }
    }
}
