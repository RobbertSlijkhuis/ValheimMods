using ModularMagic_BloodMagic.Components;
using ModularMagic_BloodMagic.Configs;
using ModularMagic_BloodMagic.Models;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using DamageHelper = ModularMagic_Core.Helpers.DamageHelper;
using ImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;
using ImbuementType = ModularMagic_Core.Types.ImbuementType;
using SpawnedCreatureHelper = ModularMagic_Core.Helpers.SpawnedCreatureHelper;

namespace ModularMagic_BloodMagic.Helpers
{
    /// <summary>
    /// The apparitions are spawned by the SpawnAbility of the scythe, so the game handles the spawn (the position, the alert and the Blood
    /// Magic damage factor of m_copySkill). Core calls OnSpawned for every creature the SpawnAbility spawns. The number of apparitions a
    /// player can have is counted here, per summoner: the game counts every loaded creature of the prefab, wild Charred included.
    /// An apparition can not die. At 0 health it is a husk until its summoner pays health to reanimate it, see ApparitionController.
    /// </summary>
    internal static class ApparitionHelper
    {
        private static readonly string[] CreatureNames = { "Charred_Melee", "Charred_Twitcher", "Charred_Archer" };

        // The strongest attack of all apparitions with the damage factor at 1, see GetBaselineDamage
        private static float baselineDamage = -1f;

        // The apparitions that are loaded on this client, of every player
        private static readonly Dictionary<Character, ApparitionController> apparitions = new Dictionary<Character, ApparitionController>();

        // The summons of the local player that the game has not spawned yet (a SpawnAbility waits before it spawns), as the time each one
        // is given up on. They count as apparitions, so a second summon in that time can not go over the maximum
        private static readonly List<float> pendingSummons = new List<float>();

        // How long after its spawn delay a summon is no longer expected, for a spawn the game has skipped
        private const float PendingSummonMargin = 5f;

        /// <summary>Called by the controller of an apparition when it knows its summoner.</summary>
        public static void Register(ApparitionController apparition)
        {
            if (apparition.Character != null)
                apparitions[apparition.Character] = apparition;
        }

        public static void Unregister(ApparitionController apparition)
        {
            if (apparition.Character is null)
                return;

            apparitions.Remove(apparition.Character);
        }

        /// <summary>The apparitions that are loaded on this client, of every player.</summary>
        public static IEnumerable<ApparitionController> Loaded => apparitions.Values;

        /// <summary>True for a loaded apparition with a summoner, active or a husk.</summary>
        public static bool IsApparition(Character character)
        {
            return character is not null && apparitions.ContainsKey(character);
        }

        /// <summary>
        /// True when the character has the data of an apparition in its ZDO. Unlike IsApparition it also works while the character
        /// is still being set up (before the Awake of its controller).
        /// </summary>
        public static bool HasApparitionData(Character character)
        {
            if (character == null || character.m_nview == null)
                return false;

            return ApparitionController.HasApparitionData(character.m_nview.GetZDO());
        }

        /// <summary>The controller of a loaded apparition with a summoner. False for every other character.</summary>
        public static bool TryGet(Character character, out ApparitionController apparition)
        {
            apparition = null;

            // This is asked for every attack and every AI update of every creature, so without apparitions it is done right away
            if (apparitions.Count == 0 || character is null)
                return false;

            return apparitions.TryGetValue(character, out apparition);
        }

        /// <summary>True for an apparition that is banished: it has no health left and waits for its summoner.</summary>
        public static bool IsHusk(Character character)
        {
            // This is asked for every enemy with a target, so without apparitions it is done right away
            if (apparitions.Count == 0 || character is null)
                return false;

            return apparitions.TryGetValue(character, out ApparitionController apparition) && apparition.IsHusk;
        }

        /// <summary>
        /// The number of loaded apparitions of a player, husks included. 'active' is the number of them that is not a husk.
        /// </summary>
        public static int Count(long playerID, out int active)
        {
            int total = 0;
            active = 0;

            foreach (ApparitionController apparition in apparitions.Values)
            {
                if (apparition == null || apparition.Summoner != playerID)
                    continue;

                total++;

                if (!apparition.IsHusk)
                    active++;
            }

            return total;
        }

