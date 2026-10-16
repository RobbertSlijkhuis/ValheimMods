using ModularMagic_Armors.Configs;
using ModularMagic_Armors.Types;
using System;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    internal class FootstepHelper
    {
        private const float FootstepLifetime = 10f;
        private const float GroundCheckUp = 0.5f;
        private const float GroundCheckDistance = 1.5f;

        private static int groundMask = 0;

        /**
         * Adds the DarkWizard flames on top of the vanilla footstep effects when the stepping character is a player wearing the set.
         * Runs on every client (the footstep RPC goes to everybody) so the set is read from the synced ZDO status of that player.
         */
        public static void AddDarkWizardFootstep(FootStep footStep, Vector3 point, Vector3 emitterVelocity)
        {
            try
            {
                GameObject prefab = ModularMagic_Armors.Instance.prefabs.DarkWizardFootstepPrefab;

                if (prefab == null || !PluginConfig.darkWizardFootsteps.Value || !WearsDarkWizardSet(footStep))
                    return;

                if (!TryGetGroundPoint(point, out Vector3 groundPoint))
                    return;

                GameObject flames = UnityEngine.Object.Instantiate(prefab, groundPoint, footStep.transform.rotation);

                foreach (ParticleSystem particleSystem in flames.GetComponentsInChildren<ParticleSystem>())
                {
                    ParticleSystem.MainModule main = particleSystem.main;
                    main.emitterVelocityMode = ParticleSystemEmitterVelocityMode.Custom;
                    main.emitterVelocity = emitterVelocity;
                }

                UnityEngine.Object.Destroy(flames, FootstepLifetime);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add DarkWizard footstep: " + e);
            }
        }

        /**
         * Position based so it also works for remote players, whose ground/water state is not simulated locally.
         * Fails when there is no ground right below the foot (airborne) or when the ground is under water.
         */
        private static bool TryGetGroundPoint(Vector3 point, out Vector3 groundPoint)
        {
            groundPoint = point;

            if (groundMask == 0)
                groundMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "terrain", "blocker", "vehicle");

            if (!Physics.Raycast(point + Vector3.up * GroundCheckUp, Vector3.down, out RaycastHit hit, GroundCheckUp + GroundCheckDistance, groundMask))
                return false;

            if (Floating.GetLiquidLevel(hit.point) > hit.point.y)
                return false;

            groundPoint = hit.point;
            return true;
        }

        private static bool WearsDarkWizardSet(FootStep footStep)
        {
            if (footStep.GetComponent<Player>() == null)
                return false;

            return ArmorSetHelper.IsSetWorn(ArmorSetId.DarkWizard, footStep.GetComponent<ZNetView>());
        }
    }
}
