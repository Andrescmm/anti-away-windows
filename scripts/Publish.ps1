[CmdletBinding()]
param(
    [ValidateSet("win-x64", "win-arm64")]
    [string]$RuntimeIdentifier = "win-x64",

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "src\AntiAway\AntiAway.csproj"
$outputPath = Join-Path $repoRoot "artifacts\publish\$RuntimeIdentifier"
$platform = if ($RuntimeIdentifier -eq "win-arm64") { "ARM64" } else { "x64" }

if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force
}

dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime $RuntimeIdentifier `
    --self-contained true `
    --output $outputPath `
    -p:Platform=$platform `
    -p:WindowsAppSDKSelfContained=true `
    -p:PublishSingleFile=false

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$executable = Join-Path $outputPath "AntiAway.exe"
if (-not (Test-Path $executable)) {
    throw "Publish completed without producing AntiAway.exe."
}

Write-Host "AntiAway publish output: $outputPath"

