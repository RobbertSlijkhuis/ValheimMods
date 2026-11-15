using ModularMagic_Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;

namespace ModularMagic_IceStaffs.Helpers
{
    /// <summary>
    /// The ice sheet, and the splash that goes with it, that the frost shots of the Ice staffs make on the water they hit. It is the effect
    /// of the Utility rune: without the rune (and inside a dungeon) a water hit makes neither. They are no longer water hit effects of the
    /// projectile prefab, they are created here when the projectile hits the water, on the client of the shooter (the only client that
    /// runs the hit).
    /// </summary>
    internal static class IceSheetHelper
    {
        private static string _projectileName;
        private static EffectList _sheetEffect;

        /// <summary>
        /// Takes the sheet and its splash out of the water hit effects of the projectile prefab and remembers them, with the settings they
        /// had there, so they are created the same way the projectile created them. Call it once at startup.
        /// </summary>
        public static void Init(GameObject projectilePrefab, params GameObject[] sheetPrefabs)
        {
            try
            {
                Projectile projectile = projectilePrefab.GetComponent<Projectile>();
                EffectList.EffectData[] effects = projectile.m_hitWaterEffects.m_effectPrefabs;
                HashSet<string> sheetNames = new HashSet<string>(sheetPrefabs.Select(p => p.name));

                EffectList.EffectData[] sheetEffects = effects.Where(e => e.m_prefab != null && sheetNames.Contains(e.m_prefab.name)).ToArray();
                projectile.m_hitWaterEffects.m_effectPrefabs = effects.Except(sheetEffects).ToArray();

                _sheetEffect = new EffectList() { m_effectPrefabs = sheetEffects };
                _projectileName = projectilePrefab.name;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not set up the ice sheet: " + e);
            }
        }

        /// <summary>
        /// Called after a projectile hit something. Creates the sheet and its splash when the projectile is a frost shot of the Ice staffs that hit
        /// the water, the staff has the Utility rune and the hit is not in a dungeon.
        /// </summary>
        public static void OnProjectileHit(Projectile projectile, ItemData weapon, Vector3 hitPoint, bool water)
        {
            if (!water || _sheetEffect == null || weapon == null)
                return;

            if (Utils.GetPrefabName(projectile.gameObject) != _projectileName)
                return;

            if (Character.InInterior(hitPoint))
                return;

            if (CoreImbuementHelper.GetLevel(weapon, ImbuementType.Utility) <= 0)
                return;

            _sheetEffect.Create(hitPoint, Quaternion.identity);
        }
    }
}
