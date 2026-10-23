### 1.0.3
---
#### General
- Added a death counter at the top of the screen that shows how many times the streamer has died. It has its own "Death counter" section in the Settings tab, with a "Show death counter" setting to turn it off.
- Added a "Show top death causers" setting (off by default) that lists the three viewers who caused the most deaths in the current world under the death counter, in gold, silver and bronze.
- Added a "Show total death count" setting (on by default). Turn it off to count only the deaths in the current world instead of all deaths of the character.
- Leaderboards now track every death of the streamer per world (not only the ones a viewer caused) and show it in the footer, e.g. "5 deaths caused (12 total)". Worlds played before this update start from the deaths viewers caused.
- The Twitch chat connection is now encrypted (TLS). Before, your Twitch access token was sent in plain text when chat connected. Chat is also read and sent in the background now, so a bad connection can no longer freeze the game, and chat reconnects by itself when the connection drops (it used to stay dead until you logged in again). Chat commands are also recognised more strictly: a viewer typing certain words (like a message in capitals containing "PING") can no longer drop their own message or break the chat connection.
- Logging in to Twitch no longer goes through the Twitch SDK. The mod now logs in itself with the same "authorize in your browser" page, so the first login after starting the game can no longer do nothing. The login is never saved: restarting the game always starts logged out. Logging out also ends the login on Twitch, Twitch is asked every hour whether the login still stands (disconnecting the app on twitch.tv logs you out within the hour), and the login now only asks for the permissions it uses (no bits and no subscriptions).
- The mod no longer includes the Twitch SDK or its native library (R66_core.dll), so nothing native is needed for Twitch anymore. The mod now also ships its debug file (WizshBoneTwitchIntegration.pdb).
- Removed the "Quick test redeem key" setting and the WBTISetQuickRedeem command. Redeems can still be tested with the Test button in the Redeems tab and the WBTIUseRedeem command.

#### Twitch Ward
- Added a "Max Twitch Wards" setting (0 means unlimited). Once the limit is reached, placing another ward is blocked with a message. Existing wards are kept if you lower the limit. On a dedicated server a client only counts the wards near it, so it can place more than the limit there.
- Added a "Show safe zone message" setting (on by default) for the "Streamer is in a safe zone!" message.
- The placement ghost of the ward no longer acts as a safe zone while you are positioning it.
- The Settings tab now pairs "Twitch Ward buildable" with "Max Twitch Wards" and "Twitch Ward recipe" with "Show safe zone message", and "Show death counter" with "Show top death causers".

#### Monsters
- "Mute idle sound" is now called "Mute sounds" and also mutes the creature's alert sound (like a roar when it notices you). Creatures in existing redeems that have it enabled will now be silent on alert too.

