using ModularMagic_BloodMagic.Helpers;
using System;
using UnityEngine;

namespace ModularMagic_BloodMagic.Components
{
    /// <summary>
    /// The object the Nova attack spawns when it fires, as its projectile. It has no ZNetView, so only the caster spawns it. It releases the
    /// apparitions of the caster (NovaHelper.Release) and is gone.
    /// </summary>
    internal class NovaTrigger : MonoBehaviour, IProjectile
    {
        public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
        {
            try
            {
                NovaHelper.Release(owner, hitData);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[Nova] The release failed: {e}");
            }
            finally
            {
                Destroy(gameObject);
            }
        }

        public string GetTooltipString(int itemQuality)
        {
            return "";
        }
    }
}
