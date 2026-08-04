# Performance TODO

Outliers found during the 2026-07-13 performance pass (follow-up to the Spawn2/SafeZone cleanup:
`55f0c439`, `5e0ffec4`, `df2fe920`). Not yet fixed — tackle top to bottom, highest impact first.

## 1. `harmony/HudPatchesWBTI.cs:69-132` — `UpdateHuds_Postfix`
Runs every frame, unconditionally (patches `EnemyHud.UpdateHuds`, called every frame any enemy is
on screen) — the only item here that's always-on regardless of whether a redeem is active. Does 2
`Transform.Find` string lookups up front, then up to 9 more `Transform.Find` calls per visible
enemy hud, every frame. With several nearby enemies (common after any bulk creature-spawn redeem)
that's 50-200+ string-walked hierarchy searches/frame.
**Gotcha:** `EnemyHud.HudData` is a vanilla type we don't own — can't just add a cache field to it.
**Fix idea:** keep our own `Dictionary<Character, CachedHudLevels>` (or a small marker component
added to `hudLevel3.parent` the first time it's seen, storing the 9 `level_custom_N` refs) so the
`Transform.Find` chain only runs once per hud instance instead of every frame. If using a
dictionary, prune entries no longer present in `__instance.m_huds` each pass (or on
`Character`/hud destroy) so it doesn't leak; a marker component self-cleans via `Destroy` instead.

## 2. `components/TwitchShieldDomeEffect.cs:192,197` — `OnRenderImage`
`m_domes.Values.GroupBy(d => d.color).ToList()` + a per-group `.ToArray()`, every rendered frame
while any dome is up (Ward or active TimeStop zone). Per-frame LINQ allocation/GC pressure.
**Key insight:** a *breaking* dome's radius shrinks every frame (`AdvanceBreakingDomes`), but its
`color` key doesn't change during that animation — so the grouping itself only needs to change on
`SetDome`/`RemoveDome`/`BreakDome`-finished (i.e. when `m_domes`' keys or colors actually change),
not every single frame just because a radius is animating.
**Fix idea:** cache the `Dictionary<Color, List<DomeEntry>>` grouping and only rebuild it from
`SetDome`/`RemoveDome`/the entry-removal path inside `AdvanceBreakingDomes`; every other frame,
reuse the cached grouping and just re-`UploadBuffer` the (possibly radius-updated) per-dome data.

## 3. `harmony/TimeStopPatchesWBTI.cs:73-103` — 4 prefixes on `Ship`/`Floating`/`ZSyncTransform`
`GetComponentInParent<TwitchPhysicsFreezeData>()` on every `Ship`/`Floating`/`ZSyncTransform` in
the loaded world, every physics tick, whenever a TimeStop zone exists anywhere — not scoped to
what's actually inside one. `ZSyncTransform` sits on nearly every networked physics object (logs,
debris, carts, in-flight arrows). Contrast with the already-fixed `ShipPatchesWBTI` idle-tick
throttle, which is scoped to redeem-spawned ships via `TwitchShipIdlePhysicsData`.
**Fix idea:** have `TwitchPhysicsFreezeData` self-register into a static count/`HashSet` in
`Awake`/`OnDestroy` — the same self-registration pattern `TwitchSafeZone.s_activeSafeZones` already
uses. Each prefix can then start with a cheap `if (s_activeFreezeCount == 0) return true;` and only
fall through to the per-object `GetComponentInParent` walk on the rare ticks where a freeze is
actually active anywhere.

## 4. `components/TwitchTimeStopZone.cs:211-332` — `RefreshPhysicsObjectFreezes`
One `Physics.OverlapSphere` + two full-world `FindObjectsByType` scans (all live `Projectile`s,
all live `Aoe`s), every `PhysicsPollInterval` (0.1s) per active zone. Already throttled, but still
the most expensive recurring scan in the codebase; scales with concurrent active TimeStop zones.
**Fix idea:** `Aoe` already has a Harmony `Awake` postfix right in this same file
(`TimeStopPatchesWBTI.Aoe_Awake_Postfix`) — piggyback a self-registering static list of live `Aoe`
instances there (register in `Awake`, deregister on destroy/disable) instead of calling
`FindObjectsByType<Aoe>` every poll. `Projectile` has no such patch yet; would need a new
`Awake`/`OnDestroy` patch to do the same. Since projectiles are typically short-lived, that half of
the scan is lower priority than the `Aoe` half if only tackling one.

## 5. `helpers/CreatureHelper.cs:401-438` — `GetNrOfTwitchInstances`/`GetNrOfSpecificTwitchInstances`
Same bug class as the already-fixed `SpawnSystem.GetNrOfInstances` scan, just missed because it's
on the direct `SpawnCreature` redeem path rather than `SpawnAbility`/`Spawn2`. Iterates the full
`BaseAI.BaseAIInstances` list + `GetComponent<TwitchCreatureClaim>()` per creature — called once
per creature type in `HandleSpawnCreatureRedeem`, and again per candidate in `ResolveSpawnList`'s
`FindAll` when `random` is set. O(types × live creatures) per redemption.
**Fix idea:** `TwitchCreatureClaim` is already the marker component added to every Twitch-spawned
creature (see `CreatureHelper.SpawnCreature`). Same self-registration pattern as `TwitchSafeZone`
again: have it self-register into a static `List<TwitchCreatureClaim>` in `Awake`/`OnDestroy`, then
`GetNrOfTwitchInstances`/`GetNrOfSpecificTwitchInstances` iterate that list directly instead of
`BaseAI.BaseAIInstances` — skips every vanilla (non-Twitch) creature in the world entirely.

## 6. `helpers/SpawnAbilityHelper.cs:143-153`
Residual uncached `SpawnSystem.GetNrOfInstances` (full scene tag-scan) call, once per prefab in
`spawnAbility.m_spawnPrefab`, right next to the code that already fixed this exact pattern inside
`Spawn2` via the `instanceCounts` dictionary.
**Fix idea:** simplest fix — this precheck loop is small (bounded by number of configured spawn
prefabs, not `toSpawn`), so it's low priority on its own. If tackling it anyway: build the same
`Dictionary<GameObject,int>` here and either reuse it for the precheck cap math or pass it into
`Spawn2` so it doesn't build a second, separate one immediately after.

## 7. `components/TwitchChatting.cs:154-156` — `ScanAndAssignUsers`
`GetUsersInChatHistory()` + a `.Select().ToList()` LINQ chain re-run once per nearby unclaimed
creature collider instead of once before the loop. Throttled by `m_scanInterval`, so lower
severity, but wasteful with several tamed/claimable creatures near the player.
**Fix idea:** hoist `List<string> users = m_chat.GetUsersInChatHistory();` and the
`assignedUsers`/`users.RemoveAll(...)` computation above the `foreach (Collider obj in objects)`
loop — nothing inside the loop body changes chat history or `m_creatureAssignments`, so it only
needs to be computed once per scan tick, not once per candidate collider.

## Design gaps (not performance, flagged here for follow-up)

### Dedicated server support — `Player.m_localPlayer` assumption
No code anywhere in this mod currently checks `ZNet.instance.IsServer()` /
`ZNet.instance.IsDedicated()`. Several redeem paths assume a local player exists on whichever
machine is running the effect, which breaks on a true headless dedicated server (no
`Player.m_localPlayer`):
- `helpers/SpawnAbilityHelper.cs:49` — `SpawnAbility()` uses `Player.m_localPlayer.transform.position`
  as the spawn origin (would NPE with no local player).
- `components/TwitchSafeZone.cs` — `HandlePlayer`/zone-entry logic is gated on
  `Player.m_localPlayer`.
- Likely others; not yet audited exhaustively.
**Open question:** does the Twitch bot/redemption listener (`TwitchAuth`/`TwitchCustomRewards`) ever
run *on* a headless dedicated server, or is "host" always a player-hosted session in practice? That
determines whether this needs fixing at all, and what "spawn origin" should mean without a local
player if it does.
**Detection:** `ZNet.instance.IsServer() && ZNet.instance.IsDedicated()` for "is dedicated server";
`ZNet.instance.IsServer() && !ZNet.instance.IsDedicated()` for "is player-hosted". `IsDedicated()`
is hardcoded `false` in the regular client build (confirmed via decompile) — only meaningful when
this mod is actually loaded inside the dedicated server process.

## Minor / low severity (not worth prioritizing on their own)
- `helpers/FlashBangHelper.cs` `SpawnGroundExplosions` — debug-log `GetComponentsInChildren<Component>` call, bounded to 6 iterations, only fires once (`i == 0`).
- `helpers/RecolorHelper.cs` — per-spawn `transform.Find` chains, once per creature spawn (scales with bulk spawns, not per-frame).
- `helpers/DetonateHelper.cs:137` — per-target `GetComponentsInChildren<Aoe>`, bounded to `MaxTargets = 100`, coroutine-spread via `DelayBetweenDetonations`.

---

## Already fixed this session (2026-07-13), for reference
- `FlashBangHelper`/`DetonateHelper`: added `ResetQueue()`, wired into `GameAwake_Postfix`, fixing
  a stuck-forever queue if the coroutine's host GameObject was destroyed mid-effect (world
  reload/relog/logout) without ever reaching its `finally` block.
- `FlashBangHelper.AttachFlashBang`: added `flash == null` guards after each `yield` so an external
  `ClearUI()` call mid-effect (e.g. from `OnSpawned`) exits cleanly instead of throwing an uncaught
  `MissingReferenceException` that silently dropped the rest of `s_queue`.
- `harmony/FlashbangPatchesWBTI.cs`: removed the `OnDeath` cleanup by design — dying mid-flashbang
  now leaves the overlay up (intentional: adds suspense, player doesn't immediately know they
  died). Cleanup on `OnSpawned` (respawn) is kept and remains safe due to the guards above.
