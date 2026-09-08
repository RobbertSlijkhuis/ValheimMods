# Disabling the Ashlands weather heat-distortion VFX

Investigated 2026-09-08 from `WizshBoneTwitchIntegration`, but this is **not** Twitch-integration
scope — it's a general Valheim/Harmony technique, meant to be re-implemented in whichever mod
wants it (a small standalone patch mod, most likely). Tested once as a proof of concept there and
confirmed to work, then pulled back out.

## The question

Ashlands weather (AshRain and, per user observation, the other Ashlands weathers too) shows a
screen-space heat-haze/distortion effect "everywhere", independent of the player's position. Goal:
find what causes it and disable it.

## Two unrelated heat-distortion mechanisms exist in the game — don't confuse them

### 1. `Character.m_lavaHeatLevel` / `HeatDistortImageEffect` — NOT this one

`assembly_valheim.dll` has exactly one distortion-capable `MonoBehaviour`,
`HeatDistortImageEffect` (screen post-process, `OnRenderImage`, shader `Hidden/CameraHeatDistort`).
It's attached to the `GameCamera` GameObject. The only two places in the whole assembly that touch
it are:

- `GameCamera.Awake()` — just grabs the component reference.
- `Character.UpdateHeatEffects(dt)` — the only place that ever sets `.enabled` / `.m_intensity`,
  and only for `Player.m_localPlayer`.

It's driven by `Character.m_lavaHeatLevel` (0–1), built up from lava proximity **or** — the part
that looks Ashlands-related — this daytime condition in `Character`'s heat-update method:

```csharp
else if (biome == Heightmap.Biome.AshLands && m_dayHeatGainRunning != 0f && IsPlayer()
          && EnvMan.IsDay() && !IsUnderRoof()
          && GetEquipmentHeatResistanceModifier() < m_dayHeatEquipmentStop)
```

This is a **biome + day/night + roof + gear** survival mechanic, not weather-driven —
`EnvMan.IsDay()` is just the clock, and `EnvMan.cs` never references `HeatDistortImageEffect` or
sets any global heat/distortion shader property. Damage (`Character.UpdateHeatDamage`) reads the
same `m_lavaHeatLevel` but is fully independent of the visual — disabling the visual doesn't touch
the actual heat-damage gameplay.

**This mechanism is not what the user was pointing at.** It's mentioned here only so a future
session doesn't waste time rediscovering it and conflating the two.

### 2. `vfx_Ashlands_HeatDistortion.prefab` — this is the actual one

This is a dedicated weather VFX asset, found via the game's **asset bundle manifest**, not the C#
assembly — `EnvMan.cs` contains no code specific to it at all (see below).

Confirmed via `<Valheim install>/valheim_Data/StreamingAssets/SoftRef/manifest_extended`
(a UTF-8 text manifest mapping addressable asset paths → bundle IDs — grep it directly, no
extraction tool needed for this part):

| Asset | Path | Bundle |
|---|---|---|
| Shader | `Assets/Shaders/Distortion.shader` | `c4210710` |
| Material | `Assets/Effects/materials/heathaze_distortion.mat` | `c4210710` |
| **VFX prefab** | `Assets/Effects/weather/ashlands/vfx_Ashlands_HeatDistortion.prefab` | `17245031` |

Bundle `17245031` contains **only** Ashlands weather prefabs — nothing else in the game's ~600
bundles:
```
Ashlands_storm.prefab
Ashlands_FaderFX.prefab
Ashlands_CinderRain.prefab
fx_ember_rain.prefab
Ashlands_MeteorShower.prefab
Ashlands_Misty.prefab
vfx_Ashlands_HeatDistortion.prefab   <-- the target
Ashlands_AshRain.prefab
```
That's why it shows up for every Ashlands weather, not just AshRain — it's packaged and (almost
certainly) referenced from each of these weather entries' `EnvSetup`.

There's also a separate **UI-space** variant, in a different bundle, not investigated further:
`Assets/Shaders/UIHeatDistortion.shader` + `Assets/UI/materials/ashlandsui_heatdistortion.mat`
(bundle `b8689a71`).

## How EnvMan actually activates it (confirmed from decompiled `EnvMan.SetEnv`)

