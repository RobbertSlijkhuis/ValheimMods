using ModularMagic_Core.Components;
using System;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// Makes the prefab an attack spawns an MMAoe: a local area hit that is done by the shooter. The Aoe, the colliders, the timed
    /// destruction and the network components are removed, so the prefab is not a networked prefab and must not be registered with the game.
    /// Call it once at startup, after the asset bundle is loaded.
    /// </summary>
    public static class MMAoeHelper
    {
        /// <summary>
        /// The area is a box. Its size and offset are those of the BoxCollider of the prefab, so the shape can still be changed in Unity.
        /// </summary>
        public static MMAoe PrepareBox(GameObject prefab, bool hitProps)
        {
            BoxCollider box = prefab.GetComponent<BoxCollider>();

            if (box == null)
                throw new Exception($"{prefab.name} has no BoxCollider");

            MMAoe aoe = prefab.AddComponent<MMAoe>();
            aoe.sphere = false;
            aoe.center = box.center;
            aoe.size = box.size;
            aoe.hitProps = hitProps;

            RemoveOldComponents(prefab);

            return aoe;
        }

        /// <summary>
        /// The area is a sphere. The radius is the radius of the Aoe of the prefab, it can be set again on the returned component.
        /// </summary>
        public static MMAoe PrepareSphere(GameObject prefab, bool hitProps)
        {
            Aoe oldAoe = prefab.GetComponent<Aoe>();

            if (oldAoe == null)
                throw new Exception($"{prefab.name} has no Aoe");

            MMAoe aoe = prefab.AddComponent<MMAoe>();
            aoe.sphere = true;
            aoe.radius = oldAoe.m_radius;
            aoe.hitProps = hitProps;

            RemoveOldComponents(prefab);

            return aoe;
        }

        private static void RemoveOldComponents(GameObject prefab)
        {
            // The sync needs the view, so it goes first
            Remove<ZSyncTransform>(prefab);
            Remove<ZNetView>(prefab);
            Remove<Aoe>(prefab);
            Remove<TimedDestruction>(prefab);

            foreach (Collider collider in prefab.GetComponents<Collider>())
                UnityEngine.Object.DestroyImmediate(collider, true);
        }

        private static void Remove<T>(GameObject prefab) where T : Component
        {
            T component = prefab.GetComponent<T>();

            if (component != null)
                UnityEngine.Object.DestroyImmediate(component, true);
        }
    }
}
