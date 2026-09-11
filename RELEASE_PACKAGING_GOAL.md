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
    Copy-Item -Path ".\README.md" -Destination "$PackagePath\README.md" -Force

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
2. **Check for the same "README copied from the wrong folder" bug** before assuming it exists —
   don't blind-fix something that isn't broken there.
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

## Status
- ✅ `UpgradeAntlerPickaxe` — done.
- ⬜ All other mods in this monorepo — not started.
