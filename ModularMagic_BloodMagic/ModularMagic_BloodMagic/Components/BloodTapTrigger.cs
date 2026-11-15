using ModularMagic_BloodMagic.Helpers;
using System;
using UnityEngine;

namespace ModularMagic_BloodMagic.Components
{
    /// <summary>
    /// The object the Blood Tap attack spawns when it fires, as its projectile. The game only fires it after it took the health and eitr, so
    /// it fills the scythe (BloodTapHelper.Tap) and is gone. It has no ZNetView, so only the caster spawns it.
    /// </summary>
    internal class BloodTapTrigger : MonoBehaviour, IProjectile
    {
        public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item, ItemDrop.ItemData ammo)
        {
            try
            {
                BloodTapHelper.Tap(owner);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[Blood Tap] The charge failed: {e}");
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
