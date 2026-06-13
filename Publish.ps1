# Panoramic Data NuGet Publish Script (Standard)
# Tags the current commit with the NBGV version and pushes to trigger CI/CD publishing.
# Usage: .\Publish.ps1

$ErrorActionPreference = 'Stop'

$status = git status --porcelain
if ($status) {
Write-Error "Working tree is not clean. Commit or stash changes before publishing.`n$status"
exit 1
}

$branch = git rev-parse --abbrev-ref HEAD
if ($branch -ne 'main') {
Write-Error "Publishing is only supported from the 'main' branch (currently on '$branch')."
exit 1
}

git fetch origin main --quiet
$localHead = git rev-parse HEAD
$remoteHead = git rev-parse origin/main
if ($localHead -ne $remoteHead) {
Write-Error "Local branch is not up to date with origin/main. Pull or push first."
exit 1
}

# Ensure the dotnet global tools directory is on PATH so 'nbgv' resolves
# even in shells that were opened before the tool was installed.
$toolsDir = Join-Path $HOME '.dotnet/tools'
if ((Test-Path $toolsDir) -and (($env:PATH -split [IO.Path]::PathSeparator) -notcontains $toolsDir)) {
$env:PATH = "$toolsDir$([IO.Path]::PathSeparator)$env:PATH"
}

if (-not (Get-Command nbgv -ErrorAction SilentlyContinue)) {
Write-Error "nbgv (Nerdbank.GitVersioning) is not available. Install it with: dotnet tool install -g nbgv"
exit 1
}

$versionJson = nbgv get-version -f json | ConvertFrom-Json
$version = $versionJson.NuGetPackageVersion

if (-not $version) {
Write-Error "Failed to determine version from nbgv."
exit 1
}

$existingTag = git tag -l $version
if ($existingTag) {
Write-Error "Tag '$version' already exists."
exit 1
}

Write-Host "Tagging as $version ..." -ForegroundColor Cyan
git tag $version
git push origin $version

Write-Host "Published tag $version. CI will build and publish the package." -ForegroundColor Green
