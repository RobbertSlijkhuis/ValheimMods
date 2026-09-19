using Jotunn.Managers;
using System;
using UnityEngine;

namespace ModularMagic_EarthStaffs.Helpers
{
    /// <summary>
    /// Creates the projectile prefabs of the damage types. Every damage type has its own prefab (with its own
    /// visual), so every client spawns the correct projectile from the name of the prefab that is in the attack.
    /// </summary>
    internal class ProjectileHelper
    {
        private static readonly string[] visualNames = { "blunt", "slash", "pierce" };

        public static void SetBlunt(GameObject projectilePrefab)
        {
            SetVisual(projectilePrefab, "blunt", 300f, 0f, 0f);
        }

        public static void SetSlash(GameObject projectilePrefab)
        {
            SetVisual(projectilePrefab, "slash", 500f, 0f, 10f);
        }

        public static void SetPierce(GameObject projectilePrefab)
        {
            SetVisual(projectilePrefab, "pierce", 0f, 0f, 500f);
        }

        // Copies the projectile under a fixed name. Must run at startup on every client, before the ZNetScene is created
        public static GameObject CreateVariant(string name, GameObject original, Action<GameObject> setup)
        {
            GameObject variant = PrefabManager.Instance.CreateClonedPrefab(name, original);

            if (variant == null)
                throw new Exception($"Could not create the projectile variant '{name}'");

            setup(variant);

            return variant;
        }

        private static void SetVisual(GameObject projectilePrefab, string visualName, float rotate, float rotateY, float rotateZ)
        {
            Projectile projectile = projectilePrefab.GetComponent<Projectile>();
            Transform visuals = projectilePrefab.transform.Find("visual");

            if (projectile == null || visuals == null)
                throw new Exception($"Projectile '{projectilePrefab.name}' has no Projectile component or visual child");

            foreach (string name in visualNames)
            {
                Transform visual = visuals.Find(name);

                if (visual == null)
                    throw new Exception($"Projectile '{projectilePrefab.name}' has no visual/{name} child");

                visual.gameObject.SetActive(name == visualName);

                if (name == visualName)
                    projectile.m_visual = visual.gameObject;
            }

            projectile.m_rotateVisual = rotate;
            projectile.m_rotateVisualY = rotateY;
            projectile.m_rotateVisualZ = rotateZ;
        }
    }
}
