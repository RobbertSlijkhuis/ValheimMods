# ModularMagic_Core: shared imbuement API

Core owns the rune table, the runes, the data that says which runes are on a weapon, and the staff framework that applies the runes to a staff. A staff mod (FireStaffs, IceStaffs, EarthStaffs, LightningStaffs) loads its prefabs, binds its config and describes its staffs in a `StaffDefinition`. This page explains how a staff mod plugs in.

`ModularMagic_FireStaffs` (`ModularMagic_FireStaffs.cs` `RegisterStaffHooks`, `helpers/FireImbuementHelper.cs`) is the simplest reference implementation. `ModularMagic_EarthStaffs` shows everything: damage type runes with their own projectiles, a summoned creature and a custom terrain op.

## The staff framework (the way to add a staff mod)

A staff mod does not apply the runes itself. It fills in a `StaffDefinition`, registers it once, and adds its staffs to it. Core then gives the staffs their rune slots, applies the runes every time a staff is equipped (and again when a config value changes), speeds up the attack animation for the Attack Speed rune, and scales summoned creatures with the staff.

```csharp
// In Awake, after the config is bound (PluginConfig.Init) and the prefabs are loaded
definition = new StaffDefinition()
{
    effectName = "Imbuements_MMFS",            // the tooltip status effect, unique for the mod
    weaponType = WeaponType.MMFS,               // the suffix of the rune texts
    colorPreset = LightColorPresetType.Orange,  // the colour of the rune table
    allowedRunes = FireImbuementHelper.AllowedRunes,
    imbuementConfig = PluginConfig.imbuementConfig,
    attackSpeedAnimation = true,                // false: the Attack Speed rune lowers the burst interval instead
    coneAttack = prefabs.mainAttackCone,        // the wrapper prefab of the cone rune, and its config
    coneConfig = PluginConfig.mainAttackCone,
    physicalDamageType = null,                  // "Blunt" or "Pierce" for staffs with damage type runes
};
definition.AddSecondaryAttack("Nova", prefabs.secondaryAttackNova, PluginConfig.secondaryAttackNova);
definition.AddSecondaryAttack("Rain", prefabs.secondaryAttackRain, PluginConfig.secondaryAttackRain);
// definition.AddSecondaryAttack("Summon", wrapperPrefab, config, creaturePrefab);   // a summoned creature
// definition.AddMainAttackAnimation("Greatsword", "greatsword", 3, ItemDrop.ItemData.AnimationState.Greatsword); // swing it like a greatsword
// definition.damageProjectiles["Slash"] = slashProjectilePrefab;                    // a projectile per damage type

StaffImbuementHelper.Register(definition);
SpawnedProjectileHelper.Register(prefabs.projectileMeteor.name, OnProjectileSpawned);   // the damage of the Rain projectiles

// When the vanilla prefabs are available: slots, tier, the item with its recipe and stats, and the base stats for the runes
definition.AddStaff(prefabs.staffFire1Prefab, PluginConfig.staffFire1, slots: 2, tier: 1);
```

The config classes are in Core too (`StaffConfig`, `SecondaryAttackConfig`, `MainAttackConfig`, `ImbuementConfig` with their `*Options`), as well as `UpdateHelper`, `RecipeHelper` and `StaffHelper.CreateStaff`. The mod binds them with its own `ConfigFile` (`staffConfig.GenerateConfig(Config, options)`), so each mod keeps its own config file. A stat or attack entry is only bound when its option has a value, so a staff only has the entries its mod gives a value for. `ImbuementConfigOptions.eitrCost` is the default of the "Eitr Cost Rune" entry (0.25 for a staff, the scythes use 0.75 for their charging hit). A mod reloads the config on the held staff with `Config.SettingChanged += (s, a) => ImbuedStaffHelper.ReapplyOnHeldStaff();`.

What Core does with the definition when a staff is equipped (`StaffImbuementHelper.Apply`): the base stats come from the config snapshot of the staff (`ItemDataSnapShot`, read from the config every time, so synced values are used), and then each rune is applied on top: Eitr Cost, Accuracy, Projectile Speed, Attack Speed, the damage type (moves the main damage between blunt, pierce and slash, and swaps the projectile), the Cone (replaces the main attack), a main attack animation rune such as Greatsword (keeps the attack, changes its animation, the hits of its chain and how the staff is held) and the secondary attacks (with the eitr savings per level). A mod only writes code for what is special about its staffs.