        /// <summary>
        /// The number of apparitions of the local player that take a slot: the loaded ones (husks included) and the summons that are
        /// on their way.
        /// </summary>
        public static int CountLocalPlayerSlots()
        {
            if (Player.m_localPlayer == null)
                return 0;

            return Count(Player.m_localPlayer.GetPlayerID(), out _) + CountPendingSummons();
        }

        // The quality and the Legion level of the last scythe the player equipped, as "quality,level". It is saved with the character, so the
        // limit is known after a relog and while no scythe is in the hand (to reanimate a husk)
        private const string LastScytheKey = "MMBM_LastScythe";

        // An id in the data of a scythe, so an upgraded scythe (a new item, see OnScytheUpgraded) is known to be the last equipped one
        private const string ScytheIdKey = "MMBM_ScytheId";

        /// <summary>
        /// Called when a character equipped an item. For a scythe of the local player, its quality and the level of its Legion rune are
        /// remembered for the limit of apparitions (GetMaxApparitions), as "quality,legion,id". The runes of a scythe can only change on
        /// the rune table, so the scythe is equipped again after a change. An upgrade is handled by OnScytheUpgraded.
        /// </summary>
        public static void RememberScythe(Humanoid humanoid, ItemDrop.ItemData item)
        {
            if (humanoid == null || humanoid != Player.m_localPlayer || item == null || ModularMagic_BloodMagic.Instance.definition == null)
                return;

            if (!ModularMagic_BloodMagic.Instance.definition.IsStaff(item))
                return;

            if (!item.m_customData.TryGetValue(ScytheIdKey, out string id) || string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString("N");
                item.m_customData[ScytheIdKey] = id;
            }

            int level = ImbuementHelper.GetLevel(item, ImbuementType.SummonLimit);
            Player.m_localPlayer.m_customData[LastScytheKey] = $"{Mathf.Max(1, item.m_quality)},{level},{id}";
        }

        /// <summary>
        /// Called by Core when an item was upgraded. The game replaces the item with a new one, Core has already moved the runes onto it.
        /// The id of the scythe moves along, and when it is the last equipped scythe the limit takes its new quality right away.
        /// </summary>
        public static void OnScytheUpgraded(ItemDrop.ItemData from, ItemDrop.ItemData to)
        {
            if (from == null || to == null || ModularMagic_BloodMagic.Instance.definition == null || !ModularMagic_BloodMagic.Instance.definition.IsStaff(to))
                return;

            if (!from.m_customData.TryGetValue(ScytheIdKey, out string id) || string.IsNullOrEmpty(id))
                return;

            to.m_customData[ScytheIdKey] = id;

            Player player = Player.m_localPlayer;
            if (player == null || !player.m_customData.TryGetValue(LastScytheKey, out string value) || string.IsNullOrEmpty(value))
                return;

            string[] parts = value.Split(',');
            if (parts.Length == 3 && parts[2] == id)
                RememberScythe(player, to);
        }

        /// <summary>
        /// The number of apparitions the local player can have: the config base, one more for every quality level of the last equipped scythe
        /// above 1, and the Legion rune of that scythe. It is worked out with the current config, so a changed value counts right away.
        /// </summary>
        public static int GetMaxApparitions()
        {
            int quality = 1;
            int legion = 0;
            Player player = Player.m_localPlayer;

            if (player != null && player.m_customData.TryGetValue(LastScytheKey, out string value) && !string.IsNullOrEmpty(value))
            {
                string[] parts = value.Split(',');

                if (parts.Length >= 2)
                {
                    int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out quality);
                    int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out legion);
                }
            }

            int perLegionLevel = PluginConfig.imbuementConfig.SummonLimit != null ? PluginConfig.imbuementConfig.SummonLimit.Value : 0;

