using System;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchSafeZone : MonoBehaviour
    {
        private TwitchCustomRewards m_customRewards;

        // Set to Player.m_localPlayer's own GameObject while this zone counted them as "inside"
        // (i.e. EnterLocalPlayerZone ran and hasn't been matched by an exit yet), null otherwise.
        // Only ever touched for the local player (see the ID check early in HandlePlayer) - a
        // remote player entering/exiting the same trigger never writes this. Exit checks this
        // instead of re-reading the live allowRedeemsOnBoats setting, so toggling that setting
        // mid-session while a player is standing on a ship can't desync s_localPlayerZoneCount from
        // reality - see HandlePlayer.
        private GameObject m_playerInZone;
        private Collider m_collider;
        private readonly string playerIdentifier = "Player(Clone)";

        // Tracks how many safe zones the local player is currently standing in,
        // so overlapping zones don't toggle the HUD/flag off while still in another zone.
        private static int s_localPlayerZoneCount = 0;

        // Every live TwitchSafeZone self-registers here, so spawn-point containment checks
        // (see IsPointInSafeZone) can test against the mod's own known safe zones directly
        // instead of running a Physics.OverlapSphere against the whole "character_trigger" layer.
        private static readonly List<TwitchSafeZone> s_activeSafeZones = new List<TwitchSafeZone>();

        // Called from GameAwake_Postfix so a fresh world load doesn't inherit stale state.
        public static void ResetLocalPlayerZoneCount()
        {
            s_localPlayerZoneCount = 0;
        }

        // Called from GameAwake_Postfix alongside ResetLocalPlayerZoneCount, as a safety net in
        // case OnDestroy doesn't get to run for every zone before a fresh world load (e.g. a crash).
        public static void ResetActiveSafeZones()
        {
            s_activeSafeZones.Clear();
        }

        // Called from OnDeath_Postfix (MiscPatchesWBTI) when the local player dies.
        // Game._RequestRespawn() destroys the player GameObject ~10s later, which may still be
        // standing inside a safe zone's trigger collider at that point; Unity does not fire
        // OnTriggerExit when one side of an overlapping pair is destroyed, so without this the
        // zone(s) the player was in never see the exit and s_localPlayerZoneCount/
        // m_playerIsInSafeZone stay stuck. Handled right away, at death, rather than waiting for
        // the actual destroy, since the player can't move out of the zone on their own in between.
        public static void HandleLocalPlayerDeath()
        {
            if (s_localPlayerZoneCount == 0)
                return;

            ForceExitAllZones();
        }

        // Unconditionally clears all client-local safe-zone bookkeeping (zone membership on
        // every live TwitchSafeZone, the local entry counter, the "in safe zone" flag, and the
        // HUD panel). Never touches Wards/ships/trader zones themselves - purely resets this
        // client's own tracking of whether the local player is standing in one. Shared by
        // HandleLocalPlayerDeath (guarded above) and the Debug tab's "Reset Safezones" button
        // (DebugTab), which calls this directly and unconditionally as a manual "unstick me"
        // action - safe to call even when nothing is actually stuck.
        public static void ForceExitAllZones()
        {
            foreach (TwitchSafeZone safeZone in s_activeSafeZones)
            {
                safeZone.m_playerInZone = null;
            }

            s_localPlayerZoneCount = 0;

            TwitchCustomRewards customRewards = Game.instance?.gameObject.GetComponent<TwitchCustomRewards>();
            if (customRewards != null)
                customRewards.m_playerIsInSafeZone = false;

            Game.instance?.gameObject.GetComponent<SafeZoneHUDPanel>()?.Hide();
        }

        public static bool IsPointInSafeZone(Vector3 point)
        {
            return OverlapsSafeZone(point, 0f);
        }

        // Adds a TwitchSafeZone to a ship's OnboardTrigger, sized to fit the ship's own
        // non-trigger colliders (i.e. its hull/deck) rather than a per-prefab hardcoded box, so
        // any current or future boat prefab gets a correctly sized zone with no magic numbers.
        // Only called for ships actually placed by a player - see WearNTear_OnPlaced_Postfix in
        // ShipPatchesWBTI and the ShipSafeZone flag in TwitchBasePersistentData.
        public static void AttachToShip(GameObject shipRoot)
        {
            try
            {
                Transform onboardTriggerTrans = shipRoot.transform.Find("OnboardTrigger");

                if (onboardTriggerTrans == null)
                {
                    Jotunn.Logger.LogWarning($"[WBTI] TwitchSafeZone.AttachToShip: '{shipRoot.name}' has no OnboardTrigger, skipping");
                    return;
                }

                BoxCollider boxCollider = onboardTriggerTrans.gameObject.GetComponent<BoxCollider>();
                boxCollider.includeLayers = LayerMask.GetMask("piece");

                Bounds? hullBounds = null;

                foreach (Collider collider in shipRoot.GetComponentsInChildren<Collider>(true))
                {
                    if (collider.isTrigger)
                        continue;

                    // Each corner is taken in the collider's own local space, then carried through
                    // that collider's transform into world space and back into the OnboardTrigger's
                    // local space - never through a world-space AABB (Collider.bounds), which is
                    // axis-aligned to the *world* and therefore a different, wrong shape/size
                    // depending on whatever heading the ship happens to be facing when this runs.
                    foreach (Vector3 localCorner in GetLocalCorners(collider))
                    {
                        Vector3 pointInOnboardSpace = onboardTriggerTrans.InverseTransformPoint(collider.transform.TransformPoint(localCorner));

                        if (hullBounds == null)
                        {
                            hullBounds = new Bounds(pointInOnboardSpace, Vector3.zero);
                        }
                        else
                        {
                            Bounds encapsulated = hullBounds.Value;
                            encapsulated.Encapsulate(pointInOnboardSpace);
                            hullBounds = encapsulated;
                        }
                    }
                }

                if (hullBounds != null)
                {
                    boxCollider.center = hullBounds.Value.center;
                    boxCollider.size = hullBounds.Value.size + new Vector3(0, 2f, 0);
                }

                // Added last, after the collider has its final size/center - TwitchSafeZone.Awake()
                // runs synchronously inside AddComponent, and (if the debug toggle is on) reads this
                // same collider's current bounds right then for the wireframe visual. Adding before
                // the resize above would capture the OnboardTrigger's original vanilla dimensions
                // instead of the computed hull bounds.
                onboardTriggerTrans.gameObject.AddComponent<TwitchSafeZone>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.AttachToShip failed: " + e);
            }
        }

        // The 8 corners of a collider's own local-space bounding box, in the collider's own local
        // space (not world space) - callers transform these through the collider's own transform to
        // get world positions, keeping the result invariant to whatever the collider's current world
        // rotation happens to be. MeshCollider falls back to its shared mesh's local bounds; any
        // other/exotic collider type falls back to its (world-space) Collider.bounds transformed back
        // into that collider's own local space, which is only an approximation for a rotated shape.
        private static Vector3[] GetLocalCorners(Collider collider)
        {
            Vector3 center;
            Vector3 half;

            switch (collider)
            {
                case BoxCollider box:
                    center = box.center;
                    half = box.size / 2f;
                    break;
                case SphereCollider sphere:
                    center = sphere.center;
                    half = Vector3.one * sphere.radius;
                    break;
                case CapsuleCollider capsule:
                    float halfHeight = Mathf.Max(capsule.height, capsule.radius * 2f) / 2f;
                    half = capsule.direction switch
                    {
                        0 => new Vector3(halfHeight, capsule.radius, capsule.radius),
                        2 => new Vector3(capsule.radius, capsule.radius, halfHeight),
                        _ => new Vector3(capsule.radius, halfHeight, capsule.radius),
                    };
                    center = capsule.center;
                    break;
                case MeshCollider mesh when mesh.sharedMesh != null:
                    center = mesh.sharedMesh.bounds.center;
                    half = mesh.sharedMesh.bounds.extents;
                    break;
                default:
                    center = collider.transform.InverseTransformPoint(collider.bounds.center);
                    half = collider.bounds.extents;
                    break;
            }

            Vector3[] corners = new Vector3[8];

            for (int i = 0; i < 8; i++)
            {
                corners[i] = center + new Vector3(
                    (i & 1) == 0 ? -half.x : half.x,
                    (i & 2) == 0 ? -half.y : half.y,
                    (i & 4) == 0 ? -half.z : half.z);
            }

            return corners;
        }

        // General case of IsPointInSafeZone: true if a sphere of the given radius centered at
        // origin overlaps any safe zone's collider (radius 0 = point containment).
        public static bool OverlapsSafeZone(Vector3 origin, float radius)
        {
            foreach (TwitchSafeZone safeZone in s_activeSafeZones)
            {
                if (safeZone.m_collider != null && Vector3.Distance(origin, safeZone.m_collider.ClosestPoint(origin)) <= radius)
                {
                    return true;
                }
            }

            return false;
        }

        public void Awake()
        {
            try
            {
                m_customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
                m_collider = GetComponent<Collider>();
                s_activeSafeZones.Add(this);

                if (PluginConfig.configShowSafeZoneDebug.Value)
                    gameObject.AddComponent<TwitchSafeZoneDebugVisual>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.Awake failed: " + e);
            }
        }

        // Called from PluginConfig.configShowSafeZoneDebug.SettingChanged so toggling the debug
        // option live immediately shows/hides bounds on every currently active zone, not just
        // ones created after the toggle. Newly created zones pick up the current value themselves
        // in Awake() above.
        public static void RefreshDebugVisuals(bool show)
        {
            foreach (TwitchSafeZone safeZone in s_activeSafeZones)
            {
                TwitchSafeZoneDebugVisual visual = safeZone.GetComponent<TwitchSafeZoneDebugVisual>();

                if (show && visual == null)
                    safeZone.gameObject.AddComponent<TwitchSafeZoneDebugVisual>();
                else if (!show && visual != null)
                    Destroy(visual);
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            try
            {
                HandlePlayer(collider, true, "Player entered");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.OnTriggerEnter failed: " + e);
            }
        }

        public void OnTriggerStay(Collider collider)
        {
            try
            {
                HandleCreatures(collider);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.OnTriggerStay failed: " + e);
            }
        }

        public void OnTriggerExit(Collider collider)
        {
            try
            {
                HandlePlayer(collider, false, "Player left");
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.OnTriggerExit failed: " + e);
            }
        }

        private void HandlePlayer(Collider collider, bool value, string message = null)
        {
            // Only gates new entries. An exit always mirrors m_playerInZone (what this zone
            // actually counted at entry) rather than re-checking the live setting here, otherwise
            // toggling allowRedeemsOnBoats between a player's entry and exit leaves
            // s_localPlayerZoneCount/m_playerIsInSafeZone stuck (or wrongly decremented). A ship
            // with allowRedeemsOnBoats on this whole time never touches the count in either
            // direction, since entry is skipped and m_playerInZone then stays null for the exit too.
            if (value && transform.parent.gameObject.GetComponent<Ship>() != null && ProfileSettingsHelper.Current.allowRedeemsOnBoats)
                return;

            if (collider.gameObject.name != playerIdentifier)
                return;

            Player player = collider.gameObject.GetComponent<Player>();

            // Everything below only concerns the local player - a remote player passing through
            // the same trigger must never affect this client's own s_localPlayerZoneCount/
            // m_playerIsInSafeZone, nor overwrite m_playerInZone out from under the local player.
            if (player.GetPlayerID() != Player.m_localPlayer.GetPlayerID())
                return;

            if (message != null && message != "")
                Jotunn.Logger.LogWarning(message);

            if (value)
            {
                m_playerInZone = collider.gameObject;
                EnterLocalPlayerZone();
            }
            else if (m_playerInZone != null)
            {
                m_playerInZone = null;
                ExitLocalPlayerZone();
            }
        }

        private void EnterLocalPlayerZone()
        {
            s_localPlayerZoneCount++;

            if (s_localPlayerZoneCount == 1)
            {
                m_customRewards.m_playerIsInSafeZone = true;
                Game.instance.gameObject.GetComponent<SafeZoneHUDPanel>()?.Show();
            }
        }

        private void ExitLocalPlayerZone()
        {
            if (s_localPlayerZoneCount > 0)
                s_localPlayerZoneCount--;

            if (s_localPlayerZoneCount == 0)
            {
                m_customRewards.m_playerIsInSafeZone = false;
                Game.instance.gameObject.GetComponent<SafeZoneHUDPanel>()?.Hide();
            }
        }

        public void HandleCreatures(Collider collider)
        {
            if (collider.gameObject.name != playerIdentifier)
            {
                if (!ProfileSettingsHelper.Current.wardBurnCreatures && !ProfileSettingsHelper.Current.wardPushCreatures)
                    return;

                if (transform.parent.gameObject.GetComponent<Ship>() != null && ProfileSettingsHelper.Current.allowRedeemsOnBoats)
                    return;

                TwitchCreaturePersistentData persistentData = collider.gameObject.GetComponent<TwitchCreaturePersistentData>();

                if (persistentData == null || persistentData.m_ignoreWard)
                    return;

                TwitchCreatureClaim creatureClaim = collider.gameObject.GetComponent<TwitchCreatureClaim>();

                if (creatureClaim == null)
                    return;

                Humanoid humanoid = collider.gameObject.GetComponent<Humanoid>();

                if (humanoid != null && humanoid.m_tamed)
                    return;

                if (!creatureClaim.m_isSpawn)
                    return;

                if (humanoid != null && ProfileSettingsHelper.Current.wardBurnCreatures && !humanoid.GetSEMan().HaveStatusEffect(WizshBoneTwitchIntegration.Instance.effects.Burning.m_nameHash))
                {
                    float duration = 10f;
                    SE_Stats burning = Instantiate(WizshBoneTwitchIntegration.Instance.effects.Burning);
                    burning.m_healthPerTick = Mathf.Round(humanoid.GetMaxHealth() / (duration - 1f) * -1f);
                    burning.m_ttl = duration;
                    humanoid.GetSEMan().AddStatusEffect(burning);
                }

                if (humanoid != null)
                {
                    float force = ProfileSettingsHelper.Current.wardPushForce;
                    Vector3 pushDir = (collider.transform.position - transform.position).normalized;
                    pushDir.y = 0;
                    Rigidbody rb = collider.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        // rb.AddForce((transform.forward * -force) + (transform.up * force), ForceMode.Acceleration);
                        rb.AddForce(pushDir * force, ForceMode.Acceleration);
                    }
                }
            }
        }

        public void OnDestroy()
        {
            try
            {
                s_activeSafeZones.Remove(this);

                if (m_playerInZone == null)
                    return;

                // Player.m_localPlayer is null during the death/respawn window
                if (Player.m_localPlayer == null)
                    return;

                // m_playerInZone is only ever set for the local player (see HandlePlayer), so no
                // further identity check is needed here.
                m_playerInZone = null;
                ExitLocalPlayerZone();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.OnDestroy failed: " + e);
            }
        }
    }
}
