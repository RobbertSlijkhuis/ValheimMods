# ModularMagic_Core: shared imbuement API

Core owns the rune table, the runes and the data that says which runes are on a weapon. A staff mod (EarthStaffs, and later Fire, Ice and Lightning) owns what the runes actually do to its staffs. This page explains how a staff mod plugs in.

`ModularMagic_EarthStaffs` (`helpers/EarthImbuementHelper.cs`, `ModularMagic_EarthStaffs.cs`, `harmony/PatchesMMES.cs`) is the reference implementation.

## Concepts

| Term | Meaning |
|---|---|
| **Rune** | An item you slot into a staff on the rune table. There is one item per rune **id**, for example `DamageSlash`. |
| **Level** | How strong a rune is. The level is the **quality** of the rune item (`ItemData.m_quality`, 1 up to the number of models the rune has), so a rune is upgraded like any item. A slotted rune is stored as its id and level. |
| **Slot** | One place on a staff that holds one rune. A staff has a number of slots, a slot **tier** and a **weapon type**. |
| **Tier** | A rune can only be slotted when the slot tier is at least the tier the rune needs for its level (`ImbuementRune.GetRequiredTier(level)`). |
| **Weapon type** | `WeaponType.MMES`, `MMFS`, `MMIS` or `MMLS`. It is the suffix of the rune texts of the weapon (see [Localization](#localization)). |
| **Supported runes** | The rune ids a weapon lists in `ImbuementSlots.m_allowedRunes` (see `RuneId`). The weapon decides this, a rune that is not listed can not be slotted on that weapon. |
| **Saved runes** | The runes stored on the staff item, written when the player applies the changes on the table. |
| **Draft** | The unsaved changes of the player who is editing. It is stored on the table, so it survives walking away and relogging. |
| **Editor** | The player who put the staff on the table. Only that player can edit it. |

## Setting up a staff mod

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
   ```

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
| `ProjectileBurst` | `ProjectileBurst` | `0.0125` | 1-4 | MMES, MMIS |
| `ProjectileSpeed` | `ProjectileVelocity` | `1` | 1-4 | MMES, MMIS |
| `Cone` | `MainAttack` | `Cone` | 1-4 | all four |
| `Creatures` | `SecondaryAttack` | `Summon` | 1-3 | all four |
| `Nova` | `SecondaryAttack` | `Nova` | 1-3 | all four |
| `Rain` | `SecondaryAttack` | `Rain` | 1-3 | MMES, MMFS, MMIS |

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

## Rules for effects that work in multiplayer

- **Apply on top of base stats, and be repeatable.** The effects are applied every time the staff is equipped. Start from the base stats, not from the item's current values, and read those base stats from the config on every equip (do not copy them at startup). EarthStaffs' `ItemDataSnapShot` is a config-backed example. Values a server syncs after the game started are then used too.
- **Never edit the prefab's attack or shared data at equip time.** Copy the attack first (`Attack.Clone()`), change the copy and assign it to the equipped item. Assigning another item's `Attack` object shares it, and later changes leak into other staffs.
- **A different look means a different prefab.** Networked objects (projectiles, spawned things) are found on other clients by the name of the prefab. Create a variant per look at startup with a fixed name, register it (`PrefabManager.AddPrefab`) on every client, and point the attack at it. Do not toggle children of a shared prefab per client, other players will not see it. See `ProjectileHelper` in EarthStaffs.
- **Only touch the ZDO as its owner.** The table already handles this for its own data.
- **The equipping client decides.** Damage is built on the attacking client, so the effects only have to be applied on the client that equips the staff.

## Public API

Everything in the namespaces `ModularMagic_Core.Components`, `.Helpers`, `.Models` and `.Types` that is listed here is public. The rest of Core is internal.

- **`ImbuementSlots`** (component): `m_slots`, `m_tier`, `m_weaponType`, `m_allowedRunes`, `IsRuneAllowed(runeId)`.
- **`ImbuementHelper`** (static):
  - `HasImbuements(ItemData)`: the weapon has an `ImbuementSlots` component.
  - `Read(ItemData)`: the runes of the weapon, one `Imbuement` per slot.
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
- **`ImbuementType`**, **`WeaponType`**, **`RuneId`**: `const string`s, so they work in `case` labels.

## Saved data

The runes are stored in the custom data of the item under `Imbuements_MMC`:

```
1:DamageSlash@2,,EitrCost@1
```

The version, a colon, and one entry per slot separated by commas. An entry is `runeId@level` and an empty slot is empty. The slot count, slot tier and weapon type come from `ImbuementSlots`, not from the string.

- An unknown version, or the old `|` format, loads as empty slots and logs a warning.
- An entry for a rune that does not exist, or with a level the rune does not have, is skipped and logged.
- Other keys on the same item: `MMC_Editor` (the player id of the editor) and `MMC_Draft` (the unsaved changes, same format).

## Adding a rune (in Core)

1. Add one rune prefab to the asset bundle. It has an `attach` child with one model per level (`rune_1`, `rune_2`, ...), the number of models is the number of levels. Register it in `ModularMagic_Core.InitAssetBundle` with `AddRunePrefab("<prefab name>")` and add a field for it to `CustomPrefabs`. The name must be fixed and the same on every client.
2. Add the id to `RuneId` and a `RuneEntryOptions` block in the matching `data/Rune*Data.cs` with the `id` (no `:`, `,` or `@`, and unique), `type`, `value`, the English `name` and `description` (also the default text of the rune), the crafting `recipe`, the `recipeUpgrade` (the items of an upgrade, multiplied per level) and `tiers` (the slot tier a level needs, one entry per level).
3. Add the id to `m_allowedRunes` of every weapon that should support it, and add its name and description in that staff mod's texts.
4. Handle the new `type` or `value` in every staff mod that should support it.

At startup Core sets the maximum quality of the item to the number of models, renders an icon per level, and logs a warning for a rune with a missing or invalid id, a duplicate id, or a different number of tiers than models. Look for `[Imbuements]` in the BepInEx log.

## How the table works with more than one player

- Every client builds the table from what is on the stand, through a postfix on `ItemStand.SetVisualItem`. A player who arrives later sees the runes within a few seconds.
- Only the editor can change anything. Everyone else sees the saved runes read-only with a lock message. To release a staff, take it off the stand and place it again.
- Saving and the draft are sent to the owner of the stand, which checks the editor and writes the data.
- Unsaved runes are kept in the draft. A rune that is taken out of a slot, or that is still unsaved when the staff leaves the stand, drops from its slot.

This has been tested in single player and with two players. The quality-based runes (levels as item quality) are written but not tested yet.
