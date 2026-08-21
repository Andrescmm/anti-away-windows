<#
.SYNOPSIS
    Signs files with Azure Trusted Signing.

.DESCRIPTION
    Resolves signtool.exe from the restored Microsoft.Windows.SDK.BuildTools
    package, downloads the Trusted Signing dlib on first use, and signs the
    given files.

    Authentication uses DefaultAzureCredential. Sign in with "az login" for
    local builds, or set AZURE_TENANT_ID, AZURE_CLIENT_ID and
    AZURE_CLIENT_SECRET (or use workload identity federation) in CI.

.EXAMPLE
    .\scripts\Sign-Files.ps1 -Path artifacts\publish\win-x64\AntiAway.exe
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string[]]$Path,

    [string]$Endpoint = $env:TRUSTED_SIGNING_ENDPOINT,

    [string]$Account = $env:TRUSTED_SIGNING_ACCOUNT,

    [string]$CertificateProfile = $env:TRUSTED_SIGNING_PROFILE,

    [string]$TimestampUrl = "http://timestamp.acs.microsoft.com",

    [string]$ClientVersion = "1.0.60"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

foreach ($required in @{ Endpoint = $Endpoint; Account = $Account; CertificateProfile = $CertificateProfile }.GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace($required.Value)) {
        throw "$($required.Key) was not supplied. Pass -$($required.Key) or set the matching TRUSTED_SIGNING_* environment variable."
    }
}

$signtool = Get-ChildItem (Join-Path $env:USERPROFILE ".nuget\packages\microsoft.windows.sdk.buildtools") `
    -Filter signtool.exe -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.DirectoryName -like "*\x64" } |
    Sort-Object FullName |
    Select-Object -Last 1

if (-not $signtool) {
    throw "signtool.exe was not found. Run 'dotnet restore src\AntiAway\AntiAway.csproj -p:Platform=x64 -r win-x64' first."
}

$toolsDir = Join-Path $repoRoot "artifacts\tools\trusted-signing-$ClientVersion"
$dlib = Join-Path $toolsDir "bin\x64\Azure.CodeSigning.Dlib.dll"

if (-not (Test-Path $dlib)) {
    Write-Host "Downloading Microsoft.Trusted.Signing.Client $ClientVersion..."
    $nupkg = Join-Path ([System.IO.Path]::GetTempPath()) "trusted-signing-$ClientVersion.zip"
    $uri = "https://www.nuget.org/api/v2/package/Microsoft.Trusted.Signing.Client/$ClientVersion"
    Invoke-WebRequest -Uri $uri -OutFile $nupkg -UseBasicParsing
    if (Test-Path $toolsDir) { Remove-Item $toolsDir -Recurse -Force }
    Expand-Archive -Path $nupkg -DestinationPath $toolsDir -Force
    Remove-Item $nupkg -Force
}

if (-not (Test-Path $dlib)) {
    throw "Azure.CodeSigning.Dlib.dll was not found under $toolsDir."
}

$metadata = Join-Path $toolsDir "metadata.json"
@{
    Endpoint               = $Endpoint
    CodeSigningAccountName = $Account
    CertificateProfileName = $CertificateProfile
} | ConvertTo-Json | Set-Content -Path $metadata -Encoding utf8

$resolved = @($Path | ForEach-Object { (Resolve-Path $_).Path })

& $signtool.FullName sign /v /fd SHA256 /tr $TimestampUrl /td SHA256 /dlib $dlib /dmdf $metadata @resolved

if ($LASTEXITCODE -ne 0) {
    throw "signtool failed with exit code $LASTEXITCODE."
}

& $signtool.FullName verify /pa /v @resolved

if ($LASTEXITCODE -ne 0) {
    throw "Signature verification failed with exit code $LASTEXITCODE."
}

Write-Host "Signed $($resolved.Count) file(s)."
