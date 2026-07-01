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

## Where logic belongs

- `harmony/` classes should contain **only** Harmony patch methods (the `[HarmonyPrefix]`/`[HarmonyPostfix]`/`[HarmonyPatch]` methods themselves). No state, no standalone effect logic.
- State and logic that a patch depends on (e.g. "is the player currently frozen") belongs in the corresponding `helpers/` class, exposed via a small public property/method the patch reads or calls.
- Most game-effect logic belongs in `helpers/`; use a `components/` `MonoBehaviour` when the logic needs to live on a GameObject (e.g. Unity lifecycle callbacks, per-instance state on a spawned prefab).

## Multiplayer

Valheim is client-server. Every redeem effect must work correctly for players who are **not** the one who triggered it, not just the local/owning client — always think through the non-owner case, even though it can only be verified manually in-game.

- **This mod never uses custom RPCs.** Networked config is replicated by writing it once into ZDO fields (hashed keys, e.g. `"WBTI_X_Y".GetStableHashCode()`) from an `Initialize()` method — called only on the client that spawned/triggered the object — and reading it back in `Awake()`, which every replicated instance (other clients, and after a scene reload) runs identically to configure itself. Follow this pattern for new networked `components/` classes instead of introducing RPCs. See `TwitchTimeStopZone`, `TwitchFreezeData`, `TwitchWindmillPersistentData`.
- **Guard ongoing ZDO writes with `m_netView.IsOwner()`.** One-time config in `Initialize()` is safe to write unconditionally (the initializing client is always the owner at that point). Anything written on an ongoing basis after that (timers, position, changing state) must check `IsOwner()` first — a non-owner's write doesn't propagate over the network and just wastes work. Cache `IsOwner()` early if a teardown path (e.g. `OnDestroy` after `WearNTear.Destroy()`) might run after the ZDO is already gone — see `TwitchPersistentDestruction.m_isOwner`.
- **A ZDO's position is set once at creation and never auto-updates.** Moving a networked object's Unity `transform` does not move its ZDO. Valheim uses the ZDO's registered position to decide which sector it belongs to, and therefore which peers even receive it at all — so anything that visually follows a moving target (e.g. `TwitchTimeStopZone` following a boat/tame) must have its **owner** call `zdo.SetPosition(transform.position)` on an ongoing basis, mirroring vanilla's `ZSyncTransform.OwnerSync()`. Skipping this makes the effect look perfect to the owner while silently never syncing to other clients.
- **Distinguish per-client-local effects from per-instance networked effects.** Some effects only ever affect the local viewer and need no ZDO sync at all (e.g. `ShieldDomeHelper`'s dome renders per-camera; `TimeStopHelper.FreezePlayer` only touches `Player.m_localPlayer`). Others are applied independently by every client's own copy of a synced object (e.g. `TwitchFreezeData`/`TwitchPhysicsFreezeData` disable local Unity components on each client's own instance of that networked GameObject, each driven by that client's own script reading the same replicated ZDO config — no RPC broadcast needed). Decide which category a new effect falls into before implementing it.
- **When Valheim's exact API/networking behavior is unclear, verify it in the real game code** rather than guessing — see `Environment.props` for the Valheim install path; `valheim_Data/Managed/assembly_valheim.dll` is the main game assembly and can be decompiled (e.g. with `ilspycmd`) to confirm method/field behavior before relying on it.

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
