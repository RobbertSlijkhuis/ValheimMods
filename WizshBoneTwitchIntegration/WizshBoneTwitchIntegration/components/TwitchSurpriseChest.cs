using System.Collections;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchSurpriseChest : MonoBehaviour
    {
        ZNetView m_netView;
        private readonly int chestDataHash = "SurpriseChestData_WBTI".GetStableHashCode();
        private readonly int openedHash = "WBTI_SurpriseChest_Opened".GetStableHashCode();

        // Dedupes this client's own reaction to the ZDO "opened" flag (see Update()) so the open
        // animation/effect fires exactly once locally, no matter which client actually set the flag.
        private bool m_openHandledLocally;

        // How long the chest lingers after its last piece of loot has come out, before removing itself.
        private const float DespawnDelayAfterLoot = 10f;

        private string m_redeemerName;
        private string m_redeemTitle;
        private int m_amount;
        private float m_force;
        private List<SurpriseChestSpawnData> m_items;
        private float m_openDelay = 1.5f;
        // TODO: implement mimic behaviour
        private bool m_mimic;
        private bool m_random;
        private int m_yeetChance = 5;
        private float m_spawnDelay = 0.7f;
        private Transform m_spawnPoint;
        private Quaternion m_spawnPointOriginalRotation;

        public bool m_interact;
        public TwitchSurpriseChestInteract m_surpriseChestInteract;
        public string m_type;

        public bool IsOpened => m_netView != null && m_netView.IsValid() && m_netView.GetZDO().GetBool(openedHash, false);

        EffectList chestOpeningEffects = new EffectList();
        EffectList despawnEffect = new EffectList();
        EffectList spawnEffects = new EffectList();
        EffectList spawnItemEffects = new EffectList();

        private Minimap.PinData m_mapPin;

        public void Awake()
        {
            try
            {
                chestOpeningEffects = WizshBoneTwitchIntegration.Instance.effectLists.ChestOpenEffect;
                despawnEffect = WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium;
                spawnEffects = WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium;
                spawnItemEffects = WizshBoneTwitchIntegration.Instance.effectLists.SpawnItemEffect;

                m_netView = gameObject.GetComponent<ZNetView>();
                m_surpriseChestInteract = transform.Find("chest_top").gameObject.GetComponent<TwitchSurpriseChestInteract>();
                m_spawnPoint = transform.Find("spawnpoint");
                m_spawnPointOriginalRotation = m_spawnPoint.rotation;

                if (m_netView == null || !m_netView.IsValid())
                    return;

                string chestDataString = m_netView.GetZDO().GetString(chestDataHash, "");

                if (chestDataString == "")
                    return;

                string[] chestRedeemData = chestDataString.Split('|');
                m_redeemerName = chestRedeemData[0];
                m_redeemTitle = chestRedeemData[1];

                RedeemData redeem = RedeemHelper.GetRedeemByTitle(m_redeemTitle);

                if (redeem == null || redeem.chestData == null)
                {
                    Jotunn.Logger.LogError("Could not find redeem by title or chestdata is null!");
                    return;
                }

                ApplyChestData(redeem.chestData);
                m_mapPin = Minimap.instance.AddPin(transform.position, Minimap.PinType.Icon3, "Surprise Chest", false, false);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("TwitchSurpriseChest.Awake failed: " + e);
            }
        }

        public void Init(SurpriseChestData chestData, CustomRewardEvent customRewardEvent)
        {
            m_redeemerName = customRewardEvent.RedeemerName;
            m_redeemTitle = customRewardEvent.CustomRewardTitle;

            ApplyChestData(chestData);

            m_netView.GetZDO().Set(chestDataHash, $"{m_redeemerName}|{m_redeemTitle}");
            RedeemerTagHelper.Apply(gameObject, customRewardEvent);
            m_mapPin = Minimap.instance.AddPin(transform.position, Minimap.PinType.Icon3, "Surprise Chest", false, false);

            TriggerSpawnEffects();

            if (!m_interact)
                StartCoroutine(OpenDelay());
        }

        private void ApplyChestData(SurpriseChestData chestData)
        {
            m_amount = chestData.amount;
            m_force = chestData.force;
            m_items = chestData.items;
            m_interact = chestData.interact;
            m_mimic = chestData.mimicChance == 0 ? false : Random.Range(0, 100) <= chestData.mimicChance;
            m_random = chestData.random;
            m_spawnDelay = chestData.spawnDelay;
            m_type = chestData.type;
            m_yeetChance = chestData.yeetChance;
        }

        // Claims the one-time right to actually open this chest: whichever client (owner or not -
        // any player can walk up and interact) gets its ZDO write in first wins, closing most of
        // the window where two players interacting near-simultaneously would otherwise each spawn
        // their own copy of the loot. The open animation/effect itself is NOT played from here -
        // every client (including this one) picks that up uniformly from Update() reading the same
        // ZDO flag back, so a late-joining/late-rendering client sees the same thing everyone else
        // does instead of a chest that silently already spawned its items.
        public bool TryTriggerOpen()
        {
            if (m_netView == null || !m_netView.IsValid())
                return false;

            if (m_netView.GetZDO().GetBool(openedHash, false))
                return false;

            if (!m_netView.IsOwner())
                m_netView.ClaimOwnership();

            m_netView.GetZDO().Set(openedHash, true);
            StartCoroutine(SpawnItemsDelay());
            return true;
        }

        private void PlayOpenEffects()
        {
            m_surpriseChestInteract.m_animator.SetTrigger("Open");
            TriggerChestOpeningEffect();
        }

        // The despawn effect is NOT played from here: OnDestroy also runs when the chest merely
        // unloads (owner walks away) or is deleted by WBTIRemoveChests, and by this point the ZDO is
        // already reset so ownership can't be checked. DespawnAfterDelay plays it on a real despawn.
        public void OnDestroy()
        {
            try
            {
                if (m_mapPin != null && Minimap.instance != null)
                    Minimap.instance.RemovePin(m_mapPin);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("TwitchSurpriseChest.OnDestroy failed: " + e);
            }
        }

        public void Update()
        {
            try
            {
                if (m_mapPin != null)
                    m_mapPin.m_pos = transform.position;

                // Every replicated instance of this chest (owner, the interacting client, and
                // anyone else who has it loaded) reacts identically to the shared "opened" ZDO
                // flag instead of only the client that happened to call TryTriggerOpen() - see the
                // no-RPC ZDO pattern in CLAUDE.md.
                if (!m_openHandledLocally && m_netView != null && m_netView.IsValid() && m_netView.GetZDO().GetBool(openedHash, false))
                {
                    m_openHandledLocally = true;
                    PlayOpenEffects();
                }
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("TwitchSurpriseChest.Update failed: " + e);
            }
        }

        public IEnumerator OpenDelay()
        {
            yield return new WaitForSeconds(m_openDelay);
            TryTriggerOpen();
        }

        private IEnumerator SpawnItemsDelay()
        {
            yield return new WaitForSeconds(4f);
            SpawnItems();
        }

        private void SpawnItems()
        {
            try
            {
                if (m_items == null)
                {
                    Jotunn.Logger.LogError("No items to spawn!");
                    return;
                }

                List<SurpriseChestSpawnData> eligibleItems = SurpriseChestHelper.GetEligibleItems(m_items);

                if (eligibleItems.Count == 0)
                {
                    Jotunn.Logger.LogWarning("No eligible surprise chest items to spawn (all filtered out for this dungeon)");
                    return;
                }

                // Everything that will come out is decided first, with the Max spawned check removing
                // entries from the eligible list as the picks add up; only then is any of it spawned.
                List<SurpriseChestSpawnData> picks = PickLoot(eligibleItems);

                if (picks.Count == 0)
                {
                    Jotunn.Logger.LogWarning("No surprise chest items left to spawn (every entry is at its max spawned limit)");
                    StartCoroutine(DespawnAfterDelay(DespawnDelayAfterLoot));
                    return;
                }

                float timeOffset = 0f;
                float lastSpawnAt = 0f;

                for (int index = 0; index < picks.Count; index++)
                {
                    float angleChange = ChangeAngleByIndex(index);
                    float force = m_yeetChance > 0 && Random.Range(0, 100) <= m_yeetChance ? 1000f : m_force;
                    SurpriseChestSpawnData spawnData = picks[index];

                    StartCoroutine(SpawnItem(spawnData, force, angleChange, timeOffset));
                    lastSpawnAt = timeOffset;
                    timeOffset += m_spawnDelay;
                }

                // Counted from when the last item comes out, so a long Amount x Spawn delay can't
                // remove the chest (and the loot coroutines living on it) before everything spawned.
                StartCoroutine(DespawnAfterDelay(lastSpawnAt + DespawnDelayAfterLoot));
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in spawning suprise chest items " + e);
            }
        }

        // Runs on the client that opened the chest (TryTriggerOpen made it the owner). Plays the
        // despawn effect while the ZDO still exists, then removes the chest for everyone.
        private IEnumerator DespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            try
            {
                TriggerDespawnEffects();
                ZNetViewHelper.Destroy(gameObject);
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("TwitchSurpriseChest.DespawnAfterDelay failed: " + e);
            }
        }

        /// <summary>
        /// Decides what comes out of the chest: m_amount weighted picks when random is on, otherwise
        /// every eligible entry once. A creature entry is taken out of the eligible list once its Max
        /// spawned limit is reached - by the creatures already alive near the player plus the creatures
        /// earlier picks of this same draw will spawn (they don't exist yet, as spawning is delayed).
        /// Item entries have no limit. Entries that would spawn nothing (creature amount or item stack of 0)
        /// are never picked. With random on, the draw stops early if nothing eligible is left.
        /// </summary>
        private List<SurpriseChestSpawnData> PickLoot(List<SurpriseChestSpawnData> eligibleItems)
        {
            List<SurpriseChestSpawnData> picks = new List<SurpriseChestSpawnData>();
            SurpriseChestHelper.LootDraw draw = new SurpriseChestHelper.LootDraw();

            if (!m_random)
            {
                foreach (SurpriseChestSpawnData entry in eligibleItems)
                {
                    if (!draw.IsSpawnable(entry))
                        continue;

                    picks.Add(entry);
                    draw.AddPlanned(entry);
                }

                return picks;
            }

            List<SurpriseChestSpawnData> pool = new List<SurpriseChestSpawnData>(eligibleItems);

            for (int i = 0; i < m_amount; i++)
            {
                pool.RemoveAll(entry => !draw.IsSpawnable(entry));

                if (pool.Count == 0)
                    break;

                SurpriseChestSpawnData pick = PickWeighted(pool);
                picks.Add(pick);
                draw.AddPlanned(pick);
            }

            return picks;
        }

        /// <summary>
        /// Picks one entry with probability proportional to its <see cref="SurpriseChestSpawnData.weight"/>
        /// (weights &lt;= 0 are never picked). Falls back to a uniform pick if every weight is 0, so a
        /// misconfigured chest still spawns something instead of nothing.
        /// </summary>
        private static SurpriseChestSpawnData PickWeighted(List<SurpriseChestSpawnData> candidates)
        {
            float total = 0f;
            foreach (SurpriseChestSpawnData candidate in candidates)
                total += Mathf.Max(0f, candidate.weight);

            if (total <= 0f)
                return candidates[Random.Range(0, candidates.Count)];

            float roll = Random.value * total;
            SurpriseChestSpawnData lastPickable = null;

            foreach (SurpriseChestSpawnData candidate in candidates)
            {
                if (candidate.weight <= 0f)
                    continue;

                lastPickable = candidate;
                roll -= candidate.weight;
                if (roll < 0f)
                    return candidate;
            }

            // Float rounding can leave the roll just above zero after the last entry.
            return lastPickable;
        }

        private IEnumerator SpawnItem(SurpriseChestSpawnData spawnData, float force, float deviation, float delay)
        {
            yield return new WaitForSeconds(delay);

            try
            {
                m_spawnPoint.rotation = m_spawnPointOriginalRotation;
                m_spawnPoint.Rotate(Vector3.up, deviation);

                if (spawnData.creatureData != null)
                {
                    // The loot creatures credit the chest redeem's redeemer (tag read from this chest's base data).
                    TwitchBasePersistentData chestTag = gameObject.GetComponent<TwitchBasePersistentData>();
                    string chestRedeemerId = chestTag != null && chestTag.HasRedeemer ? chestTag.RedeemerId : null;

                    foreach (CreatureData creature in spawnData.creatureData)
                    {
                        if (!ProgressionHelper.IsAllowedByGlobalKeys(creature.globalKeyAdd, creature.globalKeyRemove))
                            continue;

                        if (creature.amount > 0)
                            CreatureHelper.SpawnCreatures(creature, m_spawnPoint, new CustomRewardEvent() { RedeemerName = m_redeemerName, RedeemerId = chestRedeemerId, CustomRewardTitle = m_redeemTitle }, false, force);
                    }
                }

                if (spawnData.itemData != null)
                    ItemHelper.SpawnItem(spawnData.itemData, m_spawnPoint, force);

                TriggerSpawnItemEffect();
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("TwitchSurpriseChest.SpawnItem failed: " + e);
            }
        }

        private void TriggerChestOpeningEffect()
        {
            chestOpeningEffects.Create(transform.position, transform.rotation);
        }

        private void TriggerDespawnEffects()
        {
            despawnEffect.Create(transform.position, transform.rotation);
        }

        private void TriggerSpawnEffects()
        {
            spawnEffects.Create(transform.position, transform.rotation);
        }

        private void TriggerSpawnItemEffect()
        {
            spawnItemEffects.Create(transform.position, transform.rotation);
        }

        private float ChangeAngleByIndex(int index)
        {
            switch (index % 5)
            {
                case 0:
                default:
                    return 0f;
                case 1:
                    return 25f;
                case 2:
                    return -25f;
                case 3:
                    return 50f;
                case 4:
                    return -50f;
            }
        }
    }
}
