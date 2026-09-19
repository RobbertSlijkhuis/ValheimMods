# Imbuement system: refactor notes

Findings from reading the imbuement code in `ModularMagic_Core` and `ModularMagic_EarthStaffs`. This is an information-gathering pass only. No plan has been made and no code has been changed.

## How it works today

**Core (`ModularMagic_Core`)**

- **Rune item:** `components/ImbuementRune.cs` is a MonoBehaviour added to each rune prefab at startup (`ModularMagic_Core.cs:58`). It holds type, value, tier, level, allowed weapons and the locale keys.
- **Rune definitions:** `data/Rune*Data.cs` builds `RuneEntryOptions`, which become `RuneEntry` and then `ItemConfig`. Each rune has four copy-pasted tier variants: Wood, Stone, Marble and Grausten.
- **Slot model:** `models/imbuement/Imbuement.cs` has 9 fields: type, prefab, name, description, value, tier, level, saved and weaponType.
- **Persistence:** `helpers/ImbuementHelper.cs` (`ListToString`/`StringToList`) writes the list into `item.m_customData["Imbuements_MMC"]`. The format is `a|b|...|i;a|b|...`, with 9 positional fields.
- **Table logic:**
  - `components/RuneTable.cs` holds the working list, the attached item and the save flow. `CanSave` compares serialized strings.
  - `components/RuneTableRuneInteract.cs` is the per-slot component, about 370 lines. It does validation, state mutation, inventory changes, reset and kick animations, and dropping old runes.
  - `components/RuneTableAccept.cs` is the save button.
  - `harmony/PatchesMMC.cs` attaches and detaches the staff via `ItemStand.UseItem` and `ItemStand.Interact`.

**EarthStaffs (consumer)**

- **Slot creation:** `components/Imbuements.cs` creates N empty slots from `m_slots` and `m_tier` (set in `ModularMagic_EarthStaffs.cs:63-77`). It writes them into the prefab item's custom data.
- **Applying effects:** `PatchesMMES.EquipItem_Postfix` calls `helpers/ImbuementHelper.ApplyImbuements`. That helper is a large `switch` over `imbuement.type`. It fills `UpdateItemDataOptions` from a per-staff snapshot plus config values x level, and it builds an equip status effect for the tooltip.

## Problems

1. **The model and serializer are duplicated.** `Imbuement`, `ImbuementHelper.StringToList/ListToString` and the type constants exist in both Core and EarthStaffs, and `imbuementDataKey` is defined twice.
   - The EarthStaffs copy is the old, fragile version: `int.Parse` and `bool.Parse` throw on bad data, `value.Split` throws on null, and there is no `Clean()` for `|` and `;` in fields.
   - Core's copy is hardened, but only in the uncommitted diff.
   - Any future Fire, Ice or Lightning staff mod would need a third copy.
2. **The save format is brittle.** It has 9 positional fields with no version. It stores denormalized and localized data: `name` and `description` are localized at slot time and baked in, so a language change leaves stale text. `prefab`, `tier` and `weaponType` are stored per slot even though they could be derived. Any schema change breaks existing items.
3. **Types are stringly-typed.**
   - `ImbuementType`, `WeaponType` and `CanImbueType` are static string properties.
   - `case nameof(ImbuementType.X):` only works because the property name equals its value.
   - Several types are unused: `AOE`, `CreatureSpawn`, `DamageRatio`, `MaxQuality`, `ParryBonus`, and `CanImbueType.No`.
4. **The same 7-field copy from rune to `Imbuement` appears three times** in `RuneTableRuneInteract`: `UseItem`, `CompleteReset` and the clear in `Interact`. A rune-to-imbuement `Apply`/`Clear` would replace all three. `UseItem` also mixes about 6 validations with mutation and inventory removal.
5. **`RuneTableRuneInteract` does too much.** Rules, animation, inventory, state and drop logic are all in one class, and the state machine is implicit (`m_locked`, `m_dropOldItem`, `m_itemNew`, `saved`).
6. **Effect application is tightly coupled to Earth staffs.**
   - `GetItemDataSnapshot` hardcodes the staff prefab names.
   - `ApplyImbuements` toggles child visuals and rotate values on the shared `ProjectileDefault` prefab on every equip (the prefab asset itself never changes). That is per-client state, and the toggles are not synced to other clients.
   - It only runs on `EquipItem`.
   - The staff-specific effect logic and the generic imbuement handling live in the same method.
