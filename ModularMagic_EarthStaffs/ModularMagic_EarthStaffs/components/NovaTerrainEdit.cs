using Jotunn.Managers;
using ModularMagic_EarthStaffs.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Components
{
    internal class NovaTerrainEdit : MonoBehaviour
    {
        // Seconds until the spikes of fx_staff_earth_nova_MMES burst out: the start delay of its Burst Rocks, rock bursts and shockwave particles.
        // Change this when the timing of the fx changes
        private const float TerrainDelay = 1.7f;

        EffectList m_terrainEffects = new EffectList();

        public void Awake()
        {
            TerrainOpHelper.LogRegistration("nova cast");

            EffectList.EffectData hoeEffect = new EffectList.EffectData();
            hoeEffect.m_enabled = true;
            hoeEffect.m_prefab = PrefabManager.Instance.GetPrefab("vfx_Place_mud_road");
            hoeEffect.m_variant = -1;

            EffectList.EffectData mudRoadEffect = new EffectList.EffectData();
            mudRoadEffect.m_enabled = true;
            mudRoadEffect.m_prefab = ModularMagic_EarthStaffs.prefabs.LevelTerrain;
            mudRoadEffect.m_variant = -1;

            List<EffectList.EffectData> list = new List<EffectList.EffectData>();
            list.Add(hoeEffect);

            // No local player (e.g. still loading in) means we can't tell if it is an interior, so skip the terrain effect
            if (Player.m_localPlayer != null && !Player.m_localPlayer.InInterior())
                list.Add(mudRoadEffect);

            m_terrainEffects.m_effectPrefabs = list.ToArray();

            // The spikes of the nova fx come up after a delay, the terrain is edited at that moment, at the place of the fx then
            Invoke(nameof(Trigger), TerrainDelay);
        }

        public void Trigger()
        {
            m_terrainEffects.Create(transform.position, transform.rotation);
        }
    }
}
