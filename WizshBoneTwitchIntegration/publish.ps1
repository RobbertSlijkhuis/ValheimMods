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

    # SAPHONETTE-CLEANUP: temporary death sound for Saphonette's stream, not a permanent feature -
    # loaded from disk at runtime (see helpers/DeathSoundHelper.cs). Remove this block (and the
    # matching one in the Release section below) once the bit is over.
    if (Test-Path "$TargetPath\UUH.wav") {
        Copy-Item -Path "$TargetPath\UUH.wav" -Destination "$plug" -Force
    }

    # SAPHONETTE-CLEANUP: temporary shield sound for Saphonette's stream, not a permanent feature -
    # loaded from disk at runtime (see helpers/ShieldSoundHelper.cs). Remove this block (and the
    # matching one in the Release section below) once the bit is over.
    if (Test-Path "$TargetPath\Where_is_my_bubble.wav") {
        Copy-Item -Path "$TargetPath\Where_is_my_bubble.wav" -Destination "$plug" -Force
    }
}

if($Target.Equals("Release")) {
    Write-Host "Packaging for ThunderStore..."
    $Package="Package"
    $PackagePath="$ProjectPath\$Package"

    Write-Host "$PackagePath\$TargetAssembly"
    New-Item -Type Directory -Path "$PackagePath\plugins" -Force
    Copy-Item -Path "$TargetPath\$TargetAssembly" -Destination "$PackagePath\plugins\$TargetAssembly" -Force
    Copy-Item -Path "$ProjectPath\README.md" -Destination "$PackagePath\README.md" -Force

    # SAPHONETTE-CLEANUP: see the matching comment in the Debug block above.
    if (Test-Path "$TargetPath\UUH.wav") {
        Copy-Item -Path "$TargetPath\UUH.wav" -Destination "$PackagePath\plugins\UUH.wav" -Force
    }

    # SAPHONETTE-CLEANUP: see the matching comment in the Debug block above.
    if (Test-Path "$TargetPath\Where_is_my_bubble.wav") {
        Copy-Item -Path "$TargetPath\Where_is_my_bubble.wav" -Destination "$PackagePath\plugins\Where_is_my_bubble.wav" -Force
    }
    Compress-Archive -Path "$PackagePath\*" -DestinationPath "$TargetPath\$TargetAssembly.zip" -Force
}

# Pop Location
Pop-Location