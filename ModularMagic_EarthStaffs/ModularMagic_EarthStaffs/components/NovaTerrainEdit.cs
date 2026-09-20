using Jotunn.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Components
{
    internal class NovaTerrainEdit : MonoBehaviour
    {
        EffectList m_terrainEffects = new EffectList();

        public void Awake()
        {
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

            Trigger();
        }

        public void Trigger()
        {
            m_terrainEffects.Create(transform.position, transform.rotation);
        }
    }
}
