# Sweeps every mod folder in this monorepo for a current Thunderstore-ready zip
# (produced by the standardized publish.ps1 pattern described in RELEASE_PACKAGING_GOAL.md)
# and collects them all into one folder on the Desktop. Mods that haven't been migrated to
# that pattern yet are simply skipped - no mod list to maintain here.

# Make sure Get-Location is the script path; this folder IS the monorepo root.
Push-Location -Path (Split-Path -Parent $MyInvocation.MyCommand.Path)

# GetFolderPath resolves the real Desktop location even when it's OneDrive-redirected,
# which a hardcoded "$env:USERPROFILE\Desktop" would miss.
$desktop = [Environment]::GetFolderPath('Desktop')
$destination = Join-Path $desktop "MyLatestMods"

# Wipe and recreate on every run so the folder always reflects exactly the current set of
# release zips - otherwise old versions would pile up since filenames include the version.
if (Test-Path -Path $destination) {
    Remove-Item -Path $destination -Recurse -Force
}
New-Item -Type Directory -Path $destination -Force | Out-Null

$copied = @()
$skipped = @()
$failed = @()

Get-ChildItem -Path . -Directory | ForEach-Object {
    $modRoot = $_
    $modName = $modRoot.Name
    $packagePath = Join-Path $modRoot.FullName "$modName\Package"

    if (-not (Test-Path -Path $packagePath)) {
        $skipped += $modName
        return
    }

    # Only the exact filename publish.ps1 produces counts - this naturally excludes stray
    # zips left in Package\ by mods still on the legacy publish script (wrong name/no version).
    $pattern = "^$([Regex]::Escape($modName))_\d+(\.\d+)+\.zip$"
    $releaseZips = Get-ChildItem -Path $packagePath -Filter "*.zip" -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match $pattern }

    if ($releaseZips.Count -eq 0) {
        $skipped += $modName
    } elseif ($releaseZips.Count -gt 1) {
        Write-Host "FAILED  $modName - multiple candidate zips found: $($releaseZips.Name -join ', ')"
        $failed += $modName
    } else {
        $zip = $releaseZips[0]
        try {
            Copy-Item -Path $zip.FullName -Destination $destination -Force -ErrorAction Stop
            Write-Host "Copied  $($zip.Name)"
            $copied += $modName
        } catch {
            Write-Host "FAILED  $modName - $($_.Exception.Message)"
            $failed += $modName
        }
    }
}

Write-Host ""
Write-Host "Summary: $($copied.Count) copied, $($skipped.Count) skipped, $($failed.Count) failed"
if ($skipped.Count -gt 0) {
    Write-Host "Skipped (no current release setup/zip): $($skipped -join ', ')"
}
if ($failed.Count -gt 0) {
    Write-Host "Failed: $($failed -join ', ')"
}
Write-Host "Zips collected in $destination"

Pop-Location

Read-Host "Press Enter to close"
