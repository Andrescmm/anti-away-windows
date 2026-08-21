[CmdletBinding()]
param(
    [string]$Version = "0.1.0",

    [ValidateSet("win-x64", "win-arm64")]
    [string]$RuntimeIdentifier = "win-x64"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$arch = if ($RuntimeIdentifier -eq "win-arm64") { "arm64" } else { "x64" }

& (Join-Path $PSScriptRoot "Publish.ps1") -RuntimeIdentifier $RuntimeIdentifier -Configuration Release

$isccCandidates = @(
    @(
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
        (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe"),
        (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe")
    ) | Where-Object { $_ -and (Test-Path $_) }
)

if ($isccCandidates.Count -eq 0) {
    throw "Inno Setup 6 was not found. Install it with: winget install JRSoftware.InnoSetup"
}

$installerScript = Join-Path $repoRoot "installer\AntiAway.iss"
& $isccCandidates[0] "/DMyAppVersion=$Version" "/DMyAppArch=$arch" $installerScript

if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup failed with exit code $LASTEXITCODE."
}

Write-Host "Installer created under artifacts\installer."

