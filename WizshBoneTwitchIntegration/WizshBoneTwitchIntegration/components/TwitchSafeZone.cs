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
        // instead of re-reading the live safezoneBoats/safezoneTraders settings, so toggling those
        // settings mid-session while a player is standing on a ship/at a trader can't desync
        // s_localPlayerZoneCount from reality - see HandlePlayer.
        private GameObject m_playerInZone;
        private Collider m_collider;
        private bool m_isShipZone;
        private bool m_isTraderZone;
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
        // HandleLocalPlayerDeath (guarded above) and the Help tab's "Reset Safezones" button
        // (HelpTab), which calls this directly and unconditionally as a manual "unstick me"
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

            Game.instance?.gameObject.GetComponent<WizshBoneGUI>()?.HideSafeZoneHUD();
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
                // runs synchronously inside AddComponent, and (if the debug toggle is on) its
                // ColliderBoundsHelper.Register reads this same collider's current bounds right then
                // for the wireframe visual. Adding before
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
            return Overlaps(origin, radius, false);
        }

        // The shared zone test behind all the public queries. enabledOnly skips zones whose collider
        // is off or that the profile settings currently switch off (see IsEnabledBySettings).
        // ClosestPoint works in world space for the ward capsule, trader capsule and the ship's
        // rotated box alike.
        private static bool Overlaps(Vector3 origin, float radius, bool enabledOnly)
        {
            foreach (TwitchSafeZone safeZone in s_activeSafeZones)
            {
                if (safeZone.m_collider == null)
                    continue;

                if (enabledOnly && (!safeZone.m_collider.enabled || !safeZone.IsEnabledBySettings()))
                    continue;

                if (Vector3.Distance(origin, safeZone.m_collider.ClosestPoint(origin)) <= radius)
                    return true;
            }

            return false;
        }

        // True for a Twitch-spawned (not manually claimed), untamed creature that the ward applies
        // to, i.e. one whose redeem didn't set ignoreWard. Shared by the burn/push handling and the
        // projectile blocking (SafeZoneProjectileHelper). All three components are rehydrated from
        // the ZDO on every client, so this gives the same answer for owners and non-owners.
        public static bool IsWardTargetCreature(GameObject creature)
        {
            return IsWardTargetCreature(creature, out _);
        }

        // Same test, also handing back the Humanoid it had to look up anyway (null when the creature
        // has none, or when it isn't a ward target), so callers don't fetch it a second time.
        public static bool IsWardTargetCreature(GameObject creature, out Humanoid humanoid)
        {
            humanoid = null;

            TwitchCreaturePersistentData persistentData = creature.GetComponent<TwitchCreaturePersistentData>();

            if (persistentData == null || persistentData.m_ignoreWard)
                return false;

            TwitchCreatureClaim creatureClaim = creature.GetComponent<TwitchCreatureClaim>();

            if (creatureClaim == null || !creatureClaim.m_isSpawn)
                return false;

            humanoid = creature.GetComponent<Humanoid>();

            if (humanoid != null && humanoid.m_tamed)
            {
                humanoid = null;
                return false;
            }

            return true;
        }

        // True if the point is inside any enabled safe zone. Deliberately only looks at where the
        // projectile / victim is, never where the attack came from: a monster that clips into a zone
        // (pushed in, or spawned there) must not get a free pass for being "already inside".
        public static bool IsInsideEnabledSafeZone(Vector3 point)
        {
            // ClosestPoint returns the point itself when it is inside; the tolerance absorbs float noise.
            return Overlaps(point, 0.01f, true);
        }

        // Like IsInsideEnabledSafeZone, but also true within `margin` of an enabled zone's collider
        // (used to keep idle wander destinations a body's width clear of the edge).
        public static bool IsNearEnabledSafeZone(Vector3 point, float margin)
        {
            return Overlaps(point, margin, true);
        }

        public static bool HasActiveSafeZones => s_activeSafeZones.Count > 0;

        // False when the profile lets spawned monsters into zones (push is off). Their projectiles and
        // hits are then left alone too: only monsters kept out are also stopped from attacking into a
        // zone (SafeZoneProjectileHelper, SafeZoneDamageHelper). Independent of burning.
        public static bool KeepsMonstersOut => ProfileSettingsHelper.Current.wardPushCreatures;

        // False while the profile settings switch this kind of zone off: a ship's zone with
        // safezoneBoats off, a trader's with safezoneTraders off (wards are always on). The zone and
        // its collider stay in place either way - only new entries and creature handling are gated
        // on this - so it's also what the debug bounds outline follows.
        private bool IsEnabledBySettings()
        {
            if (m_isShipZone && !ProfileSettingsHelper.Current.safezoneBoats)
                return false;

            if (m_isTraderZone && !ProfileSettingsHelper.Current.safezoneTraders)
                return false;

            return true;
        }

        public void Awake()
        {
            try
            {
                // The hammer's placement ghost is a plain Instantiate of the prefab with
                // ZNetView.m_forceDisableInit set, so this Awake runs on it too. A ghost must never
                // act as a zone (spawn blocking, burn/push, bounds outline): switch it off and don't
                // register it. Not ZNetView.IsValid(): a real piece's ZNetView may Awake after this
                // child, the flag has no such ordering issue.
                if (ZNetView.m_forceDisableInit)
                {
                    Collider ghostCollider = GetComponent<Collider>();

                    if (ghostCollider != null)
                        ghostCollider.enabled = false;

                    enabled = false;
                    return;
                }

                m_customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();
                m_collider = GetComponent<Collider>();

                // Cached once: IsEnabledBySettings runs for every zone on every zone query (every
                // hit, projectile step and monster AI point check), and the parent never changes.
                Transform parent = transform.parent;
                m_isShipZone = parent != null && parent.GetComponent<Ship>() != null;
                m_isTraderZone = parent != null && parent.GetComponent<Trader>() != null;

                s_activeSafeZones.Add(this);

                if (m_isTraderZone && m_collider is CapsuleCollider capsule)
                {
                    Vector3 scale = transform.lossyScale;
                    Jotunn.Logger.LogWarning($"[WBTI] Trader safezone '{parent.name}': lossyScale={scale}, capsule radius={capsule.radius} height={capsule.height} (direction {capsule.direction}) -> world radius~{capsule.radius * Mathf.Max(scale.x, scale.z):F1} height~{capsule.height * scale.y:F1}");
                }

                ColliderBoundsHelper.Register(gameObject, ColliderBoundsHelper.SafeZoneColor, IsEnabledBySettings);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchSafeZone.Awake failed: " + e);
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
            // toggling safezoneBoats between a player's entry and exit leaves
            // s_localPlayerZoneCount/m_playerIsInSafeZone stuck (or wrongly decremented). A ship
            // with safezoneBoats off this whole time never touches the count in either
            // direction, since entry is skipped and m_playerInZone then stays null for the exit too.
            // Same gating applies to traders (safezoneTraders).
            if (value && !IsEnabledBySettings())
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
                Game.instance.gameObject.GetComponent<WizshBoneGUI>()?.ShowSafeZoneHUD();
            }
        }

        private void ExitLocalPlayerZone()
        {
            if (s_localPlayerZoneCount > 0)
                s_localPlayerZoneCount--;

            if (s_localPlayerZoneCount == 0)
            {
                m_customRewards.m_playerIsInSafeZone = false;
                Game.instance.gameObject.GetComponent<WizshBoneGUI>()?.HideSafeZoneHUD();
            }
        }

        public void HandleCreatures(Collider collider)
        {
            // No player-name check needed here: IsWardTargetCreature requires the Twitch creature
            // components, which a player never has (and it avoids a Unity .name string allocation
            // per overlapping collider per physics step).
            if (!ProfileSettingsHelper.Current.wardBurnCreatures && !ProfileSettingsHelper.Current.wardPushCreatures)
                return;

            if (!IsEnabledBySettings())
                return;

            if (!IsWardTargetCreature(collider.gameObject, out Humanoid humanoid) || humanoid == null)
                return;

            if (ProfileSettingsHelper.Current.wardBurnCreatures)
                SafeZoneBurnHelper.Apply(humanoid);

            if (ProfileSettingsHelper.Current.wardPushCreatures)
                SafeZonePushHelper.PushOut(humanoid, m_collider);
        }

        public void OnDestroy()
        {
            try
            {
                s_activeSafeZones.Remove(this);
                ColliderBoundsHelper.Unregister(gameObject);

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