#### Redeems
- Added the "Slippery Ground" status effect (pick it in a Status Effect redeem): the streamer slides on any terrain like on the Deep North ice sheets for the effect's duration. Boats and building floors keep their normal grip, and ice shoes still prevent sliding.
- Added the `WBTIRemoveTerrainEdit` console command to undo Twitch terrain edits and restore the original terrain. Permanent terrain edits (no duration) can now be undone too, but only edits made from this version on. Timed terrain edits now also reset their whole area for Rectangle and Chevron shapes.
- Fixed redeems showing up on Twitch but not being handled in the game after a Twitch session dropped. The connection for redemptions is now closed on every logout (including a lost game-server connection) and opened fresh on login, so logging out and back in fixes it.
- Redeems can no longer be enabled while you are logged out of Twitch, or before your Twitch account info has loaded. The Home tab shows a message instead. This also makes sure the check against Twitch's 50 reward limit is never skipped.
- The Home tab and the HUD now show redeems as disabled after you log out of Twitch yourself, so they no longer say enabled when nothing is on Twitch.
- Editing a redeem (name, description, points, cooldown, limits), changing the redeem prefix or changing the discount no longer deletes and re-creates its channel point reward on Twitch. The reward is edited in place and keeps its Twitch ID, so redemptions that are still pending on it can still be completed or refunded. Disabling a redeem (or a global key locking it) still removes its reward from Twitch, and enabling it again creates a new one.
- A redeem that costs 0 points is now sent to Twitch as 1 point, since Twitch's minimum is 1.
- When your redeems plus your other Twitch rewards would go over Twitch's limit of 50 (for example after a boss kill unlocks more redeems), nothing is changed on Twitch and a red "Redeems need fixing!" message stays on screen (above the safe zone one, if both apply) until you have fixed it. The Redeems tab shows how many slots you are short and a Retry button, to push again once you have freed up slots.
- The Twitch slots counter in the Redeems tab now warns (orange, with the details in its tooltip) when a boss or kill you haven't reached yet would take you to 40 or more slots, and shows it in red when it would go over the limit.
- Changes to the rewards on Twitch are now made one at a time. Several quick changes (for example disabling a few redeems in a row) are combined into one update, and changing the redeem prefix updates the rewards a moment after you stop typing, keeping their Twitch IDs.
- Every redeem now has a hidden, permanent `id` in `profile.yaml` (added automatically, written the next time the profile is saved). A redeem is linked to its reward on Twitch by that id, so renaming it or changing the prefix cannot split it from its reward, and a redemption is matched to its redeem by the reward even while Twitch still shows an old title. Copies of a redeem get an id of their own.
- The title of a redeem is now limited to 38 characters and its description to 200, so what is on Twitch always matches what you typed (Twitch cuts titles at 45 characters, and the longest prefix takes 7 of them). A longer title of an existing redeem is shortened when you open it in the editor.
- Two redeems can no longer have titles that only differ in capitals, like "Rain" and "rain". A redeem that is left out of the update because its title is empty or a duplicate is now reported with an error. Redemptions are also matched to their redeem ignoring capitals.
- Logging out or quitting the game now waits until your redeems are removed from Twitch. If some cannot be removed (for example without a connection) a dialog lets you retry or continue anyway; anything left is cleaned up the next time you enable redeems.
- The safe zone message and the new "Redeems need fixing!" message share one on-screen label and are shown together, most important first. The Twitch slots tooltip shows "Log in" instead of an unknown count while you are logged out.
- Redeems are now received through our own connection to Twitch instead of the Twitch SDK. If the connection drops it reconnects by itself, and redeems that were made in the meantime are picked up when it is back.
- Completing or refunding a redeem (also automatically) now only shows as done after Twitch has accepted it. Before, a refund that did not get through still showed as refunded while the viewer kept the points. When Twitch does not accept it (for example without a connection) the redeem stays in the history, with a message, so you can try again. Automatic completes and refunds are tried a second time after a few seconds, and a redeem that still failed keeps counting as waiting in the logout and disable warnings, also when auto-resolve is on. The chat message that tells a viewer their redeem was refunded is now sent after Twitch has done the refund; if the refund fails, chat says the streamer will refund it by hand.
- The "Reject when paused" setting is now called "Reject when in menus" and also rejects (and refunds) redeems while the WizshBone window is open, not only while the game is paused or the game menu is open. It was never covered before, because that window does not pause the game.

### 1.0.2
---
#### General
- Fixed the login flow (single authorize).
- Fixed the session-expiry warning at the 15, 10, 5 and 1 minute login checks. Once expired, you are logged out and the redeems are removed from Twitch.
- Added a first version of leaderboards: tracks viewers, points, redeem count, favorite redeems, previous names and caused deaths.
- Added a card on the Home tab to rebind the key that opens the main window, with a Reset button.
- Creating a profile can now start empty or based on another profile.
- Fixed a bug where scrolling zoomed the camera while the UI was open.
- Fixed a bug that could leave toasts stuck forever, and added a Clear Toasts button in the Help tab.
- Added built-in profiles "Normal" and "Chatting only". They always exist (they are re-created if missing) and can't be deleted, renamed or synced, but a Reset button puts one back to its original redeems and settings. An existing profile of one of these names is renamed to "name (old)" instead of being overwritten. "Normal" replaces the old "Default" profile, which is kept as a regular profile if you already have it.

