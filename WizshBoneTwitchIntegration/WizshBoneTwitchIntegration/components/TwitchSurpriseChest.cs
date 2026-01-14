using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchSurpriseChest : MonoBehaviour
    {
        ZNetView m_netView;
        private readonly int chestDataHash = "SurpriseChestData_WBTI".GetStableHashCode();

        private int m_amount;
        private float m_force;
        private List<string> m_items = new List<string>();
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
            EffectList.EffectData chestSpawnEffectData = new EffectList.EffectData();
            chestSpawnEffectData.m_enabled = true;
            chestSpawnEffectData.m_prefab = PrefabManager.Instance.GetPrefab("vfx_corpse_destruction_small");
            chestSpawnEffectData.m_variant = -1;

            List<EffectList.EffectData> chestSpawnList = new List<EffectList.EffectData>();
            chestSpawnList.Add(chestSpawnEffectData);

            endEffects.m_effectPrefabs = chestSpawnList.ToArray();
            startEffects.m_effectPrefabs = chestSpawnList.ToArray();

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

            string dataString = m_netView.GetZDO().GetString(chestDataHash, "");

            if (dataString == "")
                return;

            ChestData chestData = StringToChestData(dataString);
            m_amount = chestData.amount;
            m_force = chestData.force;
            m_interact = chestData.interact;
            m_items = chestData.items;
            m_mimic = chestData.mimicChance == 0 ? false : Random.Range(0, 100) <= chestData.mimicChance;
            m_random = chestData.random;
            m_type = chestData.type;
            m_yeetChance = chestData.yeetChance;
        }

        public void Init(ChestData chestData)
        {
            m_amount = chestData.amount > 5 ? 5 : chestData.amount < 1 ? 1 : chestData.amount;
            m_force = chestData.force;
            m_interact = chestData.interact;
            m_items = chestData.items;
            m_mimic = chestData.mimicChance == 0 ? false : Random.Range(0, 100) <= chestData.mimicChance;
            m_random = chestData.random;
            m_type = chestData.type;
            m_yeetChance = chestData.yeetChance;

            m_netView.GetZDO().Set(chestDataHash, ChestDataToString(chestData));

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
            float timeOffset = 0f;

            for (int index = 0; index < m_amount; index++)
            {
                float angleChange = ChangeAngleByIndex(index);
                float force = m_force;
                string item;

                if (m_random)
                    item = m_items[Random.Range(0, m_items.Count)];
                else
                    item = m_items[index];

                if (m_yeetChance > 0 && Random.Range(0, 100) <= m_yeetChance)
                    force = 1000f;

                StartCoroutine(SpawnItem(item, force, angleChange, timeOffset));
                timeOffset += m_spawnDelay;
            }

            TimedDestruction timedDestruction = gameObject.AddComponent<TimedDestruction>();
            timedDestruction.m_timeout = 10f;
            timedDestruction.Trigger();
        }

        private IEnumerator SpawnItem(string prefabName, float force, float deviation, float delay)
        {
            yield return new WaitForSeconds(delay);

            Transform spawnPointTrans = transform.Find("spawnpoint");
            spawnPointTrans.Rotate(Vector3.up, deviation);

            GameObject prefab = PrefabManager.Instance.GetPrefab(prefabName);
            GameObject spawned = UnityEngine.Object.Instantiate(prefab, spawnPointTrans.position, spawnPointTrans.rotation);
            Rigidbody rigidBody = spawned.GetComponent<Rigidbody>();
            rigidBody.AddForce((spawnPointTrans.forward * force) + (spawnPointTrans.up * force));
            TriggerItemSpawnEffect();
        }

        public void OnDestroy()
        {
            TriggerEndEffects();
            Minimap.instance.RemovePin(transform.position, 3f);
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

        public string ChestDataToString(ChestData chestData)
        {
            string items = "";

            foreach (string item in chestData.items)
            {
                items += $"{item};";
            }

            if (items != "")
                items = items.Remove(items.Length - 1);

            return $"{chestData.amount}|{chestData.announceMessage}|{chestData.force}|{chestData.interact}|{items}|{chestData.mimicChance}|{chestData.random}|{chestData.type}|{chestData.yeetChance}";
        }

        public ChestData StringToChestData(string value)
        {
            string[] data = value.Split('|');
            string[] items = data[4].Split(';');
            ChestData chestData = new ChestData();
            chestData.amount = int.Parse(data[0]);
            chestData.announceMessage = data[1];
            chestData.force = float.Parse(data[2]);
            chestData.interact = bool.Parse(data[3]);
            chestData.items = items.ToList();
            chestData.mimicChance = int.Parse(data[5]);
            chestData.random = bool.Parse(data[6]); ;
            chestData.type = data[7];
            chestData.yeetChance = int.Parse(data[8]);

            return chestData;
        }
    }
}