`EnvMan.SetEnv(EnvSetup env, ...)` runs every `FixedUpdate` and is generic across every biome —
there is no Ashlands-specific branch. Two relevant paths, both driven by data (`EnvSetup` fields
set in the Unity Inspector, not code):

```csharp
// env.m_envObject: a single persistent GameObject reference
if (env.m_envObject != m_currentEnvObject)
{
    if ((bool)m_currentEnvObject) { m_currentEnvObject.SetActive(false); m_currentEnvObject = null; }
    if ((bool)env.m_envObject) { m_currentEnvObject = env.m_envObject; m_currentEnvObject.SetActive(true); }
}

// env.m_psystems: an array of persistent GameObject references
if (env.m_psystems != m_currentPSystems)
{
    if (m_currentPSystems != null) { SetParticleArrayEnabled(m_currentPSystems, enabled: false); m_currentPSystems = null; }
    if (env.m_psystems != null && (...)) { SetParticleArrayEnabled(env.m_psystems, enabled: true); m_currentPSystems = env.m_psystems; }
}
```

`SetParticleArrayEnabled` (also in `EnvMan.cs`) does **not** call `GameObject.SetActive` — it
toggles components on the object's children instead:

```csharp
private void SetParticleArrayEnabled(GameObject[] psystems, bool enabled)
{
    foreach (GameObject gameObject in psystems)
    {
        foreach (var ps in gameObject.GetComponentsInChildren<ParticleSystem>())
        {
            var emission = ps.emission;
            emission.enabled = enabled;
        }
        var mist = gameObject.GetComponentInChildren<MistEmitter>();
        if ((bool)mist) mist.enabled = enabled;
    }
}
```

**Important:** neither path is `Instantiate`/`Destroy` — it's the same persistent scene object
every time, just toggled. It is not "spawned" per weather change.

## Which field is it actually in? Not confirmed

I did not open the prefab itself (that needs real Unity asset extraction — AssetStudio / UnityPy /
AssetsTools.NET; the bundle's object data is LZ4-compressed, `manifest_extended` only gives paths
and bundle IDs, not the scene hierarchy or which `EnvSetup.m_envObject`/`m_psystems` entry
references it). No such tool was installed/used in this investigation — if you want to nail this
down precisely rather than name-sniffing at runtime, that's the next step.

Given the "vfx_" naming convention, `m_psystems` is the more likely home, but the runtime
name-search approach below works regardless of which one it is.

## Why NOT to `Destroy()` it

Worth stating explicitly since it's tempting once you have a reference to it:

- If it's an `env.m_envObject`: safe to destroy. Every use is guarded with
  `if ((bool)env.m_envObject)`, and Unity's overloaded null-check treats a destroyed object as
  `false` — a destroyed reference is silently skipped forever after.
- If it's an `env.m_psystems` entry: **not safe**. `SetParticleArrayEnabled` has no null/destroyed
  guard before `gameObject.GetComponentsInChildren<ParticleSystem>()` — a destroyed entry throws
  `MissingReferenceException` on every future weather transition that touches that array, which
  aborts the rest of that `SetEnv()` call for the tick (skipping the shader/fog/lighting updates
  that come after it in the method).

Since which field it lives in wasn't confirmed, `Destroy()` is a coin flip between "fine forever"
and "breaks weather updates every transition". **Don't destroy it — disable it instead** (see
below). Disabling only requires the same care in the `m_envObject` case (vanilla will keep trying
to re-`SetActive(true)` it, so suppression needs to keep re-firing) — which the implementation
below already handles generically for both cases.

## Working implementation (tested once, worked)

Two files, following this mod's harmony/helpers split (adapt namespaces to whichever mod this
lands in).

