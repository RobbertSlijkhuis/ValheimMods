### 1.0.2
- Added leaderboards: tracks viewers, points, redeem count, favourite redeems, previous names and caused deaths.
- Fixed the login flow (single authorise, 4-hour token-expiry warning).
- Fixed a bug that could leave toasts stuck forever, and added a Clear Toasts button in the Help tab.
- Fixed starred creatures (e.g. one-star deer, starred wolves) not showing their recolour on the skin.
- Fixed recolours deviating from the entered HEX value.
- Colour picker: applies on change, accepts paste and strips a leading `#`; click a colour in the Viewer list to copy its hex code.
- Added a second (emission) colour for recolours, and recolours for Eikthyr through Yagluth.
- New creature form fields: position, name and emission colour.
- Rockies can now be claimed with `claim:`, can talk and can be recoloured.
- Scrolling no longer zooms the camera while the UI is open.
- Creating a profile can now start empty or based on another profile.
- Importing into a live profile now warns first.
- Status HUD now defaults to below the minimap.
- Status HUD now shows the configured key for opening the main UI.
- Status HUD now shows "Twitch:" and "Live:" labels, each with a green/red status dot.
- Restored the missing HUD section in Settings.
- Added an "Open window key" card on the Home tab to rebind the key that opens the main window, with a Reset button.
- Number fields now clamp to their limits immediately while typing, and out-of-range values are corrected when a form opens.
- Surprise chest items: "Stack size" is now "Amount" (existing profiles are migrated automatically), limited to the item's max stack size.
- Surprise chest items: Quality is limited to 1-10 and can go above the item's normal max quality, like extra Refinement Forge levels.
- Surprise chest items now spawn with full durability for their quality.
- Resized creatures now scale their attack reach, hitbox and AI attack range with their size: a shrunk creature no longer swings from out of reach and misses, and a grown one reaches further. Covers melee swings, area attacks (stomps and slams) and where projectiles spawn.
- Removed the unused "Aggravatable" creature option (existing profiles still load).
- Weather redeem: new "Cycle weathers" option steps through the chosen weathers in order, with a "Time per weather" setting, instead of picking one at random.
- Weather redeem: new "Follow player" option makes the weather zone follow the redeemer.
- Weather redeem: new "Frost resistance" option protects anyone standing in the zone from cold weather damage (e.g. while cycling through snow).
- Weather zones now sync to other players in multiplayer (weather, force flag and size are replicated).
- Added the `WBTIRemoveWeather` console command to remove Twitch weather zones and restore normal weather.
- Creatures can now be given gear: "Equip items" equips vanilla items on spawn, and "Remove equipment" strips parts of their default loadout.
- The creature form (spawn creature and surprise chest) is now split into General, Appearance, Behavior and Spawn tabs.
- Surprise chest: a redeem is now refunded if the dungeon filter and the Max spawned limits leave nothing the chest can spawn.
- Creature talk messages now use `{{user}}` (like all other messages) for the redeemer's name, for single and multiple (`;`-separated) messages. The old `{{userName}}` placeholder in talk messages is no longer replaced, so update any existing talk messages that use it.
- "Show safezone bounds" now also outlines weather zones, and draws capsule outlines (e.g. the ward zone) with rounded ends. Ship and trader outlines follow the safezone Boats/Traders settings.
- "Show safezone bounds" also shows a countdown above timed redeem objects (boats, weather zones, ...).
- Some form descriptions now highlight key words in yellow.
- Removed the unused "random", "isHallucination" and "index" creature fields (existing profiles still load).

### 1.0.1
- Clamped number settings.
- Removed leftover debug log messages, including chat message logging.

### 1.0.0
- Initial release.