7. **Small bugs and dead code.**
   - The debug logs in `ApplyImbuements` (lines 120-121) have an operator-precedence bug: `"Main: " + x == null ? ... : ...` always evaluates the string comparison first.
   - `imbuements.Last()` throws on an empty list.
   - `Imbuements.m_imbuements` is unused after `Awake`.
   - There are large commented-out blocks.
8. **Registration is repetitive.** `InitAssetBundle` has about 100 lines of "load / clone variant / AddPrefab" boilerplate per rune. `RuneEntryOptions` and `RuneEntry` mirror each other, and `ImbuementRune.Init` mirrors both again.
9. **Slot and tier rules are scattered.** They sit in `AddEarthStaffs`, and Staff 3 has tier 4 while the runes use 0, 2, 3 and 4. They aren't tied to the rune tier data in one place.

10. **Item-data update helpers are duplicated too.** EarthStaffs has the full `UpdateHelper`, `UpdateItemDataOptions` and `UpdateProjectileOptions`. Core has stubs that only handle name and description. `UpdateItemDataInHand` hardcodes the Staff1-3 prefab names and uses `Player.m_localPlayer`.
11. **`m_attack` is assigned by reference.** `UpdateItemData` does `itemData.m_shared.m_attack = options.mainAttack.m_shared.m_attack`, so the staff shares the Cone item's `Attack` object. Later writes such as `m_attackEitr` and `m_projectileVel` also modify that shared object. Values are re-set on every equip, so it mostly hides, but swapping in a projectile variant on it would leak to other staffs. Use `Attack.Clone()` (it exists in the game code) before modifying it.

12. **Runtime-cloned or adjusted prefabs may not be multiplayer-safe (unverified).** The user's experience is that cloned or adjusted prefabs tend not to work well in multiplayer.
    - The rune variants are cloned at startup with `CreateClonedVariant` (4 tiers per rune).
    - `AttackHelper.UpdateCone/Nova/Rain/Summon` adjust attack prefabs at `OnVanillaPrefabsAvailable` from `PluginConfig` values. If those configs are synced from the server after joining, startup would use local values and clients could differ.
    - The BloodMagic Scythe's apparitions spawned but were invisible to other players. Likely cause (unconfirmed): Valheim identifies networked prefabs by the stable hash of the prefab name. `ZNetScene.CreateObject` looks that hash up in the client's `m_namedPrefabs`, and if it is missing it logs `Missing prefab hash: <name>` and does not create the object. Check the other client's log for that line.
    - **How BloodMagic does it today:** no cloning. It adds components to the vanilla `Charred_*` prefabs at startup (`SetupPrefabs`) and spawns with a plain `Object.Instantiate`. The per-weapon variation is stored in the instance ZDO (`ApparitionData_MMBM`). `ApparitionController.Awake` reads it on every client and applies scale, per-instance materials, particle colors and health. This ZDO-driven pattern is an alternative to separate prefab variants for the projectiles: one prefab plus a component that reads a damage-type key on `Awake`.
    - **Rule for this refactor:** every prefab used by a networked object must have a fixed name and be registered on every client at startup. Never create such prefabs lazily at runtime or with config-dependent or per-client names.
    - Decision so far: the new projectile variants are authored by hand in Unity and loaded from the bundle. Whether the rune variants and the attack tweaks need the same treatment is undecided.

## BloodMagic apparition findings (out of scope, to pick up later)

Found while investigating multiplayer prefab behavior. These are code-reading findings only and have not been tested with two clients. The user decided to leave `ModularMagic_BloodMagic` alone for this refactor.

**How it works**

- No cloning. `SetupPrefabs` (`ModularMagic_BloodMagic.cs:41-88`) fetches the vanilla `Charred_Melee`, `Charred_Twitcher` and `Charred_Archer` and adds `ApparitionController` and `Tameable` to them directly.
- `SoulReaperHelper.SpawnApparition` does a plain `Object.Instantiate` on the scythe owner's client. `SetData(weaponName)` writes `"ApparitionData_MMBM"` to the ZDO, and `ApparitionController.Awake` reads it on every client to apply scale, per-instance materials, particle colors and health.

