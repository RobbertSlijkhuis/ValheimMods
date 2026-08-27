using System.Collections.Generic;
using System.IO;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class RedeemHelper
    {

        public static List<RedeemData> redeems = new List<RedeemData>();
        public static List<CreatureGroupData> creatureGroups = new List<CreatureGroupData>();
        public static PlayerSnapshot playerSnapshot;

        // Tracks which profile the lists above actually belong to, so a missing-file self-heal
        // can tell "this exact profile was already loaded and then lost its file" (safe to
        // restore from memory) apart from "a different/never-loaded profile is being read"
        // (must fall back to the embedded stock template instead - there's nothing else to use).
        private static string m_loadedProfileName;

        public static bool ReadRedeems()
        {
            string activeProfile = ProfileManager.ActiveProfile;
            string path = ProfileManager.GetActiveRedeemPath();

            if (!File.Exists(path) && m_loadedProfileName == activeProfile)
            {
                Jotunn.Logger.LogWarning($"[WBTI] Active profile file at '{path}' is missing - restoring it from the currently loaded redeems/settings instead of resetting to defaults.");
                ExtraConfigHelper.WriteRedeemsConfig(path, ProfileSettingsHelper.Current, creatureGroups, redeems);
                return true;
            }

            // ReadRedeemsConfig() self-heals from the embedded stock template if the file is
            // still missing at this point - i.e. this profile has never been loaded before.
            // Guaranteed non-null redeems/creatureGroups - see ReadRedeemsConfig.
            ModData data = ExtraConfigHelper.ReadRedeemsConfig(path);
            redeems = data.redeems;
            creatureGroups = data.creatureGroups;
            m_loadedProfileName = activeProfile;
            return true;
        }

        public static bool Reload()
        {
            return ReadRedeems();
        }

        public static RedeemData GetRedeemByTitle(string value)
        {
            TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

            if (customRewards == null)
            {
                Jotunn.Logger.LogError("Could not find custom rewards component to retrieve surprise chest spawn data");
                return null;
            }

            return redeems.Find(item => item.title == value);
        }

        //public static void SetPlayerSpeed(float multiplier)
        //{
        //    if (playerSnapshot == null)
        //        playerSnapshot = new PlayerSnapshot(Player.m_localPlayer);

        //    Player.m_localPlayer.m_jumpForce = playerSnapshot.jumpForce * multiplier;
        //    Player.m_localPlayer.m_jumpForceForward = playerSnapshot.jumpForceForward * multiplier;

        //    Player.m_localPlayer.m_crouchSpeed = playerSnapshot.crouchSpeed * multiplier;
        //    Player.m_localPlayer.m_runSpeed = playerSnapshot.runSpeed * multiplier;
        //    Player.m_localPlayer.m_speed = playerSnapshot.speed * multiplier;
        //    Player.m_localPlayer.m_walkSpeed = playerSnapshot.walkSpeed * multiplier;

        //    Player.m_localPlayer.m_swimDepth = playerSnapshot.swimDepth * multiplier;
        //    Player.m_localPlayer.m_swimSpeed = playerSnapshot.swimSpeed * multiplier;

        //    // Player.m_localPlayer.m_maxCarryWeight = playerSnapshot.maxCarryWeight * multiplier;
        //}

        //public static void ResetPlayerSpeed(Player player)
        //{
        //    playerSnapshot.Apply(player);
        //}

        public static List<CreatureData> GetResolvedCreatureList(SpawnCreatureData spawnCreatureData)
        {
            List<CreatureData> resolved = new List<CreatureData>();

            foreach (CreatureData entry in spawnCreatureData.list)
            {
                if (!string.IsNullOrEmpty(entry.prefabName))
                {
                    resolved.Add(entry);
                    continue;
                }

                if (entry.group == null)
                    continue;

                CreatureGroupData group = creatureGroups.Find(g => g.group == entry.group);

                if (group != null)
                    resolved.AddRange(group.list);
            }

            return resolved;
        }

        //private static void ClearDamage(GameObject attack)
        //{
        //    ItemDrop itemDrop = attack.GetComponent<ItemDrop>();
        //    Aoe aoe = attack.GetComponent<Aoe>();

        //    if (itemDrop != null)
        //    {
        //        // HitData.DamageTypes damages = itemDrop.m_itemData.m_shared.m_damages;

        //        //damages.m_blunt = damages.m_blunt * 0.5f;
        //        //damages.m_chop = damages.m_chop * 0.5f;
        //        //damages.m_damage = damages.m_damage * 0.5f;
        //        //damages.m_fire = damages.m_fire * 0.5f;
        //        //damages.m_frost = damages.m_frost * 0.5f;
        //        //damages.m_lightning = damages.m_lightning * 0.5f;
        //        //damages.m_pickaxe = damages.m_pickaxe * 0.5f;
        //        //damages.m_pierce = damages.m_pierce * 0.5f;
        //        //damages.m_poison = damages.m_poison * 0.5f;
        //        //damages.m_slash = damages.m_slash * 0.5f;
        //        //damages.m_spirit = damages.m_spirit * 0.5f;

        //        itemDrop.m_itemData.m_shared.m_damages.m_blunt = 0f;
        //        itemDrop.m_itemData.m_shared.m_damages.m_chop = 0f;
        //        itemDrop.m_itemData.m_shared.m_damages.m_damage = 1f;
        //        itemDrop.m_itemData.m_shared.m_damages.m_fire = 0f;
        //        itemDrop.m_itemData.m_shared.m_damages.m_frost = 0f;
        //        itemDrop.m_itemData.m_shared.m_damages.m_lightning = 0f;
        //        itemDrop.m_itemData.m_shared.m_damages.m_pickaxe = 0f;
        //        itemDrop.m_itemData.m_shared.m_damages.m_pierce = 0f;
        //        itemDrop.m_itemData.m_shared.m_damages.m_poison = 0f;
        //        itemDrop.m_itemData.m_shared.m_damages.m_slash = 0f;
        //        itemDrop.m_itemData.m_shared.m_damages.m_spirit = 0f;

        //        // itemDrop.m_itemData.m_shared.m_damages = new HitData.DamageTypes();
        //    }

        //    if (aoe != null)
        //    {
        //        // HitData.DamageTypes damages = aoe.m_damage;

        //        //damages.m_blunt = damages.m_blunt * 0.5f;
        //        //damages.m_chop = damages.m_chop * 0.5f;
        //        //damages.m_damage = damages.m_damage * 0.5f;
        //        //damages.m_fire = damages.m_fire * 0.5f;
        //        //damages.m_frost = damages.m_frost * 0.5f;
        //        //damages.m_lightning = damages.m_lightning * 0.5f;
        //        //damages.m_pickaxe = damages.m_pickaxe * 0.5f;
        //        //damages.m_pierce = damages.m_pierce * 0.5f;
        //        //damages.m_poison = damages.m_poison * 0.5f;
        //        //damages.m_slash = damages.m_slash * 0.5f;
        //        //damages.m_spirit = damages.m_spirit * 0.5f;

        //        aoe.m_damage.m_blunt = 0f;
        //        aoe.m_damage.m_chop = 0f;
        //        aoe.m_damage.m_damage = 1f;
        //        aoe.m_damage.m_fire = 0f;
        //        aoe.m_damage.m_frost = 0f;
        //        aoe.m_damage.m_lightning = 0f;
        //        aoe.m_damage.m_pickaxe = 0f;
        //        aoe.m_damage.m_pierce = 0f;
        //        aoe.m_damage.m_poison = 0f;
        //        aoe.m_damage.m_slash = 0f;
        //        aoe.m_damage.m_spirit = 0f;

        //        // aoe.m_damage = new HitData.DamageTypes();
        //    }
        //}

        //private static void DeepSearchForAttacks(GameObject attack)
        //{
        //    ItemDrop itemDrop = attack.GetComponent<ItemDrop>();

        //    if (itemDrop != null)
        //    {
        //        ClearDamage(attack);

        //        if (itemDrop.m_itemData.m_shared.m_spawnOnHit != null)
        //        {
        //            GameObject spawnOnHit = UnityEngine.Object.Instantiate(itemDrop.m_itemData.m_shared.m_spawnOnHit);
        //            ClearDamage(spawnOnHit);
        //            itemDrop.m_itemData.m_shared.m_spawnOnHit = spawnOnHit;
        //        }

        //        if (itemDrop.m_itemData.m_shared.m_spawnOnHitTerrain)
        //        {
        //            GameObject spawnOnHitTerrain = UnityEngine.Object.Instantiate(itemDrop.m_itemData.m_shared.m_spawnOnHitTerrain);
        //            ClearDamage(spawnOnHitTerrain);
        //            itemDrop.m_itemData.m_shared.m_spawnOnHitTerrain = spawnOnHitTerrain;
        //        }

        //        if (itemDrop.m_itemData.m_shared.m_triggerEffect.m_effectPrefabs.Length > 0)
        //        {
        //            List<EffectList.EffectData> effectList = new List<EffectList.EffectData>();

        //            foreach (var effect in itemDrop.m_itemData.m_shared.m_triggerEffect.m_effectPrefabs)
        //            {
        //                EffectList.EffectData effectData = new EffectList.EffectData();
        //                GameObject effectPrefab = UnityEngine.Object.Instantiate(effect.m_prefab);
        //                ClearDamage(effectPrefab);
        //                effectData.m_prefab = effectPrefab;
        //                effectData.m_enabled = true;
        //                effectData.m_variant = -1;
        //                effectList.Add(effectData);
        //            }

        //            itemDrop.m_itemData.m_shared.m_triggerEffect.m_effectPrefabs = effectList.ToArray();
        //        }
        //    }
        //}
    }
}
