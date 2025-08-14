# Splash Meads

A mod that adds (throwable) splash variants of the resistance (fire, frost, poison), Ratatosk, Vananidir, Anti-sting and health (minor, medium and major) meads. Create the items and simply throw them at friendlies to protect them!

## Now with HUD icons, timers and config options!

## A couple things to keep in mind. ##
- When a player already has the normal mead active (except health meads) the splash mead will not overwrite the status effect UNLESS the remaining time of the normal one is lower then the splash mead. Health meads will NEVER overwrite as they are cooldowns
- When a player already has a splash mead active and consumes a normal mead (except health meads as they are cooldowns) the splash mead will be overwritten (the effects don't stack anyway, but it cleans up your status effects)

## Configuration
The mod comes with a config, be sure to check wether the config has changed in future updates.

General options:
- **Show Particles**: Show particles when tames/players are affected by a splash mead. Default: *true*
- **Show Particles on players**: Show particles on players affected by a splash mead. Default: *true*
- **Show hud icons**: Show splash mead icons underneath healthbar of tames. Default: *true*
- **Icon size**: The hud icon size. Default: *30*
- **Show hud timers**: Show a timer on the icons. Default: *true*
- **Timer font size**: The font size of hud timers. Default: *16*
- **Timer format**: The format in which the timers are displayed (minutes:seconds or the entire duration in seconds). Default: *Minutes*
- **Timer alignment**: The alignment of the hud timers (topLeft, top, center etc.). Default: *Bottom*

The following options are available for every mead to configure:
- **Enable**: Wether the recipe for this item is enabled. Default: *true*
- **Name**: The name of the item. Default: *true*
- **Description**: The description of the item. Default: *true*
- **Crafting station**: The crafting station the item can be crafted in. Default: *Workbench*
- **Required station level**: The required station level to craft this item. Default: *3*
- **Recipe**: The items required to craft this item. Default: *[Mead]:6, LeatherScraps:3, Resin:2*
- **Amount crafted**: The amount of this item you'll get when crafting it. Default: *2*
- **Weight**: Weight of the item. Default: *1*
- **Max stack size**: The amount this item will stack. Default: *10*
- **Duration / Cooldown**: The duration or cooldown of the status effect (seconds). Default: *300 / 120*
- **Radius**: The radius of the splash and applying status effect (between 2-10). Default: *4*

## Roadmap
This mod was commissioned to add the three resistance meads, but more will be added in the future. The following is planned
- ~~Ratatosk~~ DONE
- ~~Vananidir~~ DONE
- ~~Anti-sting~~ DONE
- ~~health pots~~ DONE
- Revise particle effects
- Refactor to make adding new meads and features easier

## Known issues
- Currently the splash meads will phase through friendlies, keep that in mind when you throw one. I do not know if this is possible to fix as arrows also go through friendlies and this might be standard behaviour of the game.

## Bug/Suggestion
Found a bug or have some suggestions? You can find me in the Valheim Modding discord here: https://discord.gg/WC4x42KcBA.

## Screenshots
### Splashes: ###
![Barley wine splash](https://robhost.nl/img/valheim/splashmeads/BarleySplash.png)
![Frost resist splash](https://robhost.nl/img/valheim/splashmeads/FrostSplash.png
)
![Poison resist splash](https://robhost.nl/img/valheim/splashmeads/PoisonSplash.png)

### Hud icons: ###
![Hud icon Lox](https://robhost.nl/img/valheim/splashmeads/HudIconLox.png)
![Hud icons](https://robhost.nl/img/valheim/splashmeads/HudIcons.png)
![Hud icons default](https://robhost.nl/img/valheim/splashmeads/HudIconsDefault.png)
![Hud icons custom](https://robhost.nl/img/valheim/splashmeads/HudIconsCustom.png)

### Particle effects: ###
![Vananidir effect](https://robhost.nl/img/valheim/splashmeads/VananidirEffect.png)
![Ratatosk effect](https://robhost.nl/img/valheim/splashmeads/RatatoskEffect.png)
![Barley wine particles](https://robhost.nl/img/valheim/splashmeads/BarleyWineEffects.png)
![Frost resist particles](https://robhost.nl/img/valheim/splashmeads/FrostResistEffects.png)
![Poison resist particles](https://robhost.nl/img/valheim/splashmeads/PoisonResistEffects.png)