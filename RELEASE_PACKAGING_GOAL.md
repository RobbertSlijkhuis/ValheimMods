# Goal: standardize Release packaging across mods

## What
Every mod's **Release** build should produce a Thunderstore-ready zip inside its own
`Package\` folder (not `bin\Release`), built from an **explicit allowlist** of the files the
upload actually needs — not by zipping the whole `Package\` folder and hoping dev-only files
(GIMP `.xcf` sources, WIP screenshots, a stale zip from a previous build, etc.) don't sneak in.
The zip is named `<ModName>_<version>.zip` (e.g. `UpgradeAntlerPickaxe_1.2.0.zip`), and any zip
from a prior build/version is deleted first so `Package\` only ever holds the current one.

The Release build should also **fail fast on version mismatches**: the `PluginVersion` const in
the main source file is the source of truth, and the build should abort (before packaging) if
`manifest.json`'s `version_number` doesn't match it, or `CHANGELOG.md` has no described entry
for that version — instead of silently shipping a mismatched/undocumented release. It should
also abort if `manifest.json`/`README.md`/`CHANGELOG.md` were saved with a **UTF-8 BOM**, which
Thunderstore rejects.

## Reference implementation
`UpgradeAntlerPickaxe\publish.ps1` — the `Release` branch of the `if($Target.Equals(...))` block.
Use its shape as the pattern to *adapt*, not copy verbatim (see checklist below):

```powershell
if($Target.Equals("Release")) {
    Write-Host "Packaging for ThunderStore..."
    $Package="Package"
    $PackagePath="$ProjectPath\$Package"

    # Version check: PluginVersion in the main source file is the source of truth. Fails the
    # build (Write-Error -ErrorAction Stop) if manifest.json doesn't match, or CHANGELOG.md
    # has no described entry for it.
    $mainFile = "$ProjectPath\$name.cs"
    $versionMatch = Select-String -Path "$mainFile" -Pattern 'PluginVersion\s*=\s*"([^"]+)"' | Select-Object -First 1
    if (-not $versionMatch) { Write-Error -ErrorAction Stop -Message "Could not find PluginVersion in $mainFile" }
    $pluginVersion = $versionMatch.Matches[0].Groups[1].Value

    $manifest = Get-Content "$PackagePath\manifest.json" -Raw | ConvertFrom-Json
    if ($manifest.version_number -ne $pluginVersion) {
        Write-Error -ErrorAction Stop -Message "Version mismatch: PluginVersion is $pluginVersion but manifest.json version_number is $($manifest.version_number)."
    }

    $changelogLines = Get-Content "$PackagePath\CHANGELOG.md"
    $headerIndex = -1
    for ($i = 0; $i -lt $changelogLines.Count; $i++) {
        if ($changelogLines[$i] -match "^#+\s*$([Regex]::Escape($pluginVersion))\b") { $headerIndex = $i; break }
    }
    if ($headerIndex -eq -1) { Write-Error -ErrorAction Stop -Message "CHANGELOG.md has no entry for version $pluginVersion." }
    $hasContent = $false
    for ($i = $headerIndex + 1; $i -lt $changelogLines.Count; $i++) {
        if ($changelogLines[$i] -match "^#+\s") { break }
        if ($changelogLines[$i].Trim().Length -gt 0) { $hasContent = $true; break }
    }
    if (-not $hasContent) { Write-Error -ErrorAction Stop -Message "CHANGELOG.md entry for $pluginVersion has no described changes." }

    # Thunderstore rejects files saved with a UTF-8 BOM
    foreach ($textFile in @("manifest.json","README.md","CHANGELOG.md")) {
        $bytes = [System.IO.File]::ReadAllBytes("$PackagePath\$textFile")
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            Write-Error -ErrorAction Stop -Message "$textFile is saved with a UTF-8 BOM, which Thunderstore rejects."
        }
    }

    New-Item -Type Directory -Path "$PackagePath\plugins" -Force
    Copy-Item -Path "$TargetPath\$TargetAssembly" -Destination "$PackagePath\plugins\$TargetAssembly" -Force
    # Package\README.md is the source of truth (maintained directly) — never copied over from
    # a root-level README (see checklist item 2).

    $itemsToPackage = @(
        "$PackagePath\manifest.json",
        "$PackagePath\README.md",
        "$PackagePath\CHANGELOG.md",
        "$PackagePath\icon.png",
        "$PackagePath\plugins"
    )

    # Remove any zip from a previous build/version so Package only ever holds the current one
    Get-ChildItem -Path "$PackagePath" -Filter "*.zip" | Remove-Item -Force

    $zipName = "${name}_$pluginVersion.zip"
    Compress-Archive -Path $itemsToPackage -DestinationPath "$PackagePath\$zipName" -CompressionLevel Optimal -Force
}
```

## Per-mod checklist
Each mod in this monorepo has a slightly different setup, so apply this individually rather than
copy-pasting the script wholesale:

1. **Locate the actual publish/package script.** Path varies — check the mod's `.csproj`
   post-build `<Exec>` command for the real script path (e.g. `CraftingStationTweakz` calls
   `scripts/publish.ps1`, not a root-level `publish.ps1` like `UpgradeAntlerPickaxe`).
2. **`Package\README.md` is the source of truth for every mod — it must never be overwritten
   from a root-level README by the Release build.** Don't just check that the copy's source
   *path* resolves; check whether the script copies over `Package\README.md` at all. In
   `CraftingStationTweakz` the root-level `README.md` was the *unfilled JotunnModStub template*
   (empty `## Features`/`## Changelog` headers, no real content), silently clobbering the real,
   maintained `Package\README.md` on every Release build. In `UpgradeAntlerPickaxe` the root
   `README.MD` was actually a fuller, actively-maintained doc (it even had extra content —
   an inline changelog — `Package\README.md` lacked) and still overwrote `Package\README.md`
   each build; fixed the same way even though the root content there was good, since the
   direction of the copy was still wrong per this rule. In both mods the fix was the same:
   remove the `Copy-Item ... -Destination "$PackagePath\README.md"` line entirely and leave
   `Package\README.md`'s existing content untouched — don't backfill content from the root file
   into it as part of this fix (a separate, deliberate content decision if wanted later). The
   root-level README file itself is left alone in both cases (not deleted, not filled in) as a
   separate follow-up.
