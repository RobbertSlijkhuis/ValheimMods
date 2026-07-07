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

        public void OnDestroy()
        {
            try
            {
                if (m_netView != null && m_netView.IsOwner() && despawnEffect != null)
                    despawnEffect.Create(transform.position, transform.rotation);

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

                List<SurpriseChestSpawnData> eligibleItems = GetEligibleItems();

                if (eligibleItems.Count == 0)
                {
                    Jotunn.Logger.LogWarning("No eligible surprise chest items to spawn (all filtered out for this dungeon)");
                    return;
                }

                if (!m_random)
                    m_amount = eligibleItems.Count;

                float timeOffset = 0f;

                for (int index = 0; index < m_amount; index++)
                {
                    float angleChange = ChangeAngleByIndex(index);
                    float force = m_yeetChance > 0 && Random.Range(0, 100) <= m_yeetChance ? 1000f : m_force;
                    SurpriseChestSpawnData spawnData = m_random ? eligibleItems[Random.Range(0, eligibleItems.Count)] : eligibleItems[index];

                    StartCoroutine(SpawnItem(spawnData, force, angleChange, timeOffset));
                    timeOffset += m_spawnDelay;
                }

                TimedDestruction timedDestruction = gameObject.AddComponent<TimedDestruction>();
                timedDestruction.m_timeout = 10f;
                timedDestruction.Trigger();
            }
            catch (System.Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in spawning suprise chest items " + e);
            }
        }

        /// <summary>
        /// Returns the chest's configured items, minus any dungeon-forbidden creatures, so a
        /// dungeon-forbidden roll can never happen (rather than silently spawning nothing once
        /// picked). Items are only cloned/modified when something actually needs stripping;
        /// an item is dropped entirely only if that would leave it with nothing left to spawn.
        /// </summary>
        private List<SurpriseChestSpawnData> GetEligibleItems()
        {
            if (!Player.m_localPlayer.InInterior())
                return m_items;

            List<string> forbidden = CreatureHelper.GetDungeonForbiddenCreatures();
            List<SurpriseChestSpawnData> eligible = new List<SurpriseChestSpawnData>();

            foreach (SurpriseChestSpawnData item in m_items)
            {
                if (item.creatureData == null)
                {
                    eligible.Add(item);
                    continue;
                }

                List<CreatureData> allowedCreatures = item.creatureData.FindAll(c => !forbidden.Contains(c.prefabName));

                if (allowedCreatures.Count == item.creatureData.Count)
                {
                    eligible.Add(item);
                    continue;
                }

                if (allowedCreatures.Count == 0 && item.itemData == null)
                    continue;

                SurpriseChestSpawnData filtered = item.Clone<SurpriseChestSpawnData>();
                filtered.creatureData = allowedCreatures;
                eligible.Add(filtered);
            }

            return eligible;
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
                    foreach (CreatureData creature in spawnData.creatureData)
                    {
                        if (!ProgressionHelper.IsAllowedByGlobalKeys(creature.globalKeyAdd, creature.globalKeyRemove))
                            continue;

                        if (creature.amount > 0)
                            CreatureHelper.SpawnCreatures(creature, m_spawnPoint, new CustomRewardEvent() { RedeemerName = m_redeemerName }, false, force);
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
