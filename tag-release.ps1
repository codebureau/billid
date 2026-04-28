#!/usr/bin/env pwsh
# tag-release.ps1 — create and push a semver release tag
# Run from the repo root, or invoke via Tools > Tag Release in Visual Studio.

Set-Location $PSScriptRoot

# ── 1. Check working tree is clean ────────────────────────────────────────────
$status = git status --porcelain
if ($status) {
    Write-Host ""
    Write-Host "  Uncommitted changes detected:" -ForegroundColor Yellow
    $status | ForEach-Object { Write-Host "    $_" }
    Write-Host ""
    $proceed = Read-Host "  Proceed anyway? (y/N)"
    if ($proceed -ne 'y') { exit 1 }
}

# ── 2. Show the last tag so the user knows what to increment ──────────────────
$lastTag = git describe --tags --abbrev=0 2>$null
if ($lastTag) {
    Write-Host ""
    Write-Host "  Last release tag : $lastTag" -ForegroundColor Cyan
}

# ── 3. Prompt for new version ─────────────────────────────────────────────────
Write-Host ""
$rawVersion = Read-Host "  Enter new version (e.g. 1.2.0)"

if ([string]::IsNullOrWhiteSpace($rawVersion)) {
    Write-Host "  Aborted — no version entered." -ForegroundColor Yellow
    exit 0
}

$version = $rawVersion.Trim().TrimStart('v')

if ($version -notmatch '^\d+\.\d+\.\d+$') {
    Write-Host "  Invalid format '$version' — expected MAJOR.MINOR.PATCH (e.g. 1.2.0)" -ForegroundColor Red
    exit 1
}

$tag = "v$version"

# ── 4. Confirm ────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  Will create and push tag: $tag" -ForegroundColor Green
$confirm = Read-Host "  Confirm? (Y/n)"
if ($confirm -eq 'n') { exit 0 }

# ── 5. Create and push the tag ────────────────────────────────────────────────
git tag $tag
if ($LASTEXITCODE -ne 0) { Write-Host "  git tag failed" -ForegroundColor Red; exit 1 }

git push origin $tag
if ($LASTEXITCODE -ne 0) { Write-Host "  git push failed" -ForegroundColor Red; exit 1 }

Write-Host ""
Write-Host "  Tag $tag pushed — GitHub Actions release workflow is now running." -ForegroundColor Green
Write-Host "  https://github.com/codebureau/billable/actions" -ForegroundColor Cyan
Write-Host ""
