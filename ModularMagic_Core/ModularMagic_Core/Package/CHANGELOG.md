### 0.0.4
- Added the shared staff framework: a staff mod describes its staffs in a StaffDefinition and Core applies the runes, the attack speed animation and the summon scaling
- Moved the config classes of the staffs (staff, secondary attack, main attack, imbuement), the update helpers and the recipe helper into Core, with hooks for equipping a staff, the Rain projectiles, summoned creatures and custom terrain ops
- The recipe helper now supports the recipe amount and recipe snapshots, and is public
- The cone and nova can now hit other players when PvP is enabled

### 0.0.3
- First damage/recipe balance and Weapons work with RuneTable

### 0.0.2
- Rune table changes now reach other players right away, without replaying the rune animations
- Fixed the zip package so uploads are no longer rejected for an unsafe entry path

### 0.0.1
- Initial release