**Works in multiplayer (checked against the game code)**

- Visible to everyone, because the vanilla prefabs exist in every client's `ZNetScene`.
- Per-weapon variation replicates through the ZDO key.
- Damage scaling (`ApparitionPatches`) runs on the client whose apparition attacks, because `Character.Damage` only sends an RPC. The configs are `IsAdminOnly` and read at use time.
- Scythe charge visuals use the player's ZDO, and the damage hook runs on the attacker's client.

**Issue: `Awake` writes health to the ZDO on every client**

`ApparitionController.Awake` calls `ApplyHealth`, which sets `max_health` and `health` in the ZDO each time the object is created on any client.

1. `ZDO.Set(float)` has no owner check and bumps the local `DataRevision`. `ZDOMan` ignores incoming data whose revision is `<=` the local one (`ZDOMan.cs:1176`), so a non-owner's copy can briefly ignore the owner's updates. Their health bar may show full health for a while.
2. When the owner's object is re-created (teleport, zone reload, relog), `Awake` resets health to max, which is a free full heal.
3. The write isn't needed. `SetData` already writes health once at spawn, and `Character` reads `max_health` from the ZDO.

Suggested fix: apply health only in `SetData`, and keep only the local visuals in `Awake`.

**Minor**

- `SetupPrefabs` adds `Tameable` and `ApparitionController` to the vanilla `Charred_*` prefabs, so naturally spawned Charred creatures get those components too. It is consistent across clients but changes vanilla creatures.

## Implementation status

The shared API is documented for staff-mod authors in `ModularMagic_Core/IMBUEMENT_API.md`.

The approved plan is in `C:\Users\robbe\.claude\plans\lets-gather-information-first-deep-frog.md`.

- **Phase 1 (shared model): written, not yet built or tested.**
  - **Core:**
    - `ImbuementType` and `WeaponType` are public consts.
    - `ImbuementRune` has an `id`, and `ImbuementSlots` is new.
    - `Imbuement` is computed from the rune lookup.
    - `ImbuementHelper` is public, with the versioned serializer (`1:Id@level,...`), the rune registry and the duplicate/missing-id warnings.
    - The `RuneTable*` components and `PatchesMMC` use the new model.
  - **EarthStaffs:**
    - The duplicated model, types, serializer and `Imbuements` component are deleted.
    - `helpers/EarthImbuementHelper.cs` replaces `ImbuementHelper`.
    - Reference to Core's built DLL (`HintPath` to `ModularMagic_Core\bin\$(Configuration)\net48`, `Private=false`). A `ProjectReference` was tried first but failed in Visual Studio with a NuGet restore error (NU1105: the Core project is not part of the EarthStaffs solution). **Build Core first whenever its public API changes.**
  - **Rune ids:** DamageBlunt, DamagePierce, DamageSlash, EitrCost, ProjectileAccuracy, ProjectileBurst, ProjectileSpeed, Cone, Creatures, Nova, Rain.
  - **Build order:** build Core first (the `bin\Debug\net48\ModularMagic_Core.dll` that exists now predates the public API), then EarthStaffs.