How a staff is held (`m_animationState`) is read by the game when the staff is equipped, before Core applies the runes in the `EquipItem` postfix. When the runes change it, Core sets the pose of the character again (`ImbuedStaffHelper`), so the Greatsword rune shows on the first equip and after a relog.

The Range (`AttackRange`) and Legion (`SummonLimit`) runes are not applied by Core: the mod reads their level with `ImbuementHelper.GetLevel(item, type)` and the value per level from its `ImbuementConfig` (`AttackRange`, `SummonLimit`). Those entries are only bound when the mod sets `ImbuementConfigOptions.attackRange` / `summonLimit`.

### Hooks for the parts of an attack

- **`SpawnedProjectileHelper.Register(prefabName, apply)`**: the projectiles a `SpawnAbility` spawns (meteors, bolts, boulders, shards) are spawned without hit data, so they would deal the damage of their prefab. Core calls `apply(projectile, staff)` for each spawned projectile of that prefab, and the mod sets the damage of that one projectile from the staff.
- **`SpawnedCreatureHelper.Register(prefabName, apply)`**: the same for creatures a `SpawnAbility` spawns. A secondary attack with a creature is scaled by Core: the main damage of the creature becomes the main damage of the staff (quality included) times the "Damage multiplier" of the summon config, through the `s_randomSkillFactor` value in the ZDO of the creature (synced, and kept when another client becomes the owner). The composition of the damage (blunt, spirit, ...) is set in the weapon prefab of the creature.
- **`SpawnedCreatureHelper.SetDamage(creature, damage, baselineDamage, multiplyExisting)`**: the same scaling for a creature that is not part of a secondary attack (call it from your `Register` callback). The attack that deals `baselineDamage` at factor 1 now deals `damage`, and every other attack of the creature deals the same fraction of its own damage, so the attacks keep their strength compared to each other. Use one baseline for several creature prefabs (for example the strongest attack of all of them) to keep those prefabs balanced against each other. `multiplyExisting` multiplies into the factor that is already there (for example the Blood Magic factor a `SpawnAbility` writes with `m_copySkill`) and does not replace it.
- **`TerrainOpRegistry.Register(prefab)`**: a custom terrain op prefab must be in the table of every `ObjectDB` or the terrain edit fails with "Failed to deserialize TerrainOp settings". Jotunn registers it before the `ObjectDB` of the game scene exists, so Core adds it when each `ObjectDB` builds its table.
- **`AttackSpeedHelper`** (used by the framework): sets the animation speed for the Attack Speed rune and puts it back when the attack ends or the staff is put away.
- **`ImbuedStaffHelper`** (used by the framework): `Register(isStaff, apply)` is what applies the runes on equip, for a weapon that does not use a `StaffDefinition`.

## Setting up a weapon by hand (without the staff framework)

The steps below are what the framework does for a staff. They are still the way for a weapon that is not a staff.

## Concepts

