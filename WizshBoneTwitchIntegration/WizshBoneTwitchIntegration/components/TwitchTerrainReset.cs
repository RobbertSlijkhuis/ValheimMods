using System;
using System.Collections;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchTerrainReset : MonoBehaviour
    {
        private ZNetView m_netView;

        private const string KeyStarted  = "TerrainReset_Started_WBTI";
        private const string KeyDuration = "TerrainReset_Duration_WBTI";
        private const string KeyPosX     = "TerrainReset_PosX_WBTI";
        private const string KeyPosY     = "TerrainReset_PosY_WBTI";
        private const string KeyPosZ     = "TerrainReset_PosZ_WBTI";
        private const string KeyRadius   = "TerrainReset_Radius_WBTI";

        public void Awake()
        {
            try
            {
                m_netView = gameObject.GetComponent<ZNetView>();

                if (m_netView == null || !m_netView.IsValid())
                    return;

                string started = m_netView.GetZDO().GetString(KeyStarted, "");
                if (started == "")
                    return;

                float duration   = m_netView.GetZDO().GetFloat(KeyDuration, 0f);
                float timePassed = (float)DateTime.Now.Subtract(DateTime.Parse(started)).TotalSeconds;
                float remaining  = duration - timePassed;

                if (remaining <= 0f)
                    ApplyResetAndDestroy();
                else
                    StartCoroutine(ResetAfterDelay(remaining));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchTerrainReset.Start failed: " + e);
            }
        }

        public void Initialize(Vector3 targetPos, float radius, float duration)
        {
            ZDO zdo = m_netView.GetZDO();

            zdo.Set(KeyStarted,  DateTime.Now.ToString());
            zdo.Set(KeyDuration, duration);
            zdo.Set(KeyPosX,     targetPos.x);
            zdo.Set(KeyPosY,     targetPos.y);
            zdo.Set(KeyPosZ,     targetPos.z);
            zdo.Set(KeyRadius,   radius);

            StartCoroutine(ResetAfterDelay(duration));
        }

        private IEnumerator ResetAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ApplyResetAndDestroy();
        }

        private void ApplyResetAndDestroy()
        {
            try
            {
                ZDO zdo = m_netView.GetZDO();

                Vector3 pos = new Vector3(
                    zdo.GetFloat(KeyPosX,   transform.position.x),
                    zdo.GetFloat(KeyPosY,   transform.position.y),
                    zdo.GetFloat(KeyPosZ,   transform.position.z)
                );
                float radius = zdo.GetFloat(KeyRadius, 8f);

                TerrainComp terrainComp = TerrainComp.FindTerrainCompiler(pos);
                if (terrainComp == null)
                {
                    Jotunn.Logger.LogWarning("TwitchTerrainReset: No TerrainComp found when resetting terrain.");
                    return;
                }

                TerrainEditHelper.ResetTerrainToSeed(pos, radius);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchTerrainReset.ApplyResetAndDestroy failed: " + e);
            }
            finally
            {
                m_netView.Destroy();
            }
        }
    }
}