- **Phase 2 (multiplayer table): written, not yet built or tested.** Core only, so only Core needs a rebuild.
  - `PatchesMMC`: a postfix on the private `ItemStand.SetVisualItem` calls `RuneTable.SyncFromStand()` on every client (it also runs from the stand's own 4 s refresh, so late joiners and reloaded zones attach too). The old attach and remove postfixes are gone. `UseItem_Postfix` only stamps the editor on the queued item.
  - `RuneTable`:
    - `SyncFromStand()` reads the stand's ZDO through `ImbuementHelper.LoadItemFromZDO`, attaches, removes or refreshes the table, and works on the stand's own copy of the item instead of the detached inventory item.
    - `Save()` sends `RPC_MMC_SaveImbuements(string, long editorId)` to the ZDO owner. The owner checks the editor id, normalizes the string, writes it with `SaveToZDO` and sends `RPC_UpdateVisual` to everyone. The player-profile save is removed.
  - **Editor lock:** the editor is stored as `MMC_Editor` = the character's persistent player id (not the session id, which changes every time a world is joined). Only the editor can slot, remove, reset or save. Others get a read-only hover with a lock message.
  - **Known behaviors to expect:**
    - A staff that was already on a table before this change has no editor stamp. Nobody can edit it until it is taken off and placed again.
    - Players who did not place the staff can still take it off (vanilla `ItemStand`). That is also how a lock is released when the editor has disconnected.
    - Remote clients may see an attach or save up to about 4 s late if the stand's data arrives after the message. The stand's refresh covers it.
    - When a viewer's runes refresh after a save, the old runes fly out while the new ones fly in, so the animations overlap briefly.
    - The owner trusts the editor id sent with the save request (it cannot look up the sender's player id).
- **Phase 5 (unsaved runes kept on the table): written, not yet built or tested.** Core only. It was added after single-player testing showed unsaved runes were lost on relog (the runes left the inventory when slotted and only lived in the editor's memory).
  - **Draft on the stand:** the editor's working copy is stored as `MMC_Draft` in the stand's item data. Every change (slot, take out, mark for removal, reset) sends it to the ZDO owner (`RPC_MMC_SaveDraft`, editor-checked). Save clears it. When the editor's table attaches it restores from the draft, including the visuals for a rune marked for removal or replaced (`RuneTableRuneInteract.RestoreReplacedRune`). "Apply changes" shows when the draft differs from the saved data.
  - **No inventory calls in the rune table.** `m_itemNew` and `OnRemove` are gone. Taking an unsaved rune out of a slot drops it from that slot (like a removed saved rune drops on save). A prefix on `ItemStand.DropItem` (owner only, also runs when the table is destroyed) drops the draft's unsaved runes from their slots and strips the draft from the staff's data, so it works when the editor is offline. The user chose this over returning runes to the inventory, to avoid two paths handing out the same rune.
  - **Remaining risk:** a very small duplication risk if a draft update message were lost after a rune was dropped. Messages are reliable, so this is unlikely.
  - **Also:** `SyncFromStand` now waits until the local player exists (the editor check needs the player id), and `UseItem_Postfix` clears any stale draft on the staff when it is placed.
- **Phase 3 (projectile variants): written, not yet built or tested.** EarthStaffs only.
  - New `helpers/ProjectileHelper.cs`. At startup (in `InitAssetBundle`, so on every client) `projectile_MMES` is set to the blunt visual once, and `projectile_MMES_slash` and `projectile_MMES_pierce` are cloned from it (fixed names) and registered with `PrefabManager`. The rotation values are the ones that `ApplyImbuements` used before (blunt 300/0/0, slash 500/0/10, pierce 0/0/500).
  - `EarthImbuementHelper.ApplyImbuements` no longer toggles the projectile children. A Slash or Pierce rune sets `options.attackProjectile`, unless the Cone main attack is active (it has its own projectile, as before).
  - `UpdateHelper.UpdateItemData` now clones the attack (`Attack.Clone()`) when assigning `mainAttack` and `secondaryAttack`, and applies `attackProjectile` to that copy. Only `ApplyImbuements` sets those two options.
  - Note: `ApplyImbuements` only sets `options.secondaryAttack` when a secondary-attack rune is present, and `snapshot.secondaryAttack` is unused. This is not a bug in practice: runes can only be changed at the table, and taking a staff off the stand spawns a fresh item from the prefab with the default attacks. (This was first written up as a probable bug and the user confirmed in-game that removing or replacing the secondary works.)
- **Phase 4 (cleanup): written, not yet built.**
  - Removed `CanImbueType.No` (`CanSave()` never returned it, and `Save()` now checks `!= Yes`).
  - The unused `ImbuementType` constants (`AOE`, `CreatureSpawn`, `DamageRatio`, `MaxQuality`) were removed and then put back at the user's request. They are reserved names for possible future rune types.
  - Left on purpose: `ImbuementType.ParryBonus` and the commented-out ParryBonus code in `EarthImbuementHelper` (a paused feature), and `RuneTable.saveEffects` (built in `Awake`, its `Create` call is commented out, so it looks like a paused visual effect).

**All four phases are written. Nothing has been built or tested yet.** Build order: Core first, then EarthStaffs.

## Code review (`/code-review high` on ModularMagic_Core)

Eight findings. Outcome:

- **Fixed (written, not yet built or tested):**
  - `ImbuementSlots.m_weaponType` now defaults to `WeaponType.None`, and `GetName`/`GetDescription`/`Imbuement` are null-safe, so a staff mod that forgets it no longer breaks every rune hover.
  - The same rune can not be slotted while the same rune is marked for removal in any slot (`RuneTable.IsRuneMarkedForRemoval`). This also closes a case where a rune could be lost: with the old rune marked for removal, slotting the same rune again made the working copy equal the saved copy, so no draft was stored and the consumed rune was not dropped. Consequence chosen by the user: changing a rune's level takes two applies (remove and apply, then slot the new one).
  - `RuneTableRuneInteract.OnDestroy` removes its `m_onSave` listener (they piled up on the table with every refresh or re-attach; this predates the refactor).
  - Save and draft requests that reach a stand the receiver no longer owns are forwarded to the real owner (with a hop counter, max 3) instead of being dropped, so the runes consumed for them are not lost. The RPCs now carry a hop count (`Register<string, long, int>`).
- **Decided: no anti-cheat.** The save and draft requests are authorised by the editor id that the client sends, which anyone can read from the stand, and the owner does not check tier, allowed weapon or one-rune-per-type. A modified client can therefore overwrite runes or give itself runes. The user does not want protection against modified clients ("if they want to cheat that way so be it"). Do not add sender checks or extra validation for this. (If a sender check were ever added, the forwarded request would have to carry the original sender, because forwarding changes the sender.)
- **Not done on purpose:** performance caching (`SyncFromStand` skipping on an unchanged data revision, caching the rune in `Imbuement`); the user does not want caching. Old-format migration (decided: none).

## Follow-ups (not done)

- **Two-player test** of phases 2 and 3 (see the verification list in the plan file), including the `Missing prefab hash` check in the other client's log.
- **Rune consolidation:** merge the tier variants into one rune with a level (needs Unity changes). The save format is already keyed by rune id and level.
- **Startup attack tweaks / config sync: checked and fixed (not yet built or tested in-game).**
  - Jotunn applies server values by setting `ConfigEntry.BoxedValue` (`SynchronizationManager`, and again when disconnecting), which raises `SettingChanged`. Every entry in `StaffConfig` (28 of 28) and `SecondaryAttackConfig` (19 of 19) has a `SettingChanged` handler that re-applies the value to the prefab, so the startup tweaks (`AttackHelper.UpdateCone/Nova/Rain/Summon`, `CreateStaff`) are refreshed when a synced value arrives.
  - **Real gap found:** the staff snapshots (`ItemDataSnapShot`) copied the staff stats once at startup, and `ApplyImbuements` writes those copies onto the equipped staff on every equip. After a synced or changed config value, an imbued staff used the stale startup numbers (eitr cost, blunt damage and per level, accuracy, attack speed, projectile speed). This predates the refactor.
  - **Fix:** `ItemDataSnapShot.Init(itemData, config)` now keeps the `StaffConfig` and reads those values from it on every access. Four unused snapshot fields (pierce and slash damage, per level, and `secondaryAttack`) were removed.
  - **How to test in single player:** change a staff's eitr cost (or accuracy, damage) in the config manager, take the staff off and equip it again, and it should use the new value without a restart. To test the real sync: change a value on a server and join with a client that has the default config.
- **`UpdateHelper` variants:** deliberately left as is. `UpdateHelper.cs` is a copy-paste pattern in 11 projects (Armors, Food, Utilities, PlantCart, SplashMeads, Testing, Fire, Ice, Lightning, EarthStaffs and Core's stub), with two families of options classes (`UpdateItemDataOptions` and `UpdateItemDropStatsOptions`). Revisit when a second staff mod adopts the imbuement API, and extract only the common item-data part into Core then.
- **BloodMagic:** the `Awake` health write (see the section above).
- **Mushroom projectile** on Staff 0 still randomizes per client.

## Uncommitted state

`git status` shows Core changes in `ImbuementHelper`, `RuneTable*`, `PatchesMMC`, `ModularMagic_Core.cs` and `CustomPrefabs`. That's about 46 lines added and 30 removed, and it looks like a hardening pass on the serializer. A refactor should build on top of it or land after it.

## Multiplayer findings

Verified against `ItemStand` and `ItemDrop` in `assembly_valheim.dll`.

**What syncs**

- `ItemStand` keeps the attached item in its ZDO (`s_item`, plus the full item data including `m_customData` via `ItemDrop.SaveToZDO`).
- The attach visual is broadcast to everyone with the `SetVisualItem` RPC.
- `DropItem` restores the item data from the stand's ZDO, so saved imbuements travel with a dropped staff.

**Gaps**

1. **Runes appear only for the interacting player.** `RuneTable.StaffAttach`/`StaffRemove` run from Harmony postfixes on `ItemStand.UseItem` and `Interact`, which run only on the client that pressed the key. Other players see the staff but no runes, and the table never reads the ZDO to attach itself when a player joins or walks up later.
2. **Nothing is authoritative.** The slot list (`m_imbuements`, `m_itemNew`, `m_dropOldItem`) lives only in that client's `RuneTable`. Two players can edit at once, a stale table can save to a stand that no longer holds the staff, and a rune taken from the inventory but not yet saved is lost if the player disconnects.
3. **`Save()` writes the ZDO from whoever clicks,** with no ownership check. Non-owner writes don't propagate reliably. It only works today because `UseItem` happened to request ownership.
4. **Projectile visuals are chosen per client (cosmetic, and not yet verified in-game).** The prefab asset never changes. `ApplyImbuements` toggles the children of `ProjectileDefault` (`visual/blunt`, `visual/slash`, `visual/pierce`) with `SetActive` on the equipping client, and `RandomizeMushroom` does the same for the mushroom projectile. `Projectile` is a networked object, so other clients should instantiate their own copy of the prefab with their own toggle state. Another player's Slash or Pierce projectile may then look like the default on your screen. Damage is computed by the attacker's client, so it is fine as long as that client applies imbuements correctly. The `ZNetScene` spawn path was not decompiled, so confirm this in-game before acting on it. **Chosen fix: projectile variants.** Make Blunt, Slash and Pierce variants of the projectile and have the attack point at the right one. `ZNetScene` spawns networked objects by prefab hash, so every client then spawns the correct visual with no extra syncing. The variants are made by hand in Unity as separate Blunt, Slash and Pierce projectile prefabs (not cloned in code with `CreateClonedVariant`). Each variant carries its own `m_visual` and rotate values, so the per-equip toggling in `ApplyImbuements` goes away. Load and register each one with `PrefabManager` in the mod's asset bundle setup. Only `ProjectileDefault` needs variants, because the Cone main attack uses its own projectile. The mushroom projectile (`RandomizeMushroom`, staff 0) keeps its cosmetic per-client randomness for now.
5. `Game.instance.GetPlayerProfile().SavePlayerData(...)` in `Save()` is unnecessary because the staff lives on the stand, not in the inventory.

Slot creation is fine: crafting instantiates the prefab, so `Imbuements.Awake` runs on the instance and seeds the custom data.

## Decisions

1. **One editor per table.** Only the player who put the weapon on the table may edit it. Other players cannot edit until it is removed.
2. **Versioned save format.** Add a version to the serialized data so future migrations are possible. No migration is needed now, and breaking existing test staffs is acceptable.
3. **Shared code goes in `ModularMagic_Core`,** exposed to the staff mods. That covers the model, the serializer, the type constants and the data key. EarthStaffs and future staff mods depend on it.
4. **Scope:** C# only for now, no Unity work. Changing behavior is acceptable.

## Proposed direction (not yet a plan)

- One shared `Imbuement` model and versioned serializer in Core, plus a small public API for staff mods to read imbuements and register effect handlers.
- Make the stand ZDO the source of truth. `RuneTable` attaches from the stand's data on every client (not only from the local postfix). Saves are owner-checked and go through the editing player's ownership.
- Record the editing player on the stand (for example the player ID in the ZDO) and enforce the single-editor rule in the hover and interact code.
- Apply imbuement effects through handlers per item. Projectile visuals come from per-damage-type projectile variants (see finding 4), set on a cloned `Attack` (finding 11).
