# Legendary Weapons

A mod that adds 3 new weapons to the game with different weapon modes! While holding the weapon you can press the weapon mode key (Default: Y) to change the weapon mode. This will change the secondary attack on the weapon. The weapon stats and recipes are configurable or can be individually disabled!

## Configuration
The mod comes with a config, be sure to check wether the config has changed in future updates:
- **Enable**: Enable the mod. Default: *true*
- **Weapon mode key**: Key to change the weaponmode (applies to all weapons). Default: *Y*

The following options are available for each weapon:
- **Name**: The name given to the item
- **Description**: The description given to the item
- **CraftingStation**: The crafting station the item can be created in. Default: *Forge*
- **Required station level**: The required station level to craft the item. Default: *1*
- **Crafting Costs**: The items required to craft the item
- **Upgrade Costs**: The costs to upgrade the item
- **Upgrade multiplier**: The multiplier applied to the upgrade costs. Default: *1*
- **Max quality**: The maximum quality the item can become Default: *4*
- **Movement speed**: The movement speed stat on the item
- **Damage multiplier**: Multiplier to adjust the damage on the item. Default: *1*
- **Block armor**: The block armor on the item
- **Block force**: The block force on the item
- **Knockback**: The knockback on the item (Some weapons have multiple of this)
- **Backstab**: The backstab on the item
- **Attack stamina**: Normal attack stamina usage
- **Secondary (weaponmode) ability stamina**: The secondary attack stamina usage (Some weapons have multiple of this)

## Roadmap
There are still some features planned to be implemented:
- Improve AssetBundle size even more

## Bug/Suggestion
Found a bug or have some suggestions? You can leave a post or bug report here: https://www.nexusmods.com/valheim/mods/2522/

## Screenshots
![Hammer_1](https://robhost.nl/img/valheim/Hammer_1.jpg)
![Hammer_3](https://robhost.nl/img/valheim/Hammer_2.jpg)
![Hammer_3](https://robhost.nl/img/valheim/Hammer_3.jpg)

![Atgeir_1](https://robhost.nl/img/valheim/Atgeir_1.jpg)
![Atgeir_1](https://robhost.nl/img/valheim/Atgeir_2.jpg)

![Sword_1](https://robhost.nl/img/valheim/Sword_1.jpg)
![Sword_1](https://robhost.nl/img/valheim/Sword_2.jpg)
![Sword_1](https://robhost.nl/img/valheim/Sword_3.jpg)

## Changelog
### 1.0.2
- Enabled client/server requiring the mod and same version
- Added Filewatcher to watch for config changes & tested ServerSync, config changes should synchronize and update in game
- The weapon mode key config entry should no longer require you to be admin
- Fixed recipe for TriSword (Tresverd)
- Added more mocks to prefabs to decrease the amount of files in the AssetBundle

<details>
    <summary>Click to view previous versions</summary>
    <!-- have to be followed by an empty line! -->

### 1.0.1
- Fixed internal name to be unique from JotunnModStub causing issues with my other mods

### 1.0.0
- First release

  </details>
</details>