#### Redeems
- Fixed the "Enable redeems" and "Chatting" cards on the Home tab not updating live (when redeems are enabled on login).
- New toggleable settings to enable/disable auto reject and refund redeems while the game is paused or a menu is open, while the streamer is sleeping, or while teleporting (all on by default).
- A redeem that arrives while the streamer isn't in the world (dead, respawning or loading) is now refunded instead of being lost.
- Redeems that are not in the active profile are no longer discarded: they stay unresolved in the Redeem history, shown in red as "Unknown: title", and bulk Complete/Refund skips them.
- The Redeem history now holds 1000 entries (was 500), warns when 750 unresolved redeems are pending, and drops the oldest resolved entry first so pending redeems are kept.
- Disabled redeems are no longer sent to Twitch, so they don't use up one of its 50 channel point reward slots.
- The Redeems tab now shows how many Twitch reward slots you use, including rewards on Twitch at that moment. Redeems are not pushed (and an error is shown) when they would go over Twitch's limit of 50, and Twitch rejections are now reported.
- The Redeems tab's "Test" button is now called "Trigger", with a clearer tooltip.

#### Monsters
- Fixed recolors deviating from the entered HEX value.
- Rockies can now be claimed with `claim:`, can talk and can be recolored.
- Added a second (emission) color for recolors! Now you can recolor the skin and glowing parts separately!
- Added recolors for the new Skeletons, Bat, Writhan and up to Ashlands. Also the bosses from Eikthyr up to The Queen can now be recolored!
- Fixed starred creatures (e.g. one-star deer, starred wolves) not showing their recolor on the skin.
- The color picker applies on change, accepts paste and strips a leading `#`. Click a color in the Viewer list to copy its hex code.
- Resized creatures now scale their attack reach, hitbox and AI attack range with their size. Covers melee swings, area attacks (stomps and slams) and where projectiles spawn.
- Death effects of resized creatures (ragdolls etc.) now spawn at the creature's size, and recolored creatures leave a recolored ragdoll.
- Gear held or worn by resized creatures now scales with them.
- MANY new creature form fields! Some are experimental and might not work as intended, like setting equipment, but I do encourage you to experiment and report any feedback!

#### Surprise chest redeem
- "Stack size" is now "Amount" (existing profiles are migrated automatically), limited to the item's max stack size.
- Quality is limited to 1-10 and can go above the item's normal max quality, like extra Refinement Forge levels.
- Items now spawn with full durability for their quality.
- A redeem is now refunded if the dungeon filter and the Max spawned limits leave nothing the chest can spawn.

#### Weather redeem
- Added the `WBTIRemoveWeather` console command to remove Twitch weather zones and restore normal weather.
- New "Cycle weathers" option steps through the chosen weathers in order, with a "Time per weather" setting, instead of picking one at random.
- New "Follow player" option makes the weather zone follow the redeemer.
- New "Frost resistance" option protects anyone standing in the zone from cold weather damage (e.g. while cycling through snow).
- Weather zones now sync to other players in multiplayer (weather, force flag and size are replicated).

#### Flashbang redeem
- Flashbang: nearby players are flashed too when the streamer throws one, using their own Flashbang redeem settings.

#### Status effect redeem
- Grow/shrink player resizes are now visible to other players in multiplayer. Fixed swim/wade speed and interact distances for resized players.

#### Safezones
- New "Twitch Ward buildable" setting: turn it off to hide the ward from the Hammer build menu.
- Safezones: spawned creatures are pushed out harder, no longer pick destinations inside a zone, and, while monsters are pushed away, their projectiles and hits can no longer enter or hit players inside a zone.
- Fixed the ward's burn leaving spawned creatures with a little health left.
- Safezone radius changes now sync to all players, and the radius is limited to 3-1000.

#### HUD
- Restored the missing HUD section in Settings.
- Status HUD now defaults to below the minimap.
- Status HUD now shows the configured key for opening the main UI.
- Status HUD now shows "Twitch:" and "Live:" labels, each with a green/red status dot.
---

### 1.0.1
- Clamped number settings.
- Removed leftover debug log messages, including chat message logging.

### 1.0.0
- Initial release.