**`helpers/AshlandsHeatHazeHelper.cs`** — finds the live scene instance by name (not by exact
path, since the exact hierarchy wasn't confirmed) and force-disables it whenever active:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class AshlandsHeatHazeHelper
{
    private const string NameFilter = "HeatDistortion";

    private static readonly List<GameObject> _targets = new List<GameObject>();
    private static bool _found;
    private static float _nextSearchTime;
    private static int _searchAttempts;
    private static string _lastLoggedEnv;

    public static void SuppressIfActive(string envName)
    {
        if (envName != _lastLoggedEnv)
        {
            Jotunn.Logger.LogWarning($"[Mod] AshlandsHeatHazeHelper: EnvMan weather changed to '{envName}'.");
            _lastLoggedEnv = envName;
        }

        if (!_found)
        {
            bool inAshlands = Player.m_localPlayer != null
                && Player.m_localPlayer.GetCurrentBiome() == Heightmap.Biome.AshLands;

            if (inAshlands && Time.unscaledTime >= _nextSearchTime)
            {
                FindTargets();
                _nextSearchTime = Time.unscaledTime + 3f;
            }

            if (!_found)
                return;
        }

        foreach (GameObject target in _targets)
        {
            if (target != null && target.activeSelf)
            {
                Jotunn.Logger.LogWarning($"[Mod] AshlandsHeatHazeHelper: '{target.name}' was activated, disabling it.");
                target.SetActive(false);
            }
        }
    }

    private static void FindTargets()
    {
        _searchAttempts++;
        Jotunn.Logger.LogWarning($"[Mod] AshlandsHeatHazeHelper: in AshLands biome, searching scene for objects matching \"{NameFilter}\" (attempt {_searchAttempts})...");

        Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform t in all)
        {
            // Resources.FindObjectsOfTypeAll also returns loaded prefab/asset objects that
            // were never placed in a scene - skip those, we only want the live instance.
            if (!t.gameObject.scene.IsValid())
                continue;

            if (t.name.IndexOf(NameFilter, StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            if (!_targets.Contains(t.gameObject))
                _targets.Add(t.gameObject);
        }

        if (_targets.Count > 0)
        {
            _found = true;
            Jotunn.Logger.LogWarning($"[Mod] AshlandsHeatHazeHelper: found {_targets.Count} object(s) to suppress: " +
                string.Join(", ", _targets.Select(g => g.name)));
        }
        else
        {
            Jotunn.Logger.LogWarning("[Mod] AshlandsHeatHazeHelper: no matching objects found yet, will retry in 3s.");
        }
    }
}
```

**`harmony/EnvManPatches.cs`** — postfix on the private `EnvMan.SetEnv`, called every
`FixedUpdate`:

```csharp
using HarmonyLib;
using System;

[HarmonyPatch]
public class EnvManPatches
{
    // "env" matches EnvMan.SetEnv's private parameter name so Harmony passes it through.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EnvMan), "SetEnv")]
    public static void SetEnv_Postfix(EnvSetup env)
    {
        try
        {
            AshlandsHeatHazeHelper.SuppressIfActive(env?.m_name);
        }
        catch (Exception e)
        {
            Jotunn.Logger.LogError("Something went wrong in EnvMan.SetEnv_Postfix: " + e);
        }
    }
}
```

### Design notes on the implementation

- Purely client-local visual suppression — no ZDO/network writes, no multiplayer concerns.
- Search is gated on `Player.GetCurrentBiome() == Heightmap.Biome.AshLands` so it doesn't run (or
  log) for the entire rest of a playthrough spent outside Ashlands.
- Search retries every 3s until found, then stops permanently (`_found` latches true).
- Logs weather-name transitions every time `EnvMan` changes environment (`env.m_name`), search
  attempts, what was found, and every time something is actually disabled — this was added on
  request for visibility while testing; trim it down once confirmed working if it's noisy.
- In the (more likely) `m_psystems` case, vanilla never calls `SetActive` on this object at all —
  so the single disable should latch permanently on its own, without needing the postfix to keep
  re-firing. The repeated-postfix-catches-it-again behavior only matters for the `m_envObject`
  case, where vanilla actively re-`SetActive(true)`s it on every transition back into that weather.

## If picking this up again

1. Re-implement the two files above in the target mod, adjusting `Jotunn.Logger` / namespace to
   match that mod's conventions.
2. Build, launch, travel to Ashlands, wait for a matching log line.
3. Confirm in the log which weather names trigger it and what object name(s) got found — that
   tells you definitively whether it's `m_envObject` or `m_psystems`, and gives you the exact
   object name if you want to hardcode it later instead of the substring search.
4. Once confirmed, the substring/biome-gated search could be tightened to the exact known name, or
   left as-is since it already latches after the first successful find and costs nothing further.
