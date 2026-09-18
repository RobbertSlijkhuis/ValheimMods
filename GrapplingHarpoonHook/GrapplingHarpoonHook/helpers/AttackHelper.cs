using Jotunn.Managers;
using UnityEngine;

namespace GrapplingHarpoonHook.Helpers
{
    internal static class AttackHelper
    {
        private const string CloneSuffix = "(Clone)";
        private static string _hookItemName;

        internal static void ConfigureGrapplingHookSecondaryAttack()
        {
            GameObject grapplingHook = PrefabManager.Instance.GetPrefab("GrapplingHook");
            if (!grapplingHook)
            {
                Jotunn.Logger.LogError("AttackSetup: could not find GrapplingHook prefab.");
                return;
            }

            var hookShared = grapplingHook.GetComponent<ItemDrop>().m_itemData.m_shared;
            var hookSecondary = hookShared.m_secondaryAttack;
            if (hookSecondary == null)
            {
                Jotunn.Logger.LogError("AttackSetup: GrapplingHook has no secondary attack to modify.");
                return;
            }

            hookSecondary.m_attackProjectile = GrapplingHarpoonHook.Instance.prefabs.HarpoonProjectile;
            hookSecondary.m_spawnOnHit = null;
            hookSecondary.m_requiresReload = hookShared.m_attack.m_requiresReload;

            _hookItemName = hookShared.m_name;
        }

        // Projectile.Setup replaces the projectile's status effect with the (empty) one from the weapon's hit data.
        // Only restore it for our secondary projectile, fired by the GrapplingHook item.
        internal static void RestoreHarpoonEffect(Projectile projectile, ItemDrop.ItemData weapon)
        {
            if (!IsGrapplingHookSecondaryShot(projectile, weapon))
            {
                return;
            }

            string effectName = GrapplingHarpoonHook.Instance.effects.Harpoon.name;
            projectile.m_statusEffect = effectName;
            projectile.m_statusEffectHash = effectName.GetStableHashCode();
        }

        private static bool IsGrapplingHookSecondaryShot(Projectile projectile, ItemDrop.ItemData weapon)
        {
            if (!GrapplingHarpoonHook.Instance.prefabs.HarpoonProjectile || !GrapplingHarpoonHook.Instance.effects.Harpoon)
            {
                return false;
            }

            if (weapon?.m_shared == null || weapon.m_shared.m_name != _hookItemName)
            {
                return false;
            }

            string name = projectile.gameObject.name;
            if (name.EndsWith(CloneSuffix))
            {
                name = name.Substring(0, name.Length - CloneSuffix.Length);
            }

            return name == GrapplingHarpoonHook.Instance.prefabs.HarpoonProjectile.name;
        }
    }
}
