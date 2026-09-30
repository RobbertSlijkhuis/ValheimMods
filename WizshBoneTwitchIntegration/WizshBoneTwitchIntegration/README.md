# WizshBone Twitch Integration

Let your Twitch chat turn your Valheim world into chaos! WizshBone Twitch Integration connects your
channel to the game: viewers can spend channel points on redeems that trigger effects around you, and chatters can even take control of the creatures in your world!

## Getting started

1. Install the mod and its dependencies.
2. Start Valheim, load into a world and open the mod's window (default F3).
3. Click **Twitch Login** in the top bar. Your main browser opens, where you log in to Twitch and authorize the mod. (You need to do this twice currently seperate for the mod and chat)
4. Turn on redeems from the Home tab. Your redeems are created as custom channel point rewards on your channel.

Twitch only offers channel points (and custom rewards) to Affiliates and Partners, and allows at most 50 custom rewards per channel, including any you already have.

## Features

### Channel point redeems
Every redeem triggers an in-game effect around the streamer. The redeem wizard walks you through
choosing an effect, configuring it and setting up the channel point reward. Available effects:

- **Spawn creature(s)** from a creature group you define
- **Surprise chest** with random loot or unpleasant surprises!
- **Status effect** applied to you, positive or negative
- **Weather** changes for the vibes!
- **Time stop**, freezing creatures and objects around you
- **Terrain edit**, spawning sinkholes to trap the unsuspecting streamer
- **Smite** (lightning), **Meteors**, **Roots**, **Traps**
- **Fish rain**, **Log rain** and **Boat rain**
- **Doors of Doom** and **Windmills of Death**
- **Mist**, **Flashbang** and **Detonate fish**

A redeem requires at least a point cost and a title. You can also add cooldowns, and make redeems appear or disappear as you progress and defeat bosses. You can apply a percentage discount to all redeem costs, and choose whether redeems are resolved automatically
or by hand from the redeem history.

### Chatting and claiming creatures
- Chat messages can show up above nearby creatures.
- Viewers are offered nearby creatures to claim with `!claim`, either to a randomly chosen chatter
  or as a free-for-all where the first to type it wins.
- Claimed creatures carry the viewer's name and can be released with `!unclaim`.

### Viewers
Register the chatters and redeemers you care about to give them perks, like a creature color.

### Profiles
A profile holds all your redeems and settings. Switch, copy, rename, import and
export profiles to share a setup, or sync a profile so you and your friends run the exact same
configuration. Synced profiles are read-only for everyone except the owner.

### Settings
Everything below is configured per profile and saves automatically:
- Chatting: claim duration, scan interval and radius, blacklist, free-for-all claiming
- Creatures: same faction, scaling by biome tier, spawn limits, follow radius
- Indestructible boats, chests, portals and crops
- Twitch Ward recipe and behavior
- Safezones on traders and boats

### Safezones
Safezones (Twitch Wards, ships and traders) block redeem effects while you are inside them, unless a
redeem is set to ignore safezones.

## Multiplayer

The mod is designed with multiplayer in mind, so redeem effects should work for everyone in the
world, not just the streamer. It has **NOT** been thoroughly tested in multiplayer yet, so some issues
can arise. Please report any problems you run into.

## Known issues

- Login requires you to authorize twice, once for the mod and once for chat. I am looking into this.
- Creature recoloring is not finished: bosses and creatures from the Ashlands onwards can't be recolored yet.
- Multiplayer has not been tested thoroughly (see above).

## Bugs & Suggestions

Found a bug or have a suggestion? Join my Discord: https://discord.gg/M3BrRskQEd  
Select the modding role and post your feedback or bug report there!

## Planned

- More default profiles with different levels of chaos and difficulty
- Leaderboards (who killed you the most, most used redeems, points spent and more)
- More redeem types, like dropping your inventory, inverted movement and rearranging your hotbar
- A periodic vote that lets viewers decide your fate with a random redeem
- Surprise chests that can turn out to be a mimic or spawn other redeems
- A new cooldown system with shared cooldowns
- More commands for viewers to control the creatures they claimed or spawned

## Screenshots

Soon™
