using ModularMagic_BloodMagic.Configs;
using ModularMagic_BloodMagic.Models;
using UnityEngine;

namespace ModularMagic_BloodMagic.Components
{
    /// <summary>
    /// Attached to every apparition creature prefab.
    /// Stores the spawning scythe name in its ZDO so options survive
    /// teleport/reload cycles and are reapplied in Awake.
    /// Damage scaling is handled by ApparitionPatches.CharacterDamage_Prefix.
    /// </summary>
    internal class ApparitionController : MonoBehaviour
    {
        private const float ApparitionBaseDamage = 150f;

        private static readonly int ZDOKey = "ApparitionData_MMBM".GetStableHashCode();

        private ZNetView? m_netView;
        private string    m_weaponName = "";

        private void Awake()
        {
            m_netView = GetComponent<ZNetView>();

            if (m_netView == null || m_netView.GetZDO() == null)
            {
                Jotunn.Logger.LogError("ApparitionController: could not find ZNetView in Awake.");
                return;
            }

            m_weaponName = m_netView.GetZDO().GetString(ZDOKey, "");

            if (string.IsNullOrEmpty(m_weaponName))
                return;

            ApparitionOptions? options = GetOptions();
            if (options == null)
                return;

            ApplyVisuals(options);
            ApplyHealth(options);
        }

        /// <summary>
        /// Called by SpawnApparitionCoroutine immediately after Instantiate.
        /// Writes the weapon name to the ZDO and applies all options.
        /// </summary>
        public void SetData(string weaponName)
        {
            m_weaponName = weaponName;
            m_netView    = m_netView != null ? m_netView : GetComponent<ZNetView>();

            if (m_netView == null || m_netView.GetZDO() == null)
            {
                Jotunn.Logger.LogError("ApparitionController: could not find ZNetView in SetData.");
                return;
            }

            m_netView.GetZDO().Set(ZDOKey, weaponName);

            ApparitionOptions? options = GetOptions();
            if (options == null)
                return;

            ApplyVisuals(options);
            ApplyHealth(options);
            SetupFollower();
        }

        private void SetupFollower()
        {
            Tameable? tameable = GetComponent<Tameable>();
            if (tameable == null)
            {
                Jotunn.Logger.LogError("ApparitionController: Tameable not found in SetupFollower.");
                return;
            }

            tameable.m_monsterAI.MakeTame();

            if (Player.m_localPlayer != null)
                tameable.Command(Player.m_localPlayer, message: false);
            else
                Jotunn.Logger.LogWarning("ApparitionController: SetupFollower called but Player.m_localPlayer is null.");

            CharacterDrop? characterDrop = GetComponent<CharacterDrop>();
            if (characterDrop != null)
                characterDrop.m_drops.Clear();
        }

        public ApparitionOptions? GetOptions()
        {
            if (m_weaponName == PluginConfig.scythe1Name)
                return new ApparitionOptions
                {
                    health           = PluginConfig.scythe1ApparitionHealth.Value,
                    damageMultiplier = PluginConfig.scythe1ApparitionDamage.Value / ApparitionBaseDamage,
                    emissionColor    = new Color(0f, 2.666667f, 2.996078f),
                    size             = new Vector3(0.75f, 0.75f, 0.75f)
                };

            if (m_weaponName == PluginConfig.scythe2Name)
                return new ApparitionOptions
                {
                    health           = PluginConfig.scythe2ApparitionHealth.Value,
                    damageMultiplier = PluginConfig.scythe2ApparitionDamage.Value / ApparitionBaseDamage,
                    emissionColor    = new Color(2.839216f, 0f, 4f),
                    size             = new Vector3(0.875f, 0.875f, 0.875f)
                };

            if (m_weaponName == PluginConfig.scythe3Name)
                return new ApparitionOptions
                {
                    health           = PluginConfig.scythe3ApparitionHealth.Value,
                    damageMultiplier = PluginConfig.scythe3ApparitionDamage.Value / ApparitionBaseDamage,
                    emissionColor    = new Color(4f, 0.03725543f, 0f),
                    size             = new Vector3(1f, 1f, 1f)
                };

            if (m_weaponName == PluginConfig.scythe4Name)
                return new ApparitionOptions
                {
                    health           = PluginConfig.scythe4ApparitionHealth.Value,
                    damageMultiplier = PluginConfig.scythe4ApparitionDamage.Value / ApparitionBaseDamage,
                    emissionColor    = new Color(0.43921569f, 4, 0),
                    size             = new Vector3(0.75f, 0.75f, 0.75f)
                };

            Jotunn.Logger.LogWarning($"ApparitionController: no options found for weapon '{m_weaponName}'.");
            return null;
        }

        private void ApplyHealth(ApparitionOptions options)
        {
            m_netView!.GetZDO().Set("max_health", options.health);
            m_netView!.GetZDO().Set("health",     options.health);
        }

        private void ApplyVisuals(ApparitionOptions options)
        {
            transform.localScale = options.size;

            Material charredMat = ModularMagic_BloodMagic.Instance.materials.Charred;

            foreach (SkinnedMeshRenderer renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Material instancedMat = new Material(charredMat);
                instancedMat.EnableKeyword("_EMISSION");
                instancedMat.SetColor("_EmissionColor", options.emissionColor);
                renderer.materials = new Material[] { instancedMat };
            }

            foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>())
            {
                ParticleSystem.MainModule main = ps.main;
                main.startColor = options.emissionColor;
            }
        }
    }
}