using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchSurpriseChest : MonoBehaviour
    {
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

            m_supriseChestInteract = transform.Find("chest_top").gameObject.AddComponent<TwitchSurpriseChestInteract>();

            //Jotunn.Logger.LogWarning("Force: " + m_force);
            //Jotunn.Logger.LogWarning("Interact: " + m_interact);
            //Jotunn.Logger.LogWarning("Mimic: " + m_mimic);
            //Jotunn.Logger.LogWarning("Random: " + m_random);
            //Jotunn.Logger.LogWarning("YeetChance: " + m_yeetChance);

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
    }
}
