using System.Collections;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class ZNetViewHelper
    {
        // Single choke point for spawning a redeem's world object - tags it via
        // TwitchBasePersistentData.MarkRedeemSpawn() so remove commands can always identify it,
        // regardless of what other (possibly config-gated) components end up attached. Use this
        // instead of UnityEngine.Object.Instantiate for anything spawned by a redeem, so the tag
        // can't be forgotten at a new call site.
        public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject instance = UnityEngine.Object.Instantiate(prefab, position, rotation);
            instance.GetComponent<TwitchBasePersistentData>()?.MarkRedeemSpawn();
            return instance;
        }

        public static void Destroy(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            ZNetView netView = gameObject.GetComponent<ZNetView>();

            if (netView != null && netView.IsValid())
            {
                // ZNetScene.Destroy() only tells the network to destroy the ZDO when the calling
                // client owns it - otherwise the GameObject vanishes locally while the ZDO (and the
                // object, for every other client) lives on. Claim ownership first so destroying
                // something we don't own actually propagates instead of silently desyncing.
                if (!netView.IsOwner())
                    netView.ClaimOwnership();

                netView.Destroy();
            }
            else
                UnityEngine.Object.Destroy(gameObject);
        }

        // BUG (fixed here): this used to unconditionally call netView.Destroy() for anything with
        // a ZNetView, silently ignoring "delay" entirely - every ZNetView-tracked caller asking
        // for a delayed destroy actually got destroyed immediately instead. Route the ZNetView
        // case through a coroutine so the delay is honored for networked objects too, instead of
        // only ever working for the plain-GameObject fallback below.
        public static void Destroy(GameObject gameObject, float delay)
        {
            if (gameObject == null)
                return;

            ZNetView netView = gameObject.GetComponent<ZNetView>();

            if (netView != null && netView.IsValid())
            {
                if (delay <= 0f)
                {
                    if (!netView.IsOwner())
                        netView.ClaimOwnership();

                    netView.Destroy();
                    return;
                }

                WizshBoneTwitchIntegration.Instance.StartCoroutine(DestroyAfterDelay(gameObject, delay));
            }
            else
                UnityEngine.Object.Destroy(gameObject, delay);
        }

        // Re-enters Destroy(GameObject) after the wait rather than duplicating the ownership-claim/
        // netView.Destroy() logic here - that overload's own null check safely no-ops if the object
        // was already destroyed by some other path in the meantime.
        private static IEnumerator DestroyAfterDelay(GameObject gameObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }
    }
}