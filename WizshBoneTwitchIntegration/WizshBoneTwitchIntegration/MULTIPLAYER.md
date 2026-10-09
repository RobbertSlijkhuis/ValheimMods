# Multiplayer - findings and open questions

Written 2026-10-09. Covers how creature config reaches other clients, the new monster attack/AI range
scaling, and a proposed ZDO snapshot. Profile **Sync** itself is covered in `SYNC_ISSUES.md`.
Nothing here has been tested with a second client yet. "Verified" below means read in code or in the
decompiled game assembly, not tested in-game.

> **Linked to profile Sync:** if spawned objects carry their own config (see the ZDO snapshot idea below),
> Sync may shrink or change entirely, so the Sync work in `SYNC_ISSUES.md` (accept/decline dialog etc.) is on
> hold until this direction is decided.

## How creature config reaches other clients today

- `TwitchCreaturePersistentData` writes one pipe-delimited string to the ZDO (`CreatureData_WBTI`):
  `redeemer|redeemTitle|savedPrefab|ignoreWard|isFollowing|actualPrefab`. It is positional; optional trailing
  parts are guarded with `data.Length > n`.
- Every client that loads the creature runs `Awake()`, looks the redeem up **in its own loaded profile**
  (`RedeemHelper.GetRedeemByTitle` -> `FindRedeemCreatureData`) and applies the result (`ApplyCreatureData`).
  The actual `CreatureData` (size, colour, scales, gear, ...) is **not** in the ZDO.
- If the client has no matching redeem, `Awake()` logs `Could not find redeem ...` / `Could not find creature ...`
  and returns. The creature stays vanilla on that client (normal size, no recolour, no gear changes).

## Monster attack / AI range scaling (added 2026-10-09)

- `helpers/AttackScaleHelper.cs` + `harmony/EntityScalePatchesWBTI.cs` multiply a resized Twitch creature's
  attack geometry by its root `localScale.x` (set from `CreatureData.size`):
  - `Attack.DoMeleeAttack` and `Attack.DoAreaAttack`: range, height, offset, ray widths (temporary, restored in a
    finalizer).
  - `Attack.GetProjectileSpawnPoint`: height, range, offset.
  - `MonsterAI.UpdateAI`: every inventory weapon's `m_aiAttackRange` / `m_aiAttackRangeMin`, so a shrunk monster
    does not start swinging from outside its reach (restored in a finalizer, de-duplicated by `SharedData`).
- The local player uses `PlayerScaleHelper.CurrentScale`; other players are ignored (their attacks are never
  simulated on this client).
- Level effects are **not** included: `LevelEffects` scales a child transform, not the root, so a levelled
  monster keeps vanilla reach. Intended, matches vanilla.
- **Multiplayer behaviour (reasoned, not tested):** only the creature's owner runs `UpdateAI` / `Attack`, so only
  the owner's scale matters. Nothing is written to a ZDO and no RPCs are used. It is correct on a client only
  if that client applied the creature's size in `Awake()`, which depends on the issue below.

## Open issue: creature config depends on the client having the same redeem

| Case | Result |
|------|--------|
| Every client has the same profile (e.g. Sync was pressed) | Size, colour, gear and attack reach are correct everywhere, including after ownership changes. |
| A client lacks the redeem (joined after Sync, missed/declined it, different profile) | That client sees the creature at normal size and unmodified. If it **becomes the owner**, the creature also fights with unscaled reach/AI range. |

- Profile Sync (`SYNC_ISSUES.md`) narrows this but does not remove it: it only reaches players connected when
  Sync is pressed, and a profile can be edited afterwards.
- A mismatch in a redeem's *contents* (same title, different values) is silent: no error, wrong values.

## Idea: snapshot the creature's runtime config into the ZDO

Goal: a creature carries what it needs, so no client depends on its local profile.

### Why the old attempt was dropped (commit `de417fed`, 2026-04-20)

- The old `CreatureDataToString` / `StringToCreatureData` wrote ~18 `CreatureData` fields into one positional
  string and parsed them back by index with `int.Parse` / `bool.Parse`. Adding, removing or reordering a field
  made every previously saved creature throw on load.
- The schema has kept moving: `index`, `isHallucination`, `random` are gone; `aggravatable` was removed just
  before this note; `size`, `color`, `emissionColor`, `forceColor`, `speedMultiplier`, `idleSoundInterval`,
  `isBoss`, `bossEvent`, `fullyPassive`, `alwaysFollowOwner`, `equipItems`, `removeEquipment` were added.
- The old version never applied `size` in `Awake()` (only in `SetData`), so other clients never saw the size.

### Proposed approach

- **One hashed ZDO key per field** (e.g. `"WBTI_Creature_size".GetStableHashCode()`), read with a default, instead
  of one positional string. A missing key reads as the default, so adding a field never invalidates old
  creatures. This is also the pattern already used by the other networked components.
- Write once from `SetData` / `SetInherited` on the spawning client (one-time config, safe to write
  unconditionally). Read in `Awake()` on every client.
- Keep the profile lookup as a **fallback** when the ZDO has no snapshot (creatures spawned before the change).
- Read inside try/catch with per-field defaults; a bad value logs a warning instead of aborting the creature.
- Move `ignoreWard` and `isFollowing` out of the positional string onto their own keys at the same time.

### Candidate fields (everything a client applies when it loads a creature)

