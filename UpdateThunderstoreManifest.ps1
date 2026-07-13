# Updates the Thunderstore package manifest version_number.
# Called by MSBuild after the mod DLL is built and copied to the package directory.

param(
    [Parameter(Mandatory = $true)][string]$ManifestPath,
    [Parameter(Mandatory = $true)][string]$Version
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

if (-not (Test-Path -LiteralPath $ManifestPath -PathType Leaf)) {
    throw "Thunderstore manifest was not found: $ManifestPath"
}

$VersionParts = $Version.Split('.')
if ($VersionParts.Count -lt 3) {
    throw "Invalid assembly version: $Version"
}

$PackageVersion = "$($VersionParts[0]).$($VersionParts[1]).$($VersionParts[2])"
$Manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
$OldVersion = [string]$Manifest.version_number
$Manifest.version_number = $PackageVersion

$Json = $Manifest | ConvertTo-Json -Depth 100
$Utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($ManifestPath, $Json + [Environment]::NewLine, $Utf8WithoutBom)

if ($OldVersion -eq $PackageVersion) {
    Write-Host "Thunderstore manifest version is already $PackageVersion"
}
else {
    Write-Host "Thunderstore manifest version updated: $OldVersion -> $PackageVersion"
}
