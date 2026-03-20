using Jotunn.Managers;
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
        private readonly int chestRedeemDataHash = "SurpriseChestRedeemData_WBTI".GetStableHashCode();

        private string m_redeemTitle;
        private string m_redeemerName;
        private int m_amount;
        private float m_force;
        private float m_openDelay = 1.5f;
        private bool m_mimic;
        private bool m_random;
        private int m_yeetChance = 5;
        private float m_spawnDelay = 0.7f;

        public bool m_interact;
        public TwitchSurpriseChestInteract m_supriseChestInteract;
        public string m_type;

        EffectList endEffects = new EffectList();
        EffectList itemSpawnEffects = new EffectList();
        EffectList openingEffects = new EffectList();
        EffectList startEffects = new EffectList();

        public void Awake()
        {
            endEffects = WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffect;
            startEffects = WizshBoneTwitchIntegration.Instance.effectLists.SpawnEffect;

            EffectList.EffectData itemSpawnEffectData = new EffectList.EffectData();
            itemSpawnEffectData.m_enabled = true;
            itemSpawnEffectData.m_prefab = PrefabManager.Instance.GetPrefab("vfx_Place_wood_pole");
            itemSpawnEffectData.m_variant = -1;

            EffectList.EffectData itemSpawnSoundEffectData = new EffectList.EffectData();
            itemSpawnSoundEffectData.m_enabled = true;
            itemSpawnSoundEffectData.m_prefab = PrefabManager.Instance.GetPrefab("sfx_cooking_station_take");
            itemSpawnSoundEffectData.m_variant = -1;

            List<EffectList.EffectData> itemSpawnList = new List<EffectList.EffectData>();
            itemSpawnList.Add(itemSpawnEffectData);
            itemSpawnList.Add(itemSpawnSoundEffectData);

            itemSpawnEffects.m_effectPrefabs = itemSpawnList.ToArray();

            EffectList.EffectData openingGlowEffectData = new EffectList.EffectData();
            openingGlowEffectData.m_enabled = true;
            openingGlowEffectData.m_prefab = PrefabManager.Instance.GetPrefab("fx_HildirChest_Unlock");
            openingGlowEffectData.m_variant = 0;

            List<EffectList.EffectData> openingGlowList = new List<EffectList.EffectData>();
            openingGlowList.Add(openingGlowEffectData);

            openingEffects.m_effectPrefabs = openingGlowList.ToArray();

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

            string chestRedeemDataString = m_netView.GetZDO().GetString(chestRedeemDataHash, "");

            if (chestDataString == "")
            {
                Jotunn.Logger.LogError("Could not find corresponding redeem data for this chest!");
                return;
            }

            string[] chestRedeemData = chestRedeemDataString.Split('|');
            Jotunn.Logger.LogWarning("Retrieved Surprise Chest persitent data! " + chestRedeemData[0] + ", " + chestRedeemData[1]);

            SurpriseChestData chestData = StringToChestData(chestDataString);
            m_redeemerName = chestRedeemData[0];
            m_redeemTitle = chestRedeemData[1];
            m_amount = chestData.amount;
            m_force = chestData.force;
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
            //m_amount = chestData.amount > 5 ? 5 : chestData.amount < 1 ? 1 : chestData.amount;
            m_amount = chestData.amount;
            m_force = chestData.force;
            m_interact = chestData.interact;
            m_mimic = chestData.mimicChance == 0 ? false : Random.Range(0, 100) <= chestData.mimicChance;
            m_random = chestData.random;
            m_spawnDelay = chestData.spawnDelay;
            m_type = chestData.type;
            m_yeetChance = chestData.yeetChance;

            Minimap.instance.DiscoverLocation(transform.localPosition, Minimap.PinType.Icon3, "Surprise Chest", false);

            m_netView.GetZDO().Set(chestDataHash, ChestDataToString(chestData));
            m_netView.GetZDO().Set(chestRedeemDataHash, $"{m_redeemerName}|{m_redeemTitle}");

            TriggerStartEffects();

            if (!m_interact)
                StartCoroutine(OpenDelay());
        }

        public void Open()
        {
            m_supriseChestInteract.m_animator.enabled = true;
            TriggerOpeningEffect();
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
                List<SurpriseChestSpawnData> spawnList = RedeemHelper.GetSurpriseChestSpawnDataByTitle(m_redeemTitle);

                if (spawnList == null)
                {
                    Jotunn.Logger.LogError("Spawn list is null");
                    return;
                }

                if (!m_random)
                    m_amount = spawnList.Count;

                for (int index = 0; index < m_amount; index++)
                {
                    float angleChange = ChangeAngleByIndex(index);
                    float force = m_force;
                    SurpriseChestSpawnData spawnData;

                    if (m_random)
                        spawnData = spawnList[Random.Range(0, spawnList.Count)];
                    else
                        spawnData = spawnList[index];

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
                        CreatureHelper.SpawnCreatures(creature, spawnPointTrans, new CustomRewardEvent() { RedeemerName = m_redeemerName }, false);
                }
            }

            if (spawnData.itemData != null)
                ItemHelper.SpawnItem(spawnData.itemData, spawnPointTrans, force);

            TriggerItemSpawnEffect();
        }

        public void OnDestroy()
        {
            TriggerEndEffects();
            Minimap.instance.RemovePin(transform.position, 5f);
        }

        private void TriggerEndEffects()
        {
            endEffects.Create(transform.position, transform.rotation);
        }

        private void TriggerItemSpawnEffect()
        {
            itemSpawnEffects.Create(transform.position, transform.rotation);
        }

        private void TriggerOpeningEffect()
        {
            openingEffects.Create(transform.position, transform.rotation);
        }

        private void TriggerStartEffects()
        {
            startEffects.Create(transform.position, transform.rotation);
        }

        private float ChangeAngleByIndex(int index)
        {
            switch (index)
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

        public string ChestDataToString(SurpriseChestData chestData)
        {
            string items = "";

            if (items != "")
                items = items.Remove(items.Length - 1);

            return $"{chestData.amount}|{chestData.announceMessage}|{chestData.force}|{chestData.interact}|{chestData.mimicChance}|{chestData.random}|{chestData.spawnDelay}|{chestData.type}|{chestData.yeetChance}";
        }

        public SurpriseChestData StringToChestData(string value)
        {
            string[] data = value.Split('|');
            string[] items = data[4].Split(';');
            SurpriseChestData chestData = new SurpriseChestData();
            chestData.amount = int.Parse(data[0]);
            chestData.announceMessage = data[1];
            chestData.force = float.Parse(data[2]);
            chestData.interact = bool.Parse(data[3]);
            chestData.mimicChance = int.Parse(data[4]);
            chestData.random = bool.Parse(data[5]);
            chestData.spawnDelay = float.Parse(data[6]);
            chestData.type = data[7];
            chestData.yeetChance = int.Parse(data[8]);

            return chestData;
        }
    }
}