| Group | Fields |
|-------|--------|
| Look | `size`, `color`, `emissionColor`, `forceColor` |
| Stats / behaviour | `level`, `isBoss`, `bossEvent`, `maxHealth`, `speedMultiplier`, `mistVision`, `idleSoundInterval`, `damageScale`, `healthScale`, `allowDamageStructures`, `allowDrops` |
| Identity | `rename`, `name`, `group`, `friendly` |
| Tame / follow | `commandable`, `alwaysFollowOwner`, `fullyPassive` |
| Talking (`TwitchCreatureClaim`) | `talks`, `talkInteract`, `talkInterval`, `talkMessage` |
| Gear (`CreatureGearHelper`) | `equipItems`, `removeEquipment` (store each list as one joined string) |

Spawn-time only, not worth snapshotting: `announceMessage`, `amount`, `globalKeyAdd`, `globalKeyRemove`,
`maxSpawned`, `position`, `positionOffset`, `positionRadius`, `facePlayer`. `prefabName` is already stored.

### Profile *settings* are read per client too

Creature setup also reads profile settings on every client, not just the redeem: `ApplyHumanoid` uses
`creaturesSameFaction`, `creaturesScaling` and the health-scale override via `ProfileSettingsHelper.Current`,
and the damage patch uses `creaturesDamageScale`. Tier-based scaling goes through
`ProgressionHelper.GetPlayerTier()` (not checked whether that depends on the local player). A snapshot would
need the *resolved* values of these at spawn time as well, otherwise clients on different profiles still
disagree on faction and scaling. Only creatures were surveyed; other redeem types were not.

### Decisions still open

- Whole table, or a first batch (Look + Stats / behaviour)?
- Gear lists: snapshot them, or leave on the profile lookup?
- `talkMessage` can be long. Max practical ZDO string size not checked.
- **Behaviour change:** today a reload re-applies the *current* profile, so editing a redeem retroactively
  changes creatures that are already spawned. A snapshot would keep their spawn-time settings.
- Per-creature ZDO size is sent to every peer in range; fine for scalars, worth keeping an eye on with long strings.

## Weather zone: config now networked (2026-10-09, untested)

Found and fixed 2026-10-09 by reading code. **Not tested in-game with a second client.**

- **Was:** `WeatherHelper.SpawnWeather` set `EnvZone.m_environment`, `EnvZone.m_force` and the capsule
  `height` / `radius` locally after `Instantiate` and wrote none of it to the ZDO. Only `TwitchPersistentDestruction`
  (duration / start time) synced. Other clients, and the owner after a zone reload, had the prefab defaults
  (not checked, they are in the asset bundle), so other players probably never saw the weather.
- **Now:** `components/TwitchWeatherZone.cs` (on the `EnvZone` prefab), same pattern as `TwitchTimeStopZone`:
  - `Initialize(weathers, interval, force, height, radius, followTarget)` on the spawning client writes everything
    plus a `Configured` flag to the ZDO (hashed keys). `Awake()` on every copy reads it back and applies it to
    `EnvZone` and the capsule. A single weather is a one-entry list.
  - **Cycle:** with more than one weather, `Update()` picks the current one from the shared world clock
    (`ZNet.GetTimeSeconds()` minus the stored start time), so clients agree without per-switch sync and a reload or
    late joiner lands on the right weather.
  - **Follow player:** the redeemer's player ZDOID is stored. Every client moves its own copy of the zone onto that
    player in `LateUpdate()` (a transform is not synced), and the owner also calls `zdo.SetPosition()` so other peers
    keep receiving the zone. If the target isn't loaded on a client, or is destroyed (death, logout), the zone stays
    where it was.
  - `OnDestroy()` clears a forced environment on every client; before, only the spawning client did (`onEnd`).
- **Behaviour change:** other players inside the zone now see the weather. They must be within the zone's radius,
  and the zone must be loaded on their client.
- **Known edge:** `OnDestroy` also runs when a zone merely unloads (player walks away), which would clear a forced
  environment from a different zone the player is in. Same as the old `onEnd`.
- **Related, unfixed:** `WeatherHelper.OnDestroy` nulls `_activeWeatherZone` even when a newer zone has replaced
  the one being destroyed.
- **To test:** second client stands in a fixed zone (sees the weather?), a cycling zone (same weather as the
  streamer?), and next to a following streamer who walks away (zone and weather follow?). Relog inside a zone.

## Not checked yet

- **Does the scaled player size show on other clients?** `PlayerScaleHelper` sets `localScale` on
  `Player.m_localPlayer` and writes nothing to a ZDO; whether remote players see the change was not
  investigated.
- Whether `speedMultiplier` (applied with `*=` on `m_speed` / `m_runSpeed`) can stack if
  `ApplyVariables` runs more than once on one instance.
- Whether Valheim's RPC layer accepts a 2 MB profile package (also listed in `SYNC_ISSUES.md`, issue 6).

## Test checklist (two clients)

1. Both clients on the same synced profile. Spawn a size 0.5 and a size 2 monster. Both clients should see the
   scaled monster.
2. Stand next to the monster on client B, then move client A away so ownership moves to B. Check the monster
   still swings at its scaled reach: a size 0.5 monster should only swing when the target is close and should hit;
   a size 2 monster should swing from further out and hit.
3. Repeat with client B on a profile that lacks the redeem. Expect: normal-sized monster on B, and wrong reach if
   B owns it (documents the open issue).
4. Area attacks (e.g. a stomping monster) at size 0.5 / 2.
5. Ranged attackers: projectile spawn point is sensible at both sizes.
