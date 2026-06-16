# WizshBoneTwitchIntegration

BepInEx plugin for Valheim that integrates Twitch channel point redemptions and chat into the game. Built on the Jötunn modding framework (.NET 4.8).

## Project scope

All work is within `WizshBoneTwitchIntegration/` (the C# plugin). The sibling `Glimuleikar/` project is off-limits unless explicitly asked — ask for permission before reading it.

## Key files and structure

```
WizshBoneTwitchIntegration/
├── WizshBoneTwitchIntegration.cs     # Plugin entry point (BepInPlugin, Harmony init, asset loading)
├── components/
│   ├── TwitchAuth.cs                 # OAuth flow with Twitch API
│   ├── TwitchCustomRewards.cs        # Receives redemptions, dispatches to handlers
│   └── TwitchChat.cs                 # In-game chat display
├── harmony/                          # Harmony patches on vanilla Valheim methods
├── helpers/                          # Game-effect utilities (spawn, damage, items, etc.)
├── models/                           # Custom prefabs, materials, sprites, effect lists
├── configs/PluginConfig.cs           # BepInEx config entries
├── commands/                         # In-game console commands (debug/management)
├── gui/                              # In-game UI panels
├── TwitchOAuth/                      # OAuth token management
├── TwitchSDK/                        # Wrapper around native TwitchSDK.dll
└── resources/                        # YAML schemas and redeem definitions
```

## How redemptions work

1. `TwitchCustomRewards.cs` receives a channel point redemption event from the Twitch SDK.
2. It looks up the redeem type and delegates to a helper in `helpers/`.
3. Helpers apply the in-game effect (spawn creature, modify terrain, deal damage, etc.).
4. Adding a new redeem type requires both a handler in C# **and** a corresponding entry in the YAML schema (`resources/`).

## Debugging

Use `Jotunn.Logger` (or BepInEx `Logger`) for debug output — prefer `LogWarning` so messages stand out in the BepInEx console/log without being noise-level info. Do not use `Debug.Log` directly.

```csharp
Jotunn.Logger.LogWarning($"[WBTI] SomeThing: {value}");
```

## Build and deploy

- Build in Visual Studio (solution: `WizshBoneTwitchIntegration.sln`).
- Set Valheim path in `Environment.props` before first build.
- `publish.ps1` copies the built DLL to the BepInEx plugins folder or packages for ThunderStore.
- No build step needed from Claude — the user builds and tests in-game.

## Testing

No automated tests. All testing is manual/in-game. Console commands in `commands/` are the primary debug tool.

## What NOT to do

- Do not build or run the project.
- Do not read or modify sibling projects (`Glimuleikar/`, `JotunnModStub/`) without explicit permission.
- Do not add `Debug.Log` calls — use `Jotunn.Logger.LogWarning` instead.
