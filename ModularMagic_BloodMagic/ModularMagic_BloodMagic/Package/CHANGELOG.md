### 0.0.3
- The scythes now use the shared staff framework of ModularMagic_Core: they are imbuable on the rune table, have a max quality of 4 and use the shared staff config entries
- The scythes can hold the Attack Speed rune (a faster swing) and the Eitr Cost rune (less eitr per charging hit: 0.75 per level by default, so 3 at level 4)
- A scythe charges faster while you have few apparitions: 3 times as fast without any, back to the normal rate at 3. The charge per hit, the multiplier and the eitr per charging hit (10) are config entries in the new "Soul Reaper" section
- The number of apparitions is now counted per player: 3 with a scythe of quality 1 and one more for every quality level. Wild Charred and the apparitions of other players no longer count. A full scythe stays full until you have a free slot
- An apparition can no longer be killed. Without health it is banished: it stays where it is as a husk, does nothing and makes no sound, until its summoner uses it and pays health (20% of the maximum health by default) to reanimate it
- A banished apparition can only be reanimated while your active apparitions (banished ones not counted) stay within the maximum of the scythe that summoned it
- Reanimating a banished apparition plays an effect, and it returns 1.7 seconds later with a red glow that turns back to its own colour
- Only the summoner can hit a husk, and a hit kills it, so you can clear your apparitions and free their slots. Like any tamed creature, this needs PvP on or the Butcher knife
- An apparition that is banished makes a sound as it falls still
- An apparition that is banished slows to a stop in a second while its particles and glow fade, and the pose it stops in is saved, so every player sees the same pose, also after a relog
- An enemy that was attacking an apparition leaves it alone as soon as it is banished
- A banished apparition can no longer be pushed, and other creatures walk around it
- Your banished apparitions are marked on the map, also after a relog, until they are reanimated or killed
- New status effect Foul Magic Exhaustion: every active apparition lowers the damage of all your attacks by 10%, up to 50% (config entries). A banished apparition does not count
- The default health of the apparitions is doubled to 280, 980 and 1680 (an existing config file keeps its values)
- A hit that can not add charge (the scythe is already full) no longer costs eitr, and hitting a tamed creature no longer charges the scythe
- Fixed a damaged apparition getting its full health back every time it was loaded again
- Config entries renamed (their values reset): "Recipe" is now "Crafting costs", "Upgrade recipe" is "Upgrade costs", "Upgrade recipe multiplier" is "Upgrade multiplier", "Required station level" is "Required station level to craft", "Slash damage" is "Attack damage slash" and "Slash damage per level" is "Attack damage slash per level"
- Removed the Simple Reaper (the meadows scythe)
- A scythe is now identified by its prefab name, so renaming a scythe in the config no longer breaks charging and the apparitions
- The damage of the apparitions now comes from the scythe that spawned them (quality included): the new "Damage multiplier" config entry is the share of the scythe's damage they deal (0.6 is 60%). Their hardest hit deals that much, their weaker attacks less. The old "Damage" entries (a fixed value) are replaced and their values reset
- The in-game config window (ConfigurationManager) shows under each "Damage multiplier" what the hardest hit of the apparitions deals at quality 1 and at the max quality, and updates it as you change the multiplier or the damage of the scythe
- The smaller apparitions now attack from their own height: their hits, reach and arrows are scaled with their size
- New config entry "Despawn when away" (Soul Reaper): turn it off and apparitions are no longer removed when you go far away or log out
- The Blood Magic skill now raises the damage of the apparitions (the spawn ability of the scythe already had the factor, but it was never applied)
- The apparitions are now spawned by the game's own SpawnAbility instead of a copy of it

### 0.0.2
- The recipe helper is now shared through ModularMagic_Core

### 0.0.1
- Initial release
