### 0.0.4
- Added the shared staff framework: a staff mod describes its staffs in a StaffDefinition and Core applies the runes, the attack speed animation and the summon scaling
- Moved the config classes of the staffs (staff, secondary attack, main attack, imbuement), the update helpers and the recipe helper into Core, with hooks for equipping a staff, the Rain projectiles, summoned creatures and custom terrain ops
- The recipe helper now supports the recipe amount and recipe snapshots, and is public
- The cone and nova can now hit other players when PvP is enabled
- The staff config can now also bind a slash damage per level, a backstab and an attack stamina cost entry, and the shared item data update sets the stamina cost and the fire, frost and lightning damage per level (used by the Blood Magic scythes); added the MMBM weapon type
- Added SpawnedCreatureHelper.SetDamage, which sets the damage of a summoned creature from a baseline attack (used by the Blood Magic apparitions), and the summon scaling of the staffs now uses it
- A mod can now set the default of the "Eitr Cost Rune" config entry (ImbuementConfigOptions.eitrCost), the staffs keep 0.25
- New runes, for now only allowed on the Blood Magic scythes: Blood Tap (secondary attack), Greatsword (main attack), Range and Legion (two new imbuement types, AttackRange and SummonLimit). A rune whose prefab is missing from the asset bundle is logged and left out instead of breaking the other runes
- StaffDefinition.AddMainAttackAnimation: a main attack rune that keeps the attack of the staff and only changes its animation, the hits of its chain and how the staff is held. Core sets the pose of the character again when the runes change how it holds the staff
- ImbuementConfig can bind a "Range Rune" and a "Legion Rune" entry (ImbuementConfigOptions.attackRange and summonLimit), and ImbuementHelper.GetLevel returns the level of a rune type on a weapon
- Fixed the runes of a staff being lost when it is upgraded at a crafting station. ItemUpgradeHelper.OnUpgraded lets a mod move its own data onto the upgraded item
- SecondaryAttackHelper.Register takes a prepareStart callback that can block a secondary attack or set its costs, and a secondary attack that pays per burst starts its cooldown only when the burst fired

### 0.0.3
- First damage/recipe balance and Weapons work with RuneTable

### 0.0.2
- Rune table changes now reach other players right away, without replaying the rune animations
- Fixed the zip package so uploads are no longer rejected for an unsafe entry path

### 0.0.1
- Initial release
