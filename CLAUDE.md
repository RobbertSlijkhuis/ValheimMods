# Valheim modding

Jötunn/BepInEx mods for Valheim (.NET 4.8). Each mod lives in its own subfolder.

## Build and deploy

- Do not build or run projects, and do not copy or deploy DLLs. No build step is needed from Claude — the user builds and tests in-game.
- The Jötunn stub's post-build steps copy the DLL into the Valheim `BepInEx\plugins` folder and into `<Mod>Unity/Assets/Assemblies`, which is another reason not to build.

## Where logic belongs

- `harmony/` classes should contain **only** Harmony patch methods (the `[HarmonyPrefix]`/`[HarmonyPostfix]`/`[HarmonyPatch]` methods themselves). No state, no standalone effect logic.
- State and logic that a patch depends on (e.g. "is the player currently frozen") belongs in the corresponding `helpers/` class, exposed via a small public property/method the patch reads or calls.
- Most game-effect logic belongs in `helpers/`; use a `components/` `MonoBehaviour` when the logic needs to live on a GameObject (e.g. Unity lifecycle callbacks, per-instance state on a spawned prefab).

## Debugging

Use `Jotunn.Logger` (or BepInEx `Logger`) for debug output — prefer `LogWarning` so messages stand out in the BepInEx console/log without being noise-level info. Do not use `Debug.Log` directly.

```csharp
Jotunn.Logger.LogWarning($"[Tag] SomeThing: {value}");
```

## Verifying Valheim behavior

When Valheim's exact API/networking behavior is unclear, verify it in the real game code rather than guessing. See `Environment.props` in the mod's folder for the Valheim install path. `valheim_Data/Managed/assembly_valheim.dll` is the main game assembly and can be decompiled (e.g. with `ilspycmd`) to confirm method/field behavior before relying on it.