            return PluginConfig.maxApparitions.Value
                + PluginConfig.maxApparitionsPerQuality.Value * (Mathf.Max(1, quality) - 1)
                + perLegionLevel * Mathf.Max(0, legion);
        }

        /// <summary>The summons of the local player that the game has not spawned yet.</summary>
        public static int CountPendingSummons()
        {
            pendingSummons.RemoveAll(expires => Time.time > expires);
            return pendingSummons.Count;
        }

        /// <summary>
        /// Called when the local player starts a summon with this spawn ability. The summon takes a slot until the game has spawned it.
        /// </summary>
        public static void AddPendingSummon(SpawnAbility spawnAbility)
        {
            pendingSummons.Add(Time.time + spawnAbility.m_initialSpawnDelay + spawnAbility.m_preSpawnDelay + PendingSummonMargin);
        }

        /// <summary>
        /// The effect of an apparition that becomes a husk or is reanimated. The effect is networked, so only one client spawns it.
        /// </summary>
        public static void SpawnStateEffect(Vector3 position)
        {
            GameObject effect = ModularMagic_BloodMagic.Instance.prefabs.SporeEffect;

            if (effect != null)
                Object.Instantiate(effect, position + Vector3.up, Quaternion.identity);
        }

        /// <summary>
        /// A vanilla effect or sound, looked up by name when it is first needed (in game, when the vanilla prefabs are loaded). An effect
        /// with a ZNetView is sent to every client, so only one client may spawn it; one without is only seen on the client that spawns it.
        /// </summary>
        private sealed class VanillaEffect
        {
            private readonly string name;
            private GameObject? prefab;
            private bool lookedUp;

            public VanillaEffect(string name)
            {
                this.name = name;
            }

            public GameObject? Prefab
            {
                get
                {
                    if (!lookedUp)
                    {
                        lookedUp = true;
                        prefab = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab(name) : null;

                        if (prefab == null)
                            prefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab(name);

                        if (prefab == null)
                            Jotunn.Logger.LogWarning($"[Apparition] Could not find the effect '{name}', it is not played");
                    }

                    return prefab;
                }
            }

            public bool IsNetworked => Prefab != null && Prefab.GetComponent<ZNetView>() != null;

            public void Spawn(Vector3 position)
            {
                if (Prefab != null)
                    Object.Instantiate(Prefab, position, Quaternion.identity);
            }
        }

        private static readonly VanillaEffect reanimateEffect = new VanillaEffect("fx_shield_start");
        private static readonly VanillaEffect banishSound = new VanillaEffect("sfx_trainingdummy_stagger");

        /// <summary>
        /// True when the reanimation effect has a ZNetView: it is then sent to every client, so only the owner of the husk spawns it.
        /// </summary>
        public static bool IsReanimateEffectNetworked => reanimateEffect.IsNetworked;

        /// <summary>The effect at the start of a reanimation, see ApparitionController.ApplyPaidReanimation.</summary>
        public static void SpawnReanimateEffect(Vector3 position)
        {
            reanimateEffect.Spawn(position);
        }

        /// <summary>
        /// The sound of an apparition that becomes a husk, called on every client that sees it happen. A networked sound is played by the
        /// owner only, otherwise every client plays it for itself. It is not a child of the husk, so the muting of the husk does not stop it.
        /// </summary>
        public static void PlayBanishSound(Vector3 position, bool owner)
        {
            if (owner || !banishSound.IsNetworked)
                banishSound.Spawn(position);
        }

        /// <summary>The health it costs the player to reanimate a husk.</summary>
        public static float GetReanimateCost(Player player)
        {
            return player.GetMaxHealth() * PluginConfig.reanimateHealthCost.Value;
        }

        /// <summary>
        /// The reanimation of this apparition its summoner has paid for, as the number of the reanimation (the first one is 1). It is
        /// stored in the ZDO of the summoner, which only the summoner writes.
        /// </summary>
        public static int GetPaidReanimation(Player summoner, ApparitionController apparition)
        {
            ZDO zdo = summoner.m_nview != null ? summoner.m_nview.GetZDO() : null;
            int key = apparition.ReanimateKey;

            return zdo != null && key != 0 ? zdo.GetInt(key, 0) : 0;
        }

        /// <summary>
        /// Called when a player uses a tameable. Returns true when it is a husk, the use is then handled here (result is what the game
        /// gets back) and the game does not pet it. Holding the key and the rename are left to the game.
        /// </summary>
        public static bool TryUseHusk(Tameable tameable, Humanoid user, bool hold, bool alt, out bool result)
        {
            result = false;

            Character character = tameable != null ? tameable.m_character : null;

            if (hold || alt || !IsHusk(character))
                return false;

            result = TryRequestReanimation(character, user);
            return true;
        }

        /// <summary>
        /// The local player uses a husk. The summoner pays health and writes the number of the reanimation in its own ZDO. The owner of the
        /// husk sees that and reanimates it (ApparitionController.ApplyPaidReanimation), so nothing is sent that can be lost, and it does
        /// not matter which client owns the husk. Returns true when the use was handled.
        /// </summary>
        public static bool TryRequestReanimation(Character husk, Humanoid user)
        {
            Player player = user as Player;

            if (player == null || player != Player.m_localPlayer || !apparitions.TryGetValue(husk, out ApparitionController apparition))
                return false;

            if (apparition.Summoner != player.GetPlayerID())
            {
                player.Message(MessageHud.MessageType.Center, "This apparition answers to someone else");
                return true;
            }

            ZDO zdo = player.m_nview != null ? player.m_nview.GetZDO() : null;
            int key = apparition.ReanimateKey;

            if (zdo == null || key == 0)
                return false;

            int done = apparition.ReanimationsDone;

            // Paid for, but the owner of the husk has not done it yet
            if (zdo.GetInt(key, 0) > done)
            {
                player.Message(MessageHud.MessageType.Center, "The apparition is already returning");
                return true;
            }

            // The maximum of the last equipped scythe, for the active apparitions (husks do not count) and the summons on their way. This
            // is checked here, on the client of the summoner and before it pays, like the maximum of a summon. A husk that is not loaded is
            // not counted when a new apparition is summoned, so without this the player could go over the maximum
            Count(player.GetPlayerID(), out int active);
            active += CountPendingSummons() + CountReturningHusks(player);
            int max = GetMaxApparitions();

            if (active + 1 > max)
            {
                player.Message(MessageHud.MessageType.Center, $"You have too many active apparitions to reanimate it ({active}/{max})");
                return true;
            }

            float cost = GetReanimateCost(player);

            // Like the health cost of an attack: it can not take the last of your health
            if (!player.HaveHealth(cost + 0.1f))
            {
                player.Message(MessageHud.MessageType.Center, "You do not have the health to reanimate it");
                return true;
            }

            player.UseHealth(cost);
            zdo.Set(key, done + 1);

            Jotunn.Logger.LogWarning($"[Apparition] Reanimation {done + 1} of '{husk.name}' paid with {cost:F1} health");
            return true;
        }

        /// <summary>
        /// False when the game wants to remove an apparition because its summoner is far away or gone, and the config says apparitions
        /// stay. The config is synced from the server and read at the moment, so every client that owns the apparition decides the same.
        /// The ZDO is read, so an older apparition without a summoner (which is not registered) stays too.
        /// </summary>
        public static bool CanUnsummon(Tameable tameable)
        {
            return PluginConfig.apparitionsDespawn.Value || tameable == null || !HasApparitionData(tameable.m_character);
        }

        /// <summary>
        /// Called on every hit on a character. A hit with damage of its summoner kills a husk, so the summoner can clear their apparitions.
        /// Only the owner of the husk kills it, the death removes it for every client. Returns true when the hit is handled here, the game
        /// then does not handle it. Every other hit is left to the game, which ignores a hit on a husk because it has no health.
        /// To hit a husk at all the summoner needs PvP on or the Butcher knife, like for any tamed creature.
        /// </summary>
        public static bool TryKillHusk(Character character, HitData hit)
        {
            if (!IsHusk(character) || character.m_nview == null || !character.m_nview.IsOwner())
                return false;

            ApparitionController apparition = apparitions[character];

            if (hit.GetAttacker() is not Player summoner || summoner.GetPlayerID() != apparition.Summoner || hit.GetTotalDamage() <= 0f)
                return false;

            // The summoner has paid health for a reanimation that the owner has not done yet: it is returning, and the payment is not lost
            if (GetPaidReanimation(summoner, apparition) > apparition.ReanimationsDone)
                return false;

            Jotunn.Logger.LogWarning($"[Apparition] '{character.name}' is killed by its summoner");
            apparition.Kill();
            return true;
        }

        /// <summary>
        /// The husks of the player that are paid for and wait to return (see ApparitionController.ApplyPaidReanimation). They are still
        /// husks, but they will be active, so a second reanimation in that time can not go over the maximum.
        /// </summary>
        private static int CountReturningHusks(Player player)
        {
            long playerID = player.GetPlayerID();
            int returning = 0;

            foreach (ApparitionController apparition in apparitions.Values)
            {
                if (apparition != null && apparition.Summoner == playerID && apparition.IsHusk
                    && GetPaidReanimation(player, apparition) > apparition.ReanimationsDone)
                    returning++;
            }

            return returning;
        }

        /// <summary>
        /// The hover text of a husk: that it is banished, and for its summoner what it costs to reanimate it. Null for a creature that is
        /// not a husk, it keeps the text of the game.
        /// </summary>
        public static string GetHuskHoverText(Tameable tameable)
        {
            Character character = tameable != null ? tameable.m_character : null;

            if (character is null || !apparitions.TryGetValue(character, out ApparitionController apparition) || !apparition.IsHusk)
                return null;

            string text = tameable.GetHoverName() + " ( Banished )";
            Player player = Player.m_localPlayer;

            if (player != null && apparition.Summoner == player.GetPlayerID())
                text += $"\n[<color=yellow><b>$KEY_Use</b></color>] Reanimate ({Mathf.CeilToInt(GetReanimateCost(player))} health)";

            return Localization.instance.Localize(text);
        }

        /// <summary>
        /// Sets up the spawn abilities of the scythes and registers the apparitions with Core. Call it once, after the prefabs are loaded.
        /// </summary>
        public static void Setup()
        {
            CustomPrefabs prefabs = ModularMagic_BloodMagic.Instance.prefabs;

            foreach (GameObject spawnAbilityPrefab in new[] { prefabs.SpawnAbilityScythe1, prefabs.SpawnAbilityScythe2, prefabs.SpawnAbilityScythe3 })
            {
                SpawnAbility spawnAbility = spawnAbilityPrefab.GetComponent<SpawnAbility>();

                // The apparition spawns exactly where the spawn ability is placed (behind the target), see SoulReaperHelper.SpawnApparition
                spawnAbility.m_targetType = SpawnAbility.TargetType.Position;
                spawnAbility.m_spawnRadius = 0f;

                // The apparition follows its owner in ApparitionController.SetupFollower. The game's command is a toggle, so it would
                // make the apparition stay again
                spawnAbility.m_commandOnSpawn = false;

                // The game counts every loaded creature of the prefab (a wild Charred, the apparitions of another player) against its
                // maximum. The maximum is per summoner and checked before the summon (SoulReaperHelper), so the one of the game is off
                spawnAbility.m_maxSpawned = 0;
            }

            foreach (string creatureName in CreatureNames)
                SpawnedCreatureHelper.Register(creatureName, OnSpawned);
        }

        /// <summary>
        /// Called by Core for every creature a SpawnAbility spawns with one of the apparition prefabs. The staff is the item that was
        /// used for the spawn, only a scythe makes an apparition.
        /// </summary>
        private static void OnSpawned(Character creature, ItemDrop.ItemData staff)
        {
            string? weaponName = staff?.m_dropPrefab != null ? staff.m_dropPrefab.name : null;

            // Only a scythe makes an apparition, so another SpawnAbility that spawns the same creature is left alone
            ApparitionOptions? options = weaponName != null ? GetOptions(weaponName) : null;
            if (weaponName == null || options == null)
                return;

            // The summon has arrived, so it no longer takes a slot as a summon that is on its way
            if (pendingSummons.Count > 0)
                pendingSummons.RemoveAt(0);

            ApparitionController? controller = creature.GetComponent<ApparitionController>();
            if (controller == null)
            {
                Jotunn.Logger.LogError($"ApparitionHelper: '{creature.name}' has no ApparitionController.");
                return;
            }

            // The scythe summons for the local player (SoulReaperHelper.SpawnApparition), who is the owner of the new apparition
            if (Player.m_localPlayer == null)
            {
                Jotunn.Logger.LogError($"ApparitionHelper: '{creature.name}' was summoned without a local player.");
                return;
            }

            controller.SetData(weaponName, Player.m_localPlayer.GetPlayerID());

            float baseline = GetBaselineDamage();
            if (baseline <= 0f)
            {
                Jotunn.Logger.LogWarning("ApparitionHelper: the apparitions have no attack with damage, their damage is not scaled.");
                return;
            }

            // The damage of the strongest attack is the damage of the scythe times the multiplier of the config. The game has already put the
            // Blood Magic factor of the spawn ability on the apparition, so it is multiplied into that
            float scytheDamage = DamageHelper.GetMainPhysical(staff.GetDamage());
            SpawnedCreatureHelper.SetDamage(creature, scytheDamage * options.damageMultiplier, baseline, multiplyExisting: true);

            Jotunn.Logger.LogWarning($"[Apparition] '{creature.name}' from '{weaponName}': scythe damage {scytheDamage:F1} x {options.damageMultiplier} (strongest attack {baseline:F1} at factor 1)");
        }

        /// <summary>
        /// The options of the apparitions of a scythe, or null when the prefab name is not one of the scythes. A scythe is identified by its
        /// prefab name, the display name can be changed in the config.
        /// </summary>
        public static ApparitionOptions? GetOptions(string weaponName)
        {
            CustomPrefabs scythes = ModularMagic_BloodMagic.Instance.prefabs;

            if (weaponName == scythes.Scythe1.name)
                return new ApparitionOptions
                {
                    health           = PluginConfig.scythe1ApparitionHealth.Value,
                    damageMultiplier = PluginConfig.scythe1ApparitionDamageMultiplier.Value,
                    emissionColor    = new Color(0f, 2.666667f, 2.996078f),
                    size             = new Vector3(0.75f, 0.75f, 0.75f)
                };

            if (weaponName == scythes.Scythe2.name)
                return new ApparitionOptions
                {
                    health           = PluginConfig.scythe2ApparitionHealth.Value,
                    damageMultiplier = PluginConfig.scythe2ApparitionDamageMultiplier.Value,
                    emissionColor    = new Color(2.839216f, 0f, 4f),
                    size             = new Vector3(0.875f, 0.875f, 0.875f)
                };

            if (weaponName == scythes.Scythe3.name)
                return new ApparitionOptions
                {
                    health           = PluginConfig.scythe3ApparitionHealth.Value,
                    damageMultiplier = PluginConfig.scythe3ApparitionDamageMultiplier.Value,
                    emissionColor    = new Color(4f, 0.03725543f, 0f),
                    size             = new Vector3(1f, 1f, 1f)
                };

            return null;
        }

        /// <summary>
        /// The damage the hardest hit of the apparitions of a scythe would deal at this quality, before Blood Magic and the random roll of
        /// the game, read from the config of the scythe. It follows OnSpawned: the highest of the slash, pierce and blunt damage of the
        /// scythe at that quality, times the multiplier. Used for the preview in the config, see DamageMultiplierDrawer.
        /// </summary>
        public static float GetPreviewHit(ModularMagic_Core.Configs.StaffConfig scythe, float multiplier, int quality)
        {
            float slash  = GetConfigDamage(scythe.damageSlash, scythe.damageSlashPerLevel, quality);
            float pierce = GetConfigDamage(scythe.damagePierce, scythe.damagePiercePerLevel, quality);
            float blunt  = GetConfigDamage(scythe.damageBlunt, scythe.damageBluntPerLevel, quality);

            return Mathf.Max(slash, Mathf.Max(pierce, blunt)) * multiplier;
        }

        // A staff config only has the damage entries its staff uses, the others are null
        private static float GetConfigDamage(BepInEx.Configuration.ConfigEntry<float> damage, BepInEx.Configuration.ConfigEntry<float> perLevel, int quality)
        {
            float value = damage != null ? damage.Value : 0f;

            if (perLevel != null)
                value += perLevel.Value * (quality - 1);

            return value;
        }

        /// <summary>
        /// The damage of the strongest attack of all apparitions at factor 1 (the total damage of the strongest weapon of the three
        /// apparition prefabs). That attack deals the damage of the scythe times the multiplier, every other attack deals the same fraction
        /// of its own damage, so a weaker attack (like a throw) stays weaker than a melee hit and the apparitions stay balanced against each other.
        /// </summary>
        private static float GetBaselineDamage()
        {
            if (baselineDamage >= 0f)
                return baselineDamage;

            float strongest = 0f;

            foreach (string creatureName in CreatureNames)
            {
                GameObject prefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab(creatureName);
                Humanoid? humanoid = prefab != null ? prefab.GetComponent<Humanoid>() : null;

                if (humanoid == null)
                {
                    Jotunn.Logger.LogWarning($"ApparitionHelper: '{creatureName}' has no Humanoid.");
                    continue;
                }

                List<ItemDrop> items = new List<ItemDrop>();

                foreach (GameObject item in humanoid.m_defaultItems)
                    AddItem(items, item);

                foreach (GameObject item in humanoid.m_randomWeapon)
                    AddItem(items, item);

                foreach (Humanoid.RandomItem randomItem in humanoid.m_randomItems)
                    AddItem(items, randomItem.m_prefab);

                foreach (ItemDrop item in items)
                    strongest = Mathf.Max(strongest, GetAttackDamage(creatureName, item, items));
            }

            Jotunn.Logger.LogWarning($"[Apparition] Strongest attack of the apparitions: {strongest:F1}");
            // A baseline of 0 is not kept, so the next spawn looks again
            if (strongest > 0f)
                baselineDamage = strongest;

            return strongest;
        }

        private static void AddItem(List<ItemDrop> items, GameObject item)
        {
            ItemDrop? itemDrop = item != null ? item.GetComponent<ItemDrop>() : null;

            if (itemDrop != null)
                items.Add(itemDrop);
        }

        private static bool IsAmmo(ItemDrop.ItemData.SharedData shared)
        {
            return shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo || shared.m_itemType == ItemDrop.ItemData.ItemType.AmmoNonEquipable;
        }

        /// <summary>
        /// The damage of one hit of a weapon at factor 1. A weapon that uses ammo (a bow) adds the damage of its ammo to every hit, like the
        /// game does, so the strongest ammo of the creature with the same ammo type is added. Ammo alone is not an attack.
        /// </summary>
        private static float GetAttackDamage(string creatureName, ItemDrop item, List<ItemDrop> items)
        {
            ItemDrop.ItemData.SharedData shared = item.m_itemData.m_shared;

            if (IsAmmo(shared))
                return 0f;

            float damage = shared.m_damages.GetTotalDamage();

            if (!string.IsNullOrEmpty(shared.m_ammoType))
            {
                float ammoDamage = 0f;

                foreach (ItemDrop other in items)
                {
                    ItemDrop.ItemData.SharedData otherShared = other.m_itemData.m_shared;

                    if (IsAmmo(otherShared) && otherShared.m_ammoType == shared.m_ammoType)
                        ammoDamage = Mathf.Max(ammoDamage, otherShared.m_damages.GetTotalDamage());
                }

                damage += ammoDamage;
            }

            if (damage > 0f)
                Jotunn.Logger.LogWarning($"[Apparition] {creatureName}: '{item.name}' deals {damage:F1}");

            return damage;
        }
    }
}