| Term | Meaning |
|---|---|
| **Rune** | An item you slot into a staff on the rune table. There is one item per rune **id**, for example `DamageSlash`. |
| **Level** | How strong a rune is. The level is the **quality** of the rune item (`ItemData.m_quality`, 1 up to the number of models the rune has), so a rune is upgraded like any item. A slotted rune is stored as its id and level. |
| **Slot** | One place on a staff that holds one rune. A staff has a number of slots, a slot **tier** and a **weapon type**. |
| **Tier** | A rune can only be slotted when the slot tier is at least the tier the rune needs for its level (`ImbuementRune.GetRequiredTier(level)`). |
| **Weapon type** | `WeaponType.MMBM`, `MMES`, `MMFS`, `MMIS` or `MMLS`. It is the suffix of the rune texts of the weapon (see [Localization](#localization)). |
| **Supported runes** | The rune ids a weapon lists in `ImbuementSlots.m_allowedRunes` (see `RuneId`). The weapon decides this, a rune that is not listed can not be slotted on that weapon. |
| **Saved runes** | The runes stored on the staff item, written when the player applies the changes on the table. |
| **Draft** | The unsaved changes of the player who is editing. It is stored on the table, so it survives walking away and relogging. |
| **Editor** | The player who put the staff on the table. Only that player can edit it. |



1. **Reference Core.** Add a reference to the built Core DLL, and depend on it in the plugin:

   ```xml
   <Reference Include="ModularMagic_Core">
     <HintPath>..\..\ModularMagic_Core\ModularMagic_Core\bin\$(Configuration)\net48\ModularMagic_Core.dll</HintPath>
     <Private>false</Private>
   </Reference>
   ```

   ```csharp
   [BepInDependency("DeathWizsh.ModularMagic_Core")]
   ```

   Build Core first whenever its public API changes. A `ProjectReference` does not work when Core is not part of the staff mod's solution (NuGet restore error NU1105).

2. **Make the staff fit on the table.** The rune table is an item stand, so the staff prefab needs an `attach` child like every item that can go on a stand. The Earth staffs already have one.

3. **Add `ImbuementSlots` to the staff prefab at startup**, before the item is registered:

   ```csharp
   ImbuementSlots slots = staffPrefab.AddComponent<ImbuementSlots>();
   slots.m_slots = 2;                     // number of rune slots
   slots.m_tier = 1;                      // slot tier
   slots.m_weaponType = WeaponType.MMFS;  // the suffix of your rune texts
   slots.m_allowedRunes = new List<string> { RuneId.EitrCost, RuneId.Cone, RuneId.Nova };  // the runes this weapon supports
   slots.m_colorPreset = LightColorPresetType.Blue;  // the colour of the rune table while this weapon is on it
   ```

   `m_colorPreset` is required: it colours the table glow, the runes in the slots and their particles. Use a `LightColorPresetType` (`Red`, `Orange`, `Yellow`, `LemonGreen`, `Green`, `LightBlue`, `Blue`, `Pink`, `Purple`, `White`). When it is missing or not a known name the table is white. The button of the table keeps its own state colours (green, yellow, red).

   A weapon is imbuable when its drop prefab has this component. Nothing has to be seeded into the item. `m_allowedRunes` is empty by default, so a forgotten list shows up as runes that can not be slotted. The weapon also decides how high it goes: a rune level can only be slotted when the slot tier (`m_tier`) is at least the tier that level needs (set per rune in Core, see `m_tiers`), so a low tier weapon can not take high level runes, and some runes not at all.

4. **Apply the runes when the staff is equipped.** Patch `Humanoid.EquipItem` and read the runes with `ImbuementHelper.Read`:

   ```csharp
   [HarmonyPostfix]
   [HarmonyPatch(typeof(Humanoid), "EquipItem")]
   public static void EquipItem_Postfix(Humanoid __instance, ItemDrop.ItemData item)
   {
       if (__instance == null || !__instance.IsPlayer() || item == null)
           return;

       // Every staff mod shares the API, so only handle the staffs of this mod
       if (!ImbuementHelper.HasImbuements(item) || !IsMyStaff(item))
           return;

       foreach (Imbuement imbuement in ImbuementHelper.Read(item))
       {
           switch (imbuement.type)
           {
               case ImbuementType.EitrCost:
                   eitr -= amountPerLevel * imbuement.level;
                   break;
               case ImbuementType.MainAttack:
                   if (imbuement.value == "Cone") { /* switch the main attack */ }
                   break;
           }
       }
   }
   ```

   `Read` always returns one entry per slot, an empty slot has `type == ImbuementType.None`.

5. **Add the texts of your runes** (see [Localization](#localization)). Without them a rune shows its default English name and description.

## What a rune gives you

`imbuement.type` and `imbuement.value` tell you what to do, `imbuement.level` says how strong it is. How much a level is worth is up to the staff mod. EarthStaffs uses its own config amounts multiplied by the level.

| Rune id | `type` | `value` | Levels | Used to be allowed on (a starting point for your `m_allowedRunes`) |
|---|---|---|---|---|
| `DamageBlunt` | `DamageType` | `Blunt` | 1-4 | MMIS |
| `DamagePierce` | `DamageType` | `Pierce` | 1-4 | MMES |
| `DamageSlash` | `DamageType` | `Slash` | 1-4 | MMES, MMIS |
| `EitrCost` | `EitrCost` | `0.25` | 1-4 | all four |
| `ProjectileAccuracy` | `ProjectileAccuracy` | `0.125` | 1-4 | MMES, MMIS |
| `AttackSpeed` | `AttackSpeed` | `0.0125` | 1-4 | MMES, MMIS |
| `ProjectileSpeed` | `ProjectileVelocity` | `1` | 1-4 | MMES, MMIS |
| `Cone` | `MainAttack` | `Cone` | 1-4 | all four |
| `Creatures` | `SecondaryAttack` | `Summon` | 1-3 | all four |
| `Nova` | `SecondaryAttack` | `Nova` | 1-3 | all four |
| `Rain` | `SecondaryAttack` | `Rain` | 1-3 | MMES, MMFS, MMIS |
| `BloodTap` | `SecondaryAttack` | `BloodTap` | 1-3 | MMBM |
| `Greatsword` | `MainAttack` | `Greatsword` | 1-4 | MMBM |
| `Range` | `AttackRange` | `1` | 1-4 | MMBM |
| `Legion` | `SummonLimit` | `1` | 1-4 | MMBM |
| `Utility` | `Utility` | `1` | 1-4 | MMIS (the ice sheets) |

The numeric `value`s are hints. EarthStaffs ignores them and uses its config. The tiers of the levels are in `data/Rune*Data.cs`. Which runes a weapon supports is set by the weapon: the Earth staffs list everything above except `DamageBlunt` (`EarthImbuementHelper.AllowedRunes`). The last column is what Core used to allow per weapon type, so a new staff mod can start from it.

A staff can hold only one rune of each `type` at a time. The same rune also can not be slotted while the same rune is marked for removal in any slot, because it is still on the staff until the changes are saved. To change the level of a rune, remove it, apply the changes, and slot the new one.

Runes are upgraded with the normal upgrade screen at the rune table. The upgrade costs are the "Upgrade costs" of the rune in the config, multiplied per level (1x, 2x and 4x for levels 2, 3 and 4), and the station level does not matter for runes. A rune drops with its level as its quality, and the model on the rune and in the slot and the inventory icon follow the level.

## Localization

The staff mod owns the texts of its runes, since a rune can do something different on every weapon. The name and description of a rune are looked up when they are shown, with the rune id and the weapon type of the slot:

```
ImbuementHelper.GetNameKey(RuneId.Nova, WeaponType.MMES)          item_rune_nova_mmes
ImbuementHelper.GetDescriptionKey(RuneId.Nova, WeaponType.MMES)   item_rune_nova_desc_mmes
```

Add the texts in your mod with Jotunn (`CustomLocalization.AddTranslation`), using these two methods for the keys. See `localization/LocaleEnglish.cs` in EarthStaffs. When a weapon has no text for a rune, the rune shows its default English name and description from Core (`ImbuementRune.m_defaultName` and `m_defaultDescription`), so the runes are readable before a staff mod has its own texts.

## Secondary attacks (nova, rain, summon, ...)

A secondary attack is a wrapper prefab whose attack is copied to the staff on equip (see `ModularMagic_IceStaffs`/`ModularMagic_EarthStaffs`). Two things need Core's help, so register each secondary attack once in `Awake`, after the config is bound:

```csharp
SecondaryAttackHelper.Register(
    "attack_nova_MMIS",                 // the name of the projectile prefab the attack spawns, unique for this attack
    "secondary_nova_MMISCooldownStatusEffect", // the unique name of the cooldown status effect (null: no cooldown)
    "Frost nova cooldown",              // what the player sees
    () => PluginConfig.secondaryAttackNova.cooldown.Value, // the cooldown in seconds, read every time it starts
    (character, attack) => true);       // optional: block the attack (return false) or set costs on this attack
```

- **Eitr cost:** the game charges every secondary attack the eitr cost of the main attack. For a registered attack Core uses the cost of the attack itself (with the skill discount). One `Attack.GetAttackEitr` postfix in Core does this for all staff mods.
- **Cooldown:** when a cooldown is given, a registered attack can not start while the status effect is active ("The staff is still recharging!"). The cooldown starts when the attack can really start (not without enough eitr), lasts the number of seconds from the callback, and shows the icon of the staff that cast it. The same cooldown is used by every staff that has the attack.
- **Pays per burst:** for a Projectile attack with `m_perBurstResourceUsage` the game checks and pays the stamina, eitr and health when the burst fires, and stops the attack without paying or firing when one is missing (a health cost then never takes the last health). Core starts the cooldown of such an attack when the burst fired, so a stopped attack starts no cooldown.
- **`prepareStart`:** called before the attack starts, after the cooldown check, with the copy of the attack the character uses for this one attack (`Humanoid.StartAttack` clones it). Return false to block it (show the reason yourself, it costs nothing and starts no cooldown), or set a cost on it, for example `m_attackHealth` from the maximum health (the game's `m_attackHealthPercentage` is a part of the current health).
- **Recognised by the projectile name**, not by a number in Unity. An attack that is not registered is left alone. A cooldown effect is created by `Register`, a mod does not have to create status effects.

## Rules for effects that work in multiplayer

- **Apply on top of base stats, and be repeatable.** The effects are applied every time the staff is equipped. Start from the base stats, not from the item's current values, and read those base stats from the config on every equip (do not copy them at startup). `ItemDataSnapShot` is a config-backed example. Values a server syncs after the game started are then used too.
- **Never edit the prefab's attack or shared data at equip time.** Copy the attack first (`Attack.Clone()`), change the copy and assign it to the equipped item. Assigning another item's `Attack` object shares it, and later changes leak into other staffs.
- **A different look means a different prefab.** Networked objects (projectiles, spawned things) are found on other clients by the name of the prefab. Create a variant per look at startup with a fixed name, register it (`PrefabManager.AddPrefab`) on every client, and point the attack at it. Do not toggle children of a shared prefab per client, other players will not see it. See `ProjectileHelper` in EarthStaffs.
- **Only touch the ZDO as its owner.** The table already handles this for its own data.
- **The equipping client decides.** Damage is built on the attacking client, so the effects only have to be applied on the client that equips the staff.

## Public API

Everything in the namespaces `ModularMagic_Core.Components`, `.Helpers`, `.Models` and `.Types` that is listed here is public. The rest of Core is internal.

- **`ImbuementSlots`** (component): `m_slots`, `m_tier`, `m_weaponType`, `m_allowedRunes`, `m_colorPreset`, `IsRuneAllowed(runeId)`.
- **`ImbuementHelper`** (static):
  - `HasImbuements(ItemData)`: the weapon has an `ImbuementSlots` component.
  - `Read(ItemData)`: the runes of the weapon, one `Imbuement` per slot.
  - `GetLevel(ItemData, type)`: the level of the rune of that imbuement type on the weapon, 0 without one.
  - `GetSlots(ItemData)`: the `ImbuementSlots` of the weapon, or null.
  - `Serialize` / `Deserialize`: the saved string, see below.
  - `FindRune(id)` / `RegisterRune(rune)`: the rune registry (by id), Core fills it at startup.
  - `GetNameKey(runeId, weaponType)` / `GetDescriptionKey(runeId, weaponType)`: the translation keys of the rune texts.
  - `IsRune(ItemDrop)`: the item is a rune.
  - `DropRune(rune, level, position, rotation)`: drops a rune item with the level as its quality.
  - `DataKey`, `EditorKey`, `DraftKey`: the custom data keys.
  - `GetEditor`, `GetDraft`, `SetEditor`, `SetDraft`, `GetUnsavedRunes`, `LoadItemFromZDO`: used by the table, most staff mods do not need them.
- **`Imbuement`** (one slot): `type`, `value`, `name`, `description`, `prefab`, `level`, `runeId`, `tier`, `weaponType`, `rune`. `name` and `description` are localized when read.
- **`ImbuementRune`** (component on every rune prefab): `m_id`, `m_type`, `m_value`, `m_maxLevel`, `m_tiers`, `m_defaultName`, `m_defaultDescription`, `GetRequiredTier(level)`, `IsValidLevel(level)`, `GetName(weaponType)`, `GetDescription(weaponType)`.
- **`ItemUpgradeHelper`** (static): the event `OnUpgraded(from, to)`, called after an item was upgraded at a crafting station, see [Saved data](#saved-data).
- **`ImbuementType`**, **`WeaponType`**, **`RuneId`**: `const string`s, so they work in `case` labels.
- **`LightColorPresetType`**: the names for `m_colorPreset`.
- **`SecondaryAttackHelper`** (static): `Register(projectileName, cooldownEffectName, cooldownDisplayName, cooldownSeconds, prepareStart)`, see [Secondary attacks](#secondary-attacks-nova-rain-summon-).
- **The staff framework**: `StaffDefinition` (with `AddSecondaryAttack` and `AddMainAttackAnimation`), `SecondaryAttackEntry` and `MainAttackAnimationEntry` (models), `StaffImbuementHelper` (`Register`, `Apply`), `ImbuedStaffHelper`, `AttackSpeedHelper`, `SpawnedProjectileHelper`, `SpawnedCreatureHelper`, `TerrainOpRegistry`, `DamageHelper.GetMainPhysical`, `StaffHelper.CreateStaff`, `UpdateHelper` and `RecipeHelper`, see [The staff framework](#the-staff-framework-the-way-to-add-a-staff-mod).
- **The shared config classes** (`ModularMagic_Core.Configs`): `StaffConfig`, `SecondaryAttackConfig`, `MainAttackConfig` and `ImbuementConfig`, with their options in `Models` (`StaffConfigOptions`, `SecondaryAttackConfigOptions`, `MainAttackConfigOptions`, `ImbuementConfigOptions`, `UpdateItemDataOptions`, ...) and `SecondaryAttackType` in `Types`.

## Saved data

The runes are stored in the custom data of the item under `Imbuements_MMC`:

```
1:DamageSlash@2,,EitrCost@1
```

The version, a colon, and one entry per slot separated by commas. An entry is `runeId@level` and an empty slot is empty. The slot count, slot tier and weapon type come from `ImbuementSlots`, not from the string.

- An unknown version, or the old `|` format, loads as empty slots and logs a warning.
- An entry for a rune that does not exist, or with a level the rune does not have, is skipped and logged.
- Other keys on the same item: `MMC_Editor` (the player id of the editor) and `MMC_Draft` (the unsaved changes, same format).
- An upgrade at a crafting station replaces the item with a new one without its custom data (`InventoryGui.DoCrafting`). Core copies `Imbuements_MMC` onto the new item; the editor and draft only exist on the rune table, so they are not copied. A mod that keeps its own data on an item copies it in `ItemUpgradeHelper.OnUpgraded(from, to)`, which runs after the runes are copied.

## Adding a rune (in Core)

1. Add one rune prefab to the asset bundle. It has an `attach` child with one model per level (`rune_1`, `rune_2`, ...), the number of models is the number of levels. Register it in `ModularMagic_Core.InitAssetBundle` with `AddRunePrefab("<prefab name>")` and add a field for it to `CustomPrefabs`. The name must be fixed and the same on every client. A prefab that is not in the asset bundle is logged and its rune is left out (add the entry with `RuneData.Add`), so the other runes still load.
2. Add the id to `RuneId` and a `RuneEntryOptions` block in the matching `data/Rune*Data.cs` with the `id` (no `:`, `,` or `@`, and unique), `type`, `value`, the English `name` and `description` (also the default text of the rune), the crafting `recipe`, the `recipeUpgrade` (the items of an upgrade, multiplied per level) and `tiers` (the slot tier a level needs, one entry per level).
3. Add the id to `m_allowedRunes` of every weapon that should support it, and add its name and description in that staff mod's texts.
4. Handle the new `type` or `value` in every staff mod that should support it.

At startup Core sets the maximum quality of the item to the number of models, renders an icon per level, and logs a warning for a rune with a missing or invalid id, a duplicate id, or a different number of tiers than models. Look for `[Imbuements]` in the BepInEx log.

## How the table works with more than one player

- Every client attaches the table when the stand shows a staff (a postfix on `ItemStand.SetVisualItem`). From then on the table follows the ZDO of the stand: `RuneTable.Update` compares `DataRevision` and updates the runes when it changes. Changes reach other players in about 150 ms (66 to 144 ms measured), and a player who arrives later reads the current state right away.
- Only the editor can change anything. The editor claims the ownership of the stand and writes the draft and the saved runes into the ZDO itself, there are no RPCs. Everyone else sees the same runes read-only with a lock message, including the editor's unsaved changes (the draft). To release a staff, take it off the stand and place it again.
- Other players see what the editor sees: the runes stay in their slots, and a change plays the same animation (mark for removal, reset, reset with kick) without the runes flying out and in. The editor's client drops the runes, and the dropped items are networked.
- Unsaved runes are kept in the draft. A rune that is taken out of a slot, or that is still unsaved when the staff leaves the stand, drops from its slot.

This has been tested in single player and with two players on a dedicated server, including the quality-based runes.
