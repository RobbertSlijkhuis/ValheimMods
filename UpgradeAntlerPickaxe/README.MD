# Upgrade Antler Pickaxe

A simple mod to allow upgrades for the Antler Pickaxe, just like any other pickaxe in the game.

## Configuration
The mod comes with a config, be sure to check wether the config has changed on future updates. The following options are available:
- **Enable**: Wether the Antler Pickaxe can be upgraded. Default: *true*
- **Create clone**: Wether the mod creates a new item (a clone) of the Antler Pickaxe or edits the original. Default: *false*
- **Name**: The name applied to the item. Default: *Antler Pickaxe+*
- **Description**: The description applied to the item. Default: (Same as original) *This tool is hard enough to crack even the most stubborn rocks.*
- **CraftingStation**: In which crafting station you can build the item. Default: *Workbench*
- **Crafting Costs**: The crafting cost to make the item. Default: *Wood:10,HardAntler:1*
- **Upgrade Costs**: The material cost to upgrade the item. Default: *Wood:4,HardAntler:1*
- **Upgrade multiplier**: The multiplier applied to the upgrade cost. Default: *1*

## Roadmap
There are currently no features planned

## Bug/Suggestion
Found a bug or have some suggestions? You can leave a post or bug report here: https://www.nexusmods.com/valheim/mods/2523/

## Changelog
### 1.1.2
- Enabled client/server requiring the mod and same version
- Added Filewatcher to watch for config changes & tested ServerSync, config changes should synchronize and update in game


<details>
    <summary>Click to view previous versions</summary>
    <!-- have to be followed by an empty line! -->

### 1.1.1
- Fixed internal name to be unique from JotunnModStub causing issues with my other mods

### 1.1.0
- Removed the weapons I made for my friends and created entire new ones in this mod https://valheim.thunderstore.io/package/DeathWizsh/LegendaryWeapons/. The prefab names are not the same, if you update to this version they will be removed!
- Added config entries to set the name, description, crafting station, crafting recipe, upgrade costs & being able to choose wether to edit the orignal Antler Pickaxe or to create a clone
- The plugin GUID has changed, please remove the old config & edit the new one (sorry :S) 

### 1.0.1
- Updated README to announce that the special weapons will be moved to a different mod in the future

### 1.0.0
- First release

  </details>
</details>