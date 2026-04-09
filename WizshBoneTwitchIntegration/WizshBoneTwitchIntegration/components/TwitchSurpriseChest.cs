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

        private string m_redeemTitle;
        private string m_redeemerName;
        private int m_amount;
        private float m_force;
        private List<SurpriseChestSpawnData> m_items;
        private float m_openDelay = 1.5f;
        private bool m_mimic;
        private bool m_random;
        private int m_yeetChance = 5;
        private float m_spawnDelay = 0.7f;

        public bool m_interact;
        public TwitchSurpriseChestInteract m_supriseChestInteract;
        public string m_type;

        EffectList chestOpeningEffects = new EffectList();
        EffectList despawnEffect = new EffectList();
        EffectList spawnEffects = new EffectList();
        EffectList spawnItemEffects = new EffectList();

        public void Awake()
        {
            chestOpeningEffects = WizshBoneTwitchIntegration.Instance.effectLists.ChestOpenEffect;
            despawnEffect = WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium;
            spawnEffects = WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffectMedium;
            spawnItemEffects = WizshBoneTwitchIntegration.Instance.effectLists.SpawnItemEffect;

            m_netView = gameObject.GetComponent<ZNetView>();
            m_supriseChestInteract = transform.Find("chest_top").gameObject.GetComponent<TwitchSurpriseChestInteract>();

            if (m_netView == null || m_netView.GetZDO() == null)
            {
                Jotunn.Logger.LogError("Could not find ZNetView on surprise chest!");
                return;
            }

            string chestDataString = m_netView.GetZDO().GetString(chestDataHash, "");

            if (chestDataString == "")
                return;

            string[] chestRedeemData = chestDataString.Split('|');
            m_redeemerName = chestRedeemData[0];
            m_redeemTitle = chestRedeemData[1];
            Jotunn.Logger.LogWarning("Retrieved Surprise Chest persitent data: " + m_redeemerName + ", " + m_redeemTitle);

            RedeemEntry redeem = RedeemHelper.GetRedeemByTitle(m_redeemTitle);

            if (redeem == null || redeem.chestData == null)
            {
                Jotunn.Logger.LogError("Could not find redeem by title or chestdata is null!");
                return;
            }

            SurpriseChestData chestData = redeem.chestData;
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

        public void Init(SurpriseChestData chestData, CustomRewardEvent customRewardEvent)
        {
            m_redeemerName = customRewardEvent.RedeemerName;
            m_redeemTitle = customRewardEvent.CustomRewardTitle;
            m_amount = chestData.amount;
            m_force = chestData.force;
            m_items = chestData.items;
            m_interact = chestData.interact;
            m_mimic = chestData.mimicChance == 0 ? false : Random.Range(0, 100) <= chestData.mimicChance;
            m_random = chestData.random;
            m_spawnDelay = chestData.spawnDelay;
            m_type = chestData.type;
            m_yeetChance = chestData.yeetChance;

            Minimap.instance.DiscoverLocation(transform.localPosition, Minimap.PinType.Icon3, "Surprise Chest", false);

            m_netView.GetZDO().Set(chestDataHash, $"{m_redeemerName}|{m_redeemTitle}");

            TriggerSpawnEffects();

            if (!m_interact)
                StartCoroutine(OpenDelay());
        }

        public void Open()
        {
            m_supriseChestInteract.m_animator.SetTrigger("Open");
            TriggerChestOpeningEffect();
            Invoke(nameof(SpawnItems), 4f);
        }

        public IEnumerator OpenDelay()
        {
            yield return new WaitForSeconds(m_openDelay);

            Open();
        }

        private void SpawnItems()
        {
            try
            {
                float timeOffset = 0f;

                if (m_items == null)
                {
                    Jotunn.Logger.LogError("No items to spawn!");
                    return;
                }

                if (!m_random)
                    m_amount = m_items.Count;

                for (int index = 0; index < m_amount; index++)
                {
                    float angleChange = ChangeAngleByIndex(index);
                    float force = m_force;
                    SurpriseChestSpawnData spawnData;

                    if (m_random)
                        spawnData = m_items[Random.Range(0, m_items.Count)];
                    else
                        spawnData = m_items[index];

                    if (m_yeetChance > 0 && Random.Range(0, 100) <= m_yeetChance)
                        force = 1000f;

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

        private IEnumerator SpawnItem(SurpriseChestSpawnData spawnData, float force, float deviation, float delay)
        {
            yield return new WaitForSeconds(delay);

            Transform spawnPointTrans = transform.Find("spawnpoint");
            spawnPointTrans.Rotate(Vector3.up, deviation);

            if (spawnData.creatureData != null)
            {
                foreach (CreatureData creature in spawnData.creatureData)
                {
                    if (!ProgressionHelper.IsAllowedByGlobalKeys(creature.globalKeyAdd, creature.globalKeyRemove))
                        continue;

                    if (creature.amount > 0)
                        CreatureHelper.SpawnCreatures(creature, spawnPointTrans, new CustomRewardEvent() { RedeemerName = m_redeemerName }, false, force);
                }
            }

            if (spawnData.itemData != null)
                ItemHelper.SpawnItem(spawnData.itemData, spawnPointTrans, force);

            TriggerSpawnItemEffect();
        }

        public void OnDestroy()
        {
            TriggerDespawnEffects();
            Minimap.instance.RemovePin(transform.position, 5f);
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
                    return -50f;
                case 3:
                    return 75f;
                case 4:
                    return -100f;
            }
        }
    }
}
