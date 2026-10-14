using HarmonyLib;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Makes the LOCAL player slide on terrain like on the Deep North ice sheets while they have the
    // "SlipperyGround" status effect (applied by a normal StatusEffect redeem).
    //
    // Vanilla decides "slipping" inside Character.UpdateWalking from
    // m_lastGroundCollider.material.dynamicFriction <= 0 (and a layer check), so there's no flag
    // to flip. Instead, for the duration of UpdateWalking, m_lastGroundCollider is swapped for a
    // hidden dummy collider carrying a zero-friction material, then restored right after. Nothing
    // else reads the field during that window, and the real colliders are never touched.
    //
    // Purely per-client: only the owner of a character runs UpdateWalking, and other clients see
    // the slide through the synced position and the slipping animation, so no ZDO/RPC is needed.
    internal static class SlipperyGroundHelper
    {
        public const string EffectName = "SlipperyGround";

        private static readonly int s_effectHash = EffectName.GetStableHashCode();

        private static readonly AccessTools.FieldRef<Character, Collider> s_lastGroundColliderRef =
            AccessTools.FieldRefAccess<Character, Collider>("m_lastGroundCollider");

        private static readonly AccessTools.FieldRef<Character, Heightmap> s_lastGroundHeightmapRef =
            AccessTools.FieldRefAccess<Character, Heightmap>("m_lastGroundHeightmap");

        private static readonly AccessTools.FieldRef<Character, CapsuleCollider> s_bodyColliderRef =
            AccessTools.FieldRefAccess<Character, CapsuleCollider>("m_collider");

        private static readonly AccessTools.FieldRef<Character, bool> s_iceShoesRef =
            AccessTools.FieldRefAccess<Character, bool>("m_iceShoes");

        private static PhysicsMaterial s_iceMaterial;
        private static Collider s_dummyCollider;
        private static Collider s_savedCollider;
        private static bool s_swapped;

        private static bool IsActive(Character character)
        {
            return ReferenceEquals(character, Player.m_localPlayer) && Player.m_localPlayer.GetSEMan().HaveStatusEffect(s_effectHash);
        }

        public static void ResetState()
        {
            s_savedCollider = null;
            s_swapped = false;
        }

        // Called from the UpdateWalking prefix. Only swaps while standing on terrain, so boats and
        // building pieces keep their normal grip.
        public static void Swap(Character character)
        {
            if (!IsActive(character))
                return;

            Collider real = s_lastGroundColliderRef(character);

            if (real == null || s_lastGroundHeightmapRef(character) == null)
                return;

            Collider dummy = GetDummyCollider();
            // The game's slippery check also tests the collider's GameObject layer, so mirror the real one.
            dummy.gameObject.layer = real.gameObject.layer;

            s_savedCollider = real;
            s_lastGroundColliderRef(character) = dummy;
            s_swapped = true;
        }

        public static void Restore(Character character)
        {
            if (!s_swapped || !ReferenceEquals(character, Player.m_localPlayer))
                return;

            s_lastGroundColliderRef(character) = s_savedCollider;
            s_savedCollider = null;
            s_swapped = false;
        }

        // The collider swap above only fools the game's slippery LOGIC. The physics engine still
        // combines the body's friction with the real terrain's, which caps the speed at roughly run
        // speed. UpdateBodyFriction multiplies the two (0.4 * ground) while moving, and real ice has
        // ground friction 0, so zero the body's side to get the same result. Idle keeps vanilla's
        // values, which is also what real ice does. Called from the UpdateBodyFriction postfix.
        public static void ApplyBodyFriction(Character character)
        {
            if (!IsActive(character))
                return;

            if (!character.IsOnGround() || s_lastGroundHeightmapRef(character) == null || s_iceShoesRef(character))
                return;

            if (character.GetMoveDir().magnitude < 0.1f)
                return;

            PhysicsMaterial bodyMaterial = s_bodyColliderRef(character).material;
            bodyMaterial.staticFriction = 0f;
            bodyMaterial.dynamicFriction = 0f;
        }

        private static Collider GetDummyCollider()
        {
            if (s_dummyCollider != null)
                return s_dummyCollider;

            if (s_iceMaterial == null)
            {
                s_iceMaterial = new PhysicsMaterial("WBTI_SlipperyGround")
                {
                    dynamicFriction = 0f,
                    staticFriction = 0f,
                    frictionCombine = PhysicsMaterialCombine.Minimum
                };
            }

            // The collider stays disabled: it only exists to carry the material and layer.
            GameObject go = new GameObject("WBTI_SlipperyGroundDummy");
            Object.DontDestroyOnLoad(go);
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.enabled = false;
            box.sharedMaterial = s_iceMaterial;

            s_dummyCollider = box;
            return s_dummyCollider;
        }
    }
}
