param(
    [Parameter(Mandatory)]
    [ValidateSet('Debug','Release')]
    [System.String]$Target,
    
    [Parameter(Mandatory)]
    [System.String]$TargetPath,
    
    [Parameter(Mandatory)]
    [System.String]$TargetAssembly,

    [Parameter(Mandatory)]
    [System.String]$ValheimPath,

    [Parameter(Mandatory)]
    [System.String]$ProjectPath,
    
    [System.String]$DeployPath
)

# Make sure Get-Location is the script path
Push-Location -Path (Split-Path -Parent $MyInvocation.MyCommand.Path)

# Test some preliminaries
("$TargetPath",
 "$ValheimPath",
 "$(Get-Location)\libraries"
) | % {
    if (!(Test-Path "$_")) {Write-Error -ErrorAction Stop -Message "$_ folder is missing"}
}

# Plugin name without ".dll"
$name = "$TargetAssembly" -Replace('.dll')

# Create the mdb file
$pdb = "$TargetPath\$name.pdb"
if (Test-Path -Path "$pdb") {
    Write-Host "Create mdb file for plugin $name"
    Invoke-Expression "& `"$(Get-Location)\libraries\Debug\pdb2mdb.exe`" `"$TargetPath\$TargetAssembly`""
}

# Main Script
Write-Host "Publishing for $Target from $TargetPath"

if ($Target.Equals("Debug")) {
    if ($DeployPath.Equals("")){
      $DeployPath = "$ValheimPath\BepInEx\plugins"
    }
    
    $plug = New-Item -Type Directory -Path "$DeployPath\$name" -Force
    Write-Host "Copy $TargetAssembly to $plug"
    Copy-Item -Path "$TargetPath\$name.dll" -Destination "$plug" -Force
    Copy-Item -Path "$TargetPath\$name.pdb" -Destination "$plug" -Force
    Copy-Item -Path "$TargetPath\$name.dll.mdb" -Destination "$plug" -Force

    # The Twitch SDK's native library isn't part of the .csproj build output, so it never lands
    # in TargetPath on its own - copy it straight from the vendored TwitchSDK folder instead, or
    # the plugin throws DllNotFoundException: R66_core the first time it touches Twitch.API.
    Copy-Item -Path "$ProjectPath\TwitchSDK\x86_64\R66_core.dll" -Destination "$plug" -Force
}

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

    if (-not (Test-Path "$PackagePath\CHANGELOG.md")) {
        Write-Error -ErrorAction Stop -Message "CHANGELOG.md is missing at $PackagePath\CHANGELOG.md."
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

    # Package\README.md is the source of truth (maintained directly) — never copied over from
    # a root-level README.

    Remove-Item -Path "$PackagePath\plugins" -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -Type Directory -Path "$PackagePath\plugins" -Force | Out-Null
    Copy-Item -Path "$TargetPath\$TargetAssembly" -Destination "$PackagePath\plugins\$TargetAssembly" -Force

    # The Twitch SDK's native library isn't part of the .csproj build output (see the Debug
    # branch above) - the Release package needs it too, or the plugin throws
    # DllNotFoundException: R66_core the first time it touches Twitch.API.
    Copy-Item -Path "$ProjectPath\TwitchSDK\x86_64\R66_core.dll" -Destination "$PackagePath\plugins" -Force

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

# Pop Location
Pop-Location