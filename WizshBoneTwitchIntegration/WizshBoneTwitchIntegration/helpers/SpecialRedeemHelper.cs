using System;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Exceptions;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Temporary hack: hardcoded redeems that aren't in the YAML redeem list.
    internal static class SpecialRedeemHelper
    {
        // Special surprises for Saphonette's stream.
        private static readonly string[] AllowedUsers = { "nikontheline", "GraziFM", "DeathWizsh", "Naffiy", "Naarke", "Alenea", "Whurtlefak", "soma_af" };
        private static readonly string[] BathtubTriggerWords = { "hottub", "hot tub", "hot-tub" };
        private const string BathtubRedeemTitle = "WBTI: Saphonette bathtub";

        // soma_af's Somasquito - scoped separately from AllowedUsers/BathtubTriggerWords since
        // only soma_af (and the DeathWizsh test alias) should be able to trigger it, not everyone
        // in the bathtub allowlist.
        private static readonly string[] SomaUsers = { "soma_af", "DeathWizsh" };
        private static readonly string[] SomaTriggerWords = { "hi", "hey", "hello" };
        private const string SomaRedeemTitle = "WBTI: soma_af special";

        // SAPHONETTE-CLEANUP: runtime-only toggles for the redeems/effects most likely to need
        // shutting off mid-stream. Reset to enabled on every launch - DeathWizsh flips these (and
        // DeathSoundHelper.Enabled, ShieldSoundHelper.Enabled) via the "!toggle" chat command (see
        // TryHandleToggleCommand) so they can be shut off without a rebuild.
        public static bool CoffeeRedeemEnabled = true;
        public static bool BlobCircleRedeemEnabled = true;
        public static bool BathtubRedeemEnabled = true;
        public static bool SomasquitoRedeemEnabled = true;
        public static bool LogRainOverrideEnabled = true;

        // DeathWizsh's numeric Twitch user ID (not the login name, which can be reused by someone
        // else if the account ever renames away from it). Find it by typing anything in chat and
        // reading the "(id=...)" printed alongside your name in the BepInEx console (TwitchChat.Read).
        private const string DeathWizshUserId = "46594581";

        // TEMPORARY - Saphonette's stream: Log Rain only rains full Yggwood logs that talk. Delete
        // ApplyLogRainOverride (and its call site in SpawnAbilityHelper.SpawnAbility) once the bit
        // is over.
        private const string YggwoodLogPrefab = "yggashoot_log";
        private const string YggwoodLogTalkMessage = "Its me-uh... K.A.R.L.!;K.A.R.L. to the rescue!;Besides being handsome, I'm also long and hard!;Did you know K.A.R.L. stands for \"Kinetic Action Relationship Log\"?";
        // Shown on-screen when the override fires - without it, unexpectedly talking logs just look
        // like a bug instead of an intentional bit. Placeholder - easy to edit.
        private const string YggwoodLogAnnounceMessage = "K.A.R.L. has jumped into the fray {{user}}!";

        // The "Don't forget your coffee" reward already exists on Twitch, created outside the mod -
        // title must match exactly.
        private const string CoffeeRedeemTitle = "Don't forget your coffee";

        // SAPHONETTE-CLEANUP: "!killcoffee" despawn radius - generous since the draugr always closes
        // distance to Player.m_localPlayer on its own (alwaysFollowOwner), so it should never actually
        // be far away by the time anyone bothers to kill it. Remove alongside the rest of the coffee bit.
        private const float KillCoffeeRadius = 50f;

        // "Oh no, oh no no no" reward already exists on Twitch, created outside the mod - title
        // must match exactly. Spawns a ring of talking Blobs around the player and forces Bonemass
        // weather over the same area.
        private const string BlobCircleRedeemTitle = "Oh no, oh no no no";
        private const int BlobCircleCount = 5;
        private const float BlobCircleRadius = 10f;
        private const string BlobTalkMessage = "A foul smell from Nik on the line!;Bwaaawaugh! Nik sends his regards!;We ooze, therefore we are - blame Nik!;Something's rotten in the swamp tonight, its Nik!;The bog is closing in... courtesy of Nik!;Uuhuh!";
        private const float BlobTalkInterval = 10f;
        private const int BlobWeatherDuration = 180;
        private const float BlobWeatherRadius = 150f;
        private const float BlobWeatherHeight = 50f;
        private const string BlobWeatherAnnounceMessage = "The bog rises around Saphonette... Nik stirs!";

        // HandleRedeem re-looks-up the redeem by title after TryGetChatWordRedeem already matched
        // it, so the synthesized chat-word title must resolve here too.
        public static RedeemData TryGetRedeem(string title)
        {
            if (title == BathtubRedeemTitle)
                return BathtubRedeemEnabled ? BuildBathtubRedeem() : null;

            if (title == SomaRedeemTitle)
                return SomasquitoRedeemEnabled ? BuildSomaRedeem() : null;

            if (title == CoffeeRedeemTitle)
                return CoffeeRedeemEnabled ? BuildCoffeeRedeem() : null;

            // BlobCircle isn't a single-effect RedeemData (it spawns creatures *and* forces
            // weather) - this sentinel just gets it past HandleRedeem's shared guard clauses
            // (banned user/safe zone/paused/etc.) and into redeem history. The actual effect runs
            // via TryHandleSpecialEffect, called separately from HandleRedeem.
            if (title == BlobCircleRedeemTitle)
                return BlobCircleRedeemEnabled ? new RedeemData { title = BlobCircleRedeemTitle, type = RedeemType.Undefined } : null;

            return null;
        }

        // nikontheline/GraziFM/DeathWizsh saying "hottub"/"hot tub"/"hot-tub" in chat spawns a
        // bathtub. soma_af/DeathWizsh saying "hi"/"hey"/"hello" in chat spawns a Somasquito.
        public static RedeemData TryGetChatWordRedeem(string userName, string message)
        {
            if (userName == null || message == null)
                return null;

            if (BathtubRedeemEnabled && MatchesAny(userName, AllowedUsers) && ContainsAny(message, BathtubTriggerWords))
            {
                Jotunn.Logger.LogWarning($"[WBTI] SpecialRedeemHelper: hottub trigger word validated in message from {userName}");
                return BuildBathtubRedeem();
            }

            if (SomasquitoRedeemEnabled && MatchesAny(userName, SomaUsers) && ContainsAny(message, SomaTriggerWords))
            {
                Jotunn.Logger.LogWarning($"[WBTI] SpecialRedeemHelper: soma trigger word validated in message from {userName}");
                return BuildSomaRedeem();
            }

            return null;
        }

        private static bool MatchesAny(string userName, string[] users)
        {
            foreach (string user in users)
            {
                if (userName.Equals(user, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool ContainsAny(string message, string[] words)
        {
            foreach (string word in words)
            {
                if (message.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static RedeemData BuildBathtubRedeem()
        {
            return new RedeemData
            {
                title = BathtubRedeemTitle,
                type = RedeemType.SpawnCreature,
                creatureData = new SpawnCreatureData
                {
                    list = new List<CreatureData>
                    {
                        new CreatureData
                        {
                            prefabName = "piece_bathtub",
                            position = SpawnPositionType.InFrontOfPlayer,
                            requireMonsterComponents = false,
                            smelterFuelAmount = float.MaxValue,
                            talkMessage = "Come join me in the hot tub Saphonette! I'm moist :);Is it hot in here? Or is that just Saphonette!;Oh my... its getting hot in here!;Who needs K.A.R.L. when you have a hot tub?!",
                            talkInterval = 5,
                            talks = true,
                        }
                    }
                }
            };
        }

        private static RedeemData BuildSomaRedeem()
        {
            return new RedeemData
            {
                title = SomaRedeemTitle,
                type = RedeemType.SpawnCreature,
                creatureData = new SpawnCreatureData
                {
                    list = new List<CreatureData>
                    {
                        new CreatureData
                        {
                            prefabName = "Deathsquito",
                            name = "Somasquito",
                            rename = true,
                            size = 1.5f,
                            color = "#FFBBDF",
                            forceColor = true,
                            talkMessage = "I'm gonna sting ya Saphonette!;Aint I... stunging?;Ready or not! Here I sting!;SaphonetteAboutToBeStung!",
                            talkInterval = 5,
                            talks = true,
                        }
                    }
                }
            };
        }

        private static RedeemData BuildCoffeeRedeem()
        {
            return new RedeemData
            {
                title = CoffeeRedeemTitle,
                type = RedeemType.SpawnCreature,
                creatureData = new SpawnCreatureData
                {
                    list = new List<CreatureData>
                    {
                        new CreatureData
                        {
                            prefabName = "Draugr",
                            size = 0.5f,
                            friendly = true,
                            commandable = false,
                            alwaysFollowOwner = true,
                            fullyPassive = true,
                            talkMessage = "Don't forget your coffee, Saphonette!;Coffee's getting cold, don't forget it!;Hey Saphonette, coffee time - don't forget!;Did you seriously microwaved the milk? ICK!",
                            talkInterval = 10,
                            talks = true,
                            speedMultiplier = 4f,
                            name = "Coffee Reminder",
                            rename = true,
                            helmetItem = "HelmetCelebration",
                            heldItem = "Tankard_dvergr",
                            removeShield = true,
                            color = "#ff8fdd",
                            forceColor = true,
                        }
                    }
                }
            };
        }

        // SAPHONETTE-CLEANUP: DeathWizsh-only "!killcoffee" chat command - despawns any currently-active
        // coffee draugr(s) near the local player on demand. The draugr is fullyPassive (never fights
        // back, see SpecialRedeemPatchesWBTI.IsEnemy_Prefix) and always closing distance at 4x speed, so
        // fighting one down manually is awkward - this gives an instant way to get rid of it instead.
        // Identifies a coffee draugr by TwitchCreaturePersistentData.RedeemTitle rather than prefab name
        // alone, so it never sweeps up an unrelated redeem-spawned Draugr. Remove alongside the rest of
        // the coffee bit.
        public static void TryHandleKillCoffeeCommand(TwitchChat chat, string userId)
        {
            if (userId == null || userId != DeathWizshUserId)
                return;

            if (Player.m_localPlayer == null)
                return;

            Collider[] objects = Physics.OverlapSphere(Player.m_localPlayer.transform.position, KillCoffeeRadius, LayerMask.GetMask("character"));
            int killed = 0;

            foreach (Collider obj in objects)
            {
                GameObject gameObject = obj.transform.root.gameObject;
                TwitchCreaturePersistentData persistentData = gameObject.GetComponent<TwitchCreaturePersistentData>();

                if (persistentData == null || persistentData.RedeemTitle != CoffeeRedeemTitle)
                    continue;

                ZNetViewHelper.Destroy(gameObject);
                killed++;
            }

            chat.Send(killed > 0
                ? $"Put down {killed} coffee draugr{(killed == 1 ? "" : "s")}."
                : "No coffee draugr found nearby.");

            Jotunn.Logger.LogWarning($"[WBTI] SpecialRedeemHelper: !killcoffee removed {killed} coffee draugr(s)");
        }

        // SAPHONETTE-CLEANUP: "!nik" chat command - open to anyone, no allowlist/admin gate, and no
        // cooldown (every message plays it again). Bypasses the normal RedeemData/HandleRedeem
        // pipeline entirely since it's just a local sound effect, not a game-world redeem - no chat
        // confirmation either, the sound itself is the feedback. Remove alongside the rest of the
        // death sound bit.
        public static void TryHandleNikCommand(string message)
        {
            if (message == null || !message.StartsWith("!nik", StringComparison.OrdinalIgnoreCase))
                return;

            DeathSoundHelper.PlayForNikCommand();
        }

        // Dispatched from TwitchCustomRewards.HandleRedeem separately from the normal per-type
        // dispatch, since BlobCircle needs two effects (creature spawn ring + weather) out of one
        // redemption rather than the single helper each RedeemType normally maps to.
        public static void TryHandleSpecialEffect(RedeemData redeem, CustomRewardEvent customRewardEvent, TwitchChat chat)
        {
            if (redeem.title == BlobCircleRedeemTitle)
                HandleBlobCircleRedeem(customRewardEvent, chat);
        }

        private static void HandleBlobCircleRedeem(CustomRewardEvent customRewardEvent, TwitchChat chat)
        {
            if (Player.m_localPlayer.InInterior())
            {
                chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the streamer is inside a dungeon and this redeem is not allowed in dungeons! {(ProfileSettingsHelper.Current.autoResolveRedeems ? TwitchCustomRewards.m_refundAutoResolveOn : TwitchCustomRewards.m_refundAutoResolveOff)}");
                throw new RedeemException("Player is in dungeon and redeem is not allowed", ExceptionType.Warning);
            }

            if (CreatureHelper.GetNrOfTwitchInstances(ProfileSettingsHelper.Current.creaturesMaxRadius) >= ProfileSettingsHelper.Current.creaturesMaxAmount)
            {
                chat.Send($"Sorry @{customRewardEvent.RedeemerName}, the maximum spawned creature limit has been reached! {(ProfileSettingsHelper.Current.autoResolveRedeems ? TwitchCustomRewards.m_refundAutoResolveOn : TwitchCustomRewards.m_refundAutoResolveOff)}");
                throw new RedeemException("To many spawned creatures", ExceptionType.Warning);
            }

            Vector3 center = Player.m_localPlayer.transform.position;

            for (int i = 0; i < BlobCircleCount; i++)
            {
                float angle = i * (Mathf.PI * 2f / BlobCircleCount);
                Vector3 point = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * BlobCircleRadius;

                CreatureData blobData = new CreatureData
                {
                    prefabName = "Blob",
                    position = SpawnPositionType.WorldPosition,
                    positionOffset = new PositionOffsetData { x = point.x, y = point.y, z = point.z },
                    // Blob may not have a Humanoid component (touch-damage, no equipped weapon) -
                    // skip the requireMonsterComponents validation rather than risk it being
                    // destroyed+thrown on spawn. See CreatureHelper.SpawnCreature.
                    requireMonsterComponents = false,
                    talks = true,
                    talkMessage = BlobTalkMessage,
                    talkInterval = BlobTalkInterval,
                };

                CreatureHelper.SpawnCreature(blobData, Player.m_localPlayer.transform, customRewardEvent);
            }

            WeatherHelper.SpawnWeather(new WeatherData
            {
                items = new List<string> { "Bonemass" },
                force = true,
                duration = BlobWeatherDuration,
                radius = BlobWeatherRadius,
                height = BlobWeatherHeight,
                announceMessage = BlobWeatherAnnounceMessage,
            }, customRewardEvent);
        }

        // Only the prefab and talk behavior are overridden - min/max/radius/delay/etc. are left as
        // whatever the real configured Log Rain redeem has.
        public static void ApplyLogRainOverride(SpawnAbilityData spawnAbilityData)
        {
            spawnAbilityData.spawns = new List<string> { YggwoodLogPrefab };
            spawnAbilityData.isBiomeList = false;
            spawnAbilityData.talks = true;
            spawnAbilityData.talkInterval = 10;
            spawnAbilityData.talkMessage = YggwoodLogTalkMessage;
            spawnAbilityData.announceMessage = YggwoodLogAnnounceMessage;
        }

        // SAPHONETTE-CLEANUP: only meaningful when creatureData.talks was set by ApplyLogRainOverride
        // - inert otherwise. Called from SpawnAbilityExtension.Spawn2 for non-creature spawns (e.g.
        // rained logs), which normally never get a TwitchCreatureClaim at all.
        public static void TryAttachTalk(GameObject gameObject, CustomRewardEvent customRewardEvent, CreatureData creatureData)
        {
            if (!creatureData.talks || creatureData.talkMessage == null)
                return;

            TwitchCreatureClaim talkingClaim = gameObject.GetComponent<TwitchCreatureClaim>() ?? gameObject.AddComponent<TwitchCreatureClaim>();
            talkingClaim.ReInit(customRewardEvent.RedeemerName, isSpawn: true, creatureData);
        }

        // SAPHONETTE-CLEANUP: carries the 3 talk fields ApplyLogRainOverride sets on a
        // SpawnAbilityData into the CreatureData built for Spawn2 - collapse back to
        // `new CreatureData()` at the call site once the bit is over.
        public static CreatureData BuildTalkCreatureData(SpawnAbilityData spawnAbilityData) => new CreatureData
        {
            talks = spawnAbilityData.talks,
            talkMessage = spawnAbilityData.talkMessage,
            talkInterval = spawnAbilityData.talkInterval,
        };

        // Checks the chat word triggers and, on a match, dispatches it through the normal
        // HandleRedeem pipeline via a synthesized CustomRewardEvent (same pattern as WBTIUseRedeem).
        public static void TryHandleChatWord(TwitchCustomRewards customRewards, string userName, string message)
        {
            RedeemData redeem = TryGetChatWordRedeem(userName, message);
            if (redeem == null || customRewards == null)
                return;

            CustomRewardEvent chatWordEvent = new CustomRewardEvent();
            chatWordEvent.RedemptionId = Guid.Empty.ToString();
            chatWordEvent.RedeemerName = userName;
            chatWordEvent.RedeemedAt = DateTime.Now.ToShortDateString();
            chatWordEvent.CustomRewardTitle = redeem.title;
            chatWordEvent.CustomRewardCost = 0;
            chatWordEvent.Status = CustomRewardRedemptionState.Unfulfilled;

            customRewards.HandleRedeem(chatWordEvent);
        }

        // SAPHONETTE-CLEANUP: DeathWizsh-only "!toggle <coffee|blob|bathtub|status>" chat command to
        // shut off individual special redeems mid-stream without a rebuild. Remove along with the
        // dispatch in TwitchChat.Read() and the enable flags above once the bit is over.
        public static void TryHandleToggleCommand(TwitchChat chat, string userId, string message)
        {
            if (userId == null || userId != DeathWizshUserId)
                return;

            string arg = message.Length > "!toggle".Length
                ? message.Substring("!toggle".Length).Trim().ToLowerInvariant()
                : "";

            switch (arg)
            {
                case "coffee":
                case "draugr":
                    CoffeeRedeemEnabled = !CoffeeRedeemEnabled;
                    chat.Send($"Coffee redeem is now {(CoffeeRedeemEnabled ? "ENABLED" : "DISABLED")}.");
                    break;
                case "blob":
                case "blobs":
                    BlobCircleRedeemEnabled = !BlobCircleRedeemEnabled;
                    chat.Send($"Blob circle redeem is now {(BlobCircleRedeemEnabled ? "ENABLED" : "DISABLED")}.");
                    break;
                case "bathtub":
                case "tub":
                    BathtubRedeemEnabled = !BathtubRedeemEnabled;
                    chat.Send($"Bathtub redeem is now {(BathtubRedeemEnabled ? "ENABLED" : "DISABLED")}.");
                    break;
                case "soma":
                case "somasquito":
                    SomasquitoRedeemEnabled = !SomasquitoRedeemEnabled;
                    chat.Send($"Somasquito redeem is now {(SomasquitoRedeemEnabled ? "ENABLED" : "DISABLED")}.");
                    break;
                case "deathsound":
                case "death":
                    DeathSoundHelper.Enabled = !DeathSoundHelper.Enabled;
                    chat.Send($"Death sound is now {(DeathSoundHelper.Enabled ? "ENABLED" : "DISABLED")}.");
                    break;
                case "nik":
                    DeathSoundHelper.NikCommandEnabled = !DeathSoundHelper.NikCommandEnabled;
                    chat.Send($"!nik sound is now {(DeathSoundHelper.NikCommandEnabled ? "ENABLED" : "DISABLED")}.");
                    break;
                case "shield":
                    ShieldSoundHelper.Enabled = !ShieldSoundHelper.Enabled;
                    chat.Send($"Shield sound is now {(ShieldSoundHelper.Enabled ? "ENABLED" : "DISABLED")}.");
                    break;
                case "karl":
                case "lograin":
                case "logs":
                    LogRainOverrideEnabled = !LogRainOverrideEnabled;
                    chat.Send($"K.A.R.L. log rain override is now {(LogRainOverrideEnabled ? "ENABLED" : "DISABLED")}.");
                    break;
                case "status":
                case "":
                    chat.Send($"coffee={(CoffeeRedeemEnabled ? "ON" : "OFF")}, blob={(BlobCircleRedeemEnabled ? "ON" : "OFF")}, bathtub={(BathtubRedeemEnabled ? "ON" : "OFF")}, somasquito={(SomasquitoRedeemEnabled ? "ON" : "OFF")}, deathsound={(DeathSoundHelper.Enabled ? "ON" : "OFF")}, nik={(DeathSoundHelper.NikCommandEnabled ? "ON" : "OFF")}, shield={(ShieldSoundHelper.Enabled ? "ON" : "OFF")}, karl={(LogRainOverrideEnabled ? "ON" : "OFF")}");
                    break;
                default:
                    chat.Send("Usage: !toggle <coffee|blob|bathtub|somasquito|deathsound|nik|shield|karl|status>");
                    break;
            }

            Jotunn.Logger.LogWarning($"[WBTI] SpecialRedeemHelper: !toggle {arg} handled for DeathWizsh");
        }
    }
}