3. **Point the zip's `-DestinationPath` at that mod's own `Package\` folder**, not `bin\Release`.
4. **Build the zip from an explicit allowlist** of that mod's actual `Package\` files — inspect
   what's really in `Package\` for that mod first; don't assume the same file set as
   `UpgradeAntlerPickaxe` (manifest/README/CHANGELOG/icon/plugins).
5. **Add a `*/Package/*.zip`-equivalent rule** to that mod's own `.gitignore` if it doesn't already
   have one, so the generated zip doesn't become a stray untracked/committed file.
6. **Verify asset folders (e.g. `img\`) aren't accidentally gitignored** — check with
   `git status --porcelain --ignored`, not `git check-ignore` alone (the latter can report a
   false-positive match on an empty/non-existent directory passed with a trailing slash).
7. **Decide per-mod whether Release should bundle `.pdb`/`.dll.mdb` debug symbols.**
   `UpgradeAntlerPickaxe` chose not to (a plain Release stack trace still shows the throwing
   method/class name, just not the exact line) — another mod's owner may choose differently.
8. **Adapt the version-consistency check** to that mod's own main plugin file name/path (the
   check assumes `$ProjectPath\$name.cs` defines `PluginVersion = "..."` — confirm that mod's
   main file actually matches its assembly name before reusing the check as-is) and to its
   `CHANGELOG.md` heading format if different from `### X.Y.Z`.
9. **Recreate `plugins\` from scratch before copying the DLL into it**, don't just
   `New-Item -Force` an existing folder. It's gitignored (per item 5's pattern), so nothing else
   ever cleans it — `CraftingStationTweakz\Package\plugins\` had accumulated stale
   `.pdb`/`.dll.mdb` files from old builds, which the allowlist's `plugins` entry would have
   silently zipped in alongside the current `.dll`.

## Status
- ✅ `UpgradeAntlerPickaxe` — done. Also removed its root `README.MD`→`Package\README.md` copy
  per item 2, so `Package\README.md` is the source of truth there too; root README content
  left as-is (its extra inline changelog section was not backfilled into `Package\README.md`).
  Its `plugins\` recreation was later backported to item 9 (see `PavedRoadNoLevel` entry below) —
  it originally still did a plain `New-Item -Force` without clearing the folder first.
- ✅ `CraftingStationTweakz` — done. Hit and fixed the item 2 README-clobbering bug (see above)
  and the item 9 stale-`plugins\`-contents bug; both callouts added to the checklist from this.
- ✅ `PavedRoadNoLevel` — done. Hit the item 2 README-clobbering bug (same fix) and the item 9
  stale-`plugins\`-contents bug (`Package\plugins\` had leftover `.dll.mdb`/`.pdb` from old
  builds), fixed the same way as `CraftingStationTweakz`. Also backported the item 9 fix to
  `UpgradeAntlerPickaxe\publish.ps1` itself, since inspecting it for this rollout showed it never
  got that fix even though it's the reference implementation.
- ✅ `RestingRockFace` — done. Script lives at `scripts\publish.ps1` (like
  `CraftingStationTweakz`, not a root-level `publish.ps1`). Hit and fixed the item 2
  README-clobbering bug (`Package\README.md` was being overwritten from the project-root
  `README.md`, which is the unfilled JotunnModStub template) and the item 9
  stale-`plugins\`-contents bug (`Package\plugins\` had leftover `.dll.mdb`/`.pdb` from an old
  build). **Known pre-existing gap left unfixed, by explicit user choice**: `manifest.json`
  `version_number` (`0.0.1`) doesn't match the code's `PluginVersion` (`1.0.0`), and
  `CHANGELOG.md` has no `1.0.0` entry — so the new version check will make the *next* Release
  build fail until the mod owner updates those files themselves.
- ✅ `PlantCart` — done. Script lives at `scripts\publish.ps1` (like `CraftingStationTweakz`,
  not a root-level `publish.ps1`). Hit and fixed the item 2 README-clobbering bug (`Package\README.md`
  was being overwritten from the project-root `README.md`, which is the unfilled JotunnModStub
  template) and the item 9 stale-`plugins\`-contents risk (previously a plain `New-Item -Force`
  without clearing the folder first). Also normalized a pre-existing git-tracking quirk not
  covered by the standard checklist: `Package\CHANGELOG.md` and `Package\icon.xcf` were tracked
  under a lowercase `package/` path while the rest of the folder (`README.md`, `icon.png`,
  `manifest.json`) was tracked under capital `Package/` — harmless on case-insensitive Windows
  but a correctness risk on a case-sensitive checkout, so both files were `git mv`'d onto the
  capital-`Package` path.
- ✅ `WizshBoneTwitchIntegration` — done. Script is a root-level `publish.ps1` (like
  `UpgradeAntlerPickaxe`, not the `scripts\publish.ps1` pattern). Unlike every mod fixed so far,
  its Release branch had none of the guard logic at all *and* was zipping to the wrong place —
  `Compress-Archive -Path "$PackagePath\*" -DestinationPath "$TargetPath\$TargetAssembly.zip"`
  zipped everything unfiltered from `Package\` (no allowlist) into `bin\Release\net48\...dll.zip`,
  not `Package\` itself, with no version in the name; fixed to the standard allowlist/destination/
  naming per items 3-4. Hit and fixed the item 2 README-clobbering bug (`Package\README.md` was
  being overwritten from the project-root `README.md`, the unfilled JotunnModStub template — note
  both files were already byte-identical, so the fix freezes the placeholder text rather than
  restoring real content) and applied the item 9 preventive `plugins\` recreate-from-scratch fix
  (no stale contents existed yet, but applied per the established pattern). Also added copying the
  vendored native `TwitchSDK\x86_64\R66_core.dll` into `Package\plugins\` — this mod's Release
  package previously shipped without it even though the Debug deploy path already copied it; the
  plugin throws `DllNotFoundException: R66_core` at runtime without it. Stripped a pre-existing
  UTF-8 BOM from both `Package\manifest.json` and `Package\README.md` so the new BOM check (and
  Thunderstore) accept them. Added the missing `*/Package/*.zip` gitignore rule to the **shared
  root `.gitignore`** rather than a new per-mod file, since this mod has no `.gitignore` of its
  own and already inherited the existing `*/Package/plugins/*` rule from there.
  **Known pre-existing gaps left unfixed, by explicit user choice**: `Package\CHANGELOG.md`
  doesn't exist at all (not even a stub) — the new version check will make the *next* Release
  build fail until the mod owner creates it with a `0.0.1`-or-current entry, same as the
  `RestingRockFace` precedent. `Package\manifest.json` also still has placeholder content
  (`name: "JotunnModStub"`, empty `description`/`website_url`) — left as a separate follow-up.
- ✅ `GrapplingHarpoonHook` — done. Script lives at `scripts\publish.ps1` (like
  `CraftingStationTweakz`). Its Release branch was the stock JotunnModStub one (no guards, zipped
  `Package\*` unfiltered into `bin\Release`, and overwrote `Package\README.md` from the project-root
  `README.md`) — fixed per items 2-4 and 9. The asset bundle is an `EmbeddedResource`, so
  `plugins\` only needs the DLL. Stripped a pre-existing UTF-8 BOM from `Package\manifest.json` and
  `Package\README.md`, created `Package\CHANGELOG.md` with a stub `### 0.0.1` / "Initial release"
  entry (user's choice) so the first Release build passes the version check, and added the missing
  `*/Package/*.zip` rule to the mod's own `.gitignore`. **Left as follow-ups**: `manifest.json` still
  has placeholder content (`name: "JotunnModStub"`, empty `description`/`website_url`), the
  `Package\README.md` body is still the template, and the Unix `scripts\publish.sh` wasn't touched.
- ✅ `ModularMagic_Core` and `ModularMagic_EarthStaffs` — done. Both use a root-level
  `publish.ps1` (like `UpgradeAntlerPickaxe`; the two scripts were byte-identical stock stubs and
  are identical again), so the reference Release branch was spliced in as-is (`$name.cs` matches
  each mod's main file). Fixed the item 2 README-clobbering bug (`Package\README.md` was being
  overwritten from the root `README.MD`, which is a different, longer doc — both left untouched),
  the item 3/4 wrong-destination/unfiltered zip, and applied the item 9 `plugins\` recreate.
  Stripped a pre-existing UTF-8 BOM from both mods' `Package\manifest.json` and `Package\README.md`,
  and created `Package\CHANGELOG.md` with a stub `### 0.0.1` / "Initial release" entry so the first
  Release build passes the version check. (The root `.gitignore` rules `*/Package/...` turned out
  *not* to cover these mods — see the `**/Package/...` rules added in the next entry.)
  EarthStaffs' `manifest.json` originally didn't list ModularMagic_Core as a dependency although
  the plugin has a `[BepInDependency]` on it; fixed in the next entry. **Left as follow-up**: the
  Unix `publish_release.sh` wasn't touched.
- ✅ `ModularMagic_Armors`, `_FireStaffs`, `_Food`, `_IceStaffs`, `_LightningStaffs`, `_Utilities`
  and `_BloodMagic` — done, same treatment as Core/EarthStaffs. The six root-level `publish.ps1`
  files are byte-identical to Core's; BloodMagic's lives at `scripts\publish.ps1` (its Debug
  branch/preamble differ, so only the Release branch was spliced in, LF endings kept). Each got
  a BOM-stripped `Package\manifest.json`/`README.md` and a stub `### 0.0.1` `Package\CHANGELOG.md`.
  **Gitignore correction**: the shared root `.gitignore` rules `*/Package/plugins/*` and
  `*/Package/*.zip` are anchored one folder deep, so they never matched
  `ModularMagic_X\ModularMagic_X\Package\...`; added `**/Package/plugins/` and `**/Package/*.zip`
  (verified with `git check-ignore`). Ignore rules don't untrack files already committed —
  Core's/EarthStaffs' zip + `plugins\` DLLs and Armors' `plugins\*.dll`/`.dll.mdb` are tracked and
  would need a `git rm --cached` to drop. **Left as follow-ups, by explicit user choice**: all
  seven `manifest.json` files are still the JotunnModStub placeholder (`name: "JotunnModStub"`,
  empty `description`, old Jotunn 2.24.3/2.26.1 dependency) and the `Package\README.md` bodies
  are still the template; the Unix `publish_release.sh` / `scripts\publish.sh` weren't touched.
  **Core dependency**: every Modular Magic addon now depends on `ModularMagic_Core` (they use its
  materials and runes). Each of the seven got `[BepInDependency("DeathWizsh.ModularMagic_Core")]`
  in its main plugin file (EarthStaffs already had it), and all eight addon `manifest.json` files
  (these seven + EarthStaffs) list `"DeathWizsh-ModularMagicCore-0.0.1"` in `dependencies`. The
  `DeathWizsh` Thunderstore author segment is **assumed** from the plugin GUID prefix — confirm it
  against the real Thunderstore team name, and keep the version in step with the published Core
  release. No csproj references to Core's DLL were added.
- ⬜ All other mods in this monorepo — not started.
