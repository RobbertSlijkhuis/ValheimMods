using System;
using UnityEngine;

namespace ModularMagic_Food.Helpers
{
    internal class PickableHelper
    {
        private const string FootParticlesPath = "foot/particles";

        /**
         * The foot particles are not under Pickable.m_hideWhenPicked, so they have to follow it by hand.
         * m_hideWhenPicked is the one Pickable already keeps in sync on every client (Awake reads the ZDO,
         * RPC_SetPicked reaches everybody), so mirroring its state is multiplayer safe without touching the ZDO.
         * instant is for a fresh load: nothing should fade out there, the pickable was already picked.
         */
        public static void SyncFootParticles(Pickable pickable, bool instant)
        {
            try
            {
                if (pickable == null || pickable.m_hideWhenPicked == null || !pickable.name.StartsWith("MMF_"))
                    return;

                Transform footParticles = pickable.transform.Find(FootParticlesPath);

                if (footParticles == null)
                    return;

                ParticleSystem particles = footParticles.GetComponent<ParticleSystem>();

                if (particles == null)
                    return;

                if (pickable.m_hideWhenPicked.activeSelf)
                {
                    footParticles.gameObject.SetActive(true);
                    particles.Play();
                }
                else if (instant)
                {
                    particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    footParticles.gameObject.SetActive(false);
                }
                else
                {
                    particles.Stop();
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not sync foot particles: " + e);
            }
        }
    }
}
