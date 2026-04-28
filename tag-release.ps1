#!/usr/bin/env pwsh
# tag-release.ps1 — create and push a semver release tag
# Run from the repo root, or invoke via Tools > Tag Release in Visual Studio.

Set-Location $PSScriptRoot

Add-Type -AssemblyName Microsoft.VisualBasic
Add-Type -AssemblyName System.Windows.Forms

# ── 1. Check working tree is clean ────────────────────────────────────────────
$status = git status --porcelain
if ($status) {
    Write-Host ""
    Write-Host "  Uncommitted changes detected:" -ForegroundColor Yellow
    $status | ForEach-Object { Write-Host "    $_" }
    Write-Host ""
    $result = [System.Windows.Forms.MessageBox]::Show(
        "Uncommitted changes detected:`n`n$($status -join "`n")`n`nProceed anyway?",
        "Uncommitted Changes",
        [System.Windows.Forms.MessageBoxButtons]::YesNo,
        [System.Windows.Forms.MessageBoxIcon]::Warning)
    if ($result -ne [System.Windows.Forms.DialogResult]::Yes) {
        Write-Host "  Aborted." -ForegroundColor Yellow; exit 1
    }
}

# ── 2. Show the last tag so the user knows what to increment ──────────────────
$lastTag = git describe --tags --abbrev=0 2>$null
$prompt = if ($lastTag) { "Last release: $lastTag`n`nEnter new version (e.g. 1.2.0):" } else { "Enter new version (e.g. 1.0.0):" }
if ($lastTag) {
    Write-Host ""
    Write-Host "  Last release tag : $lastTag" -ForegroundColor Cyan
}

# ── 3. Prompt for new version (GUI input box) ─────────────────────────────────
Write-Host ""
$rawVersion = [Microsoft.VisualBasic.Interaction]::InputBox($prompt, "Tag Release", "")

if ([string]::IsNullOrWhiteSpace($rawVersion)) {
    Write-Host "  Aborted — no version entered." -ForegroundColor Yellow; exit 0
}

$version = $rawVersion.Trim().TrimStart('v')

if ($version -notmatch '^\d+\.\d+\.\d+$') {
    [System.Windows.Forms.MessageBox]::Show(
        "Invalid format '$version' — expected MAJOR.MINOR.PATCH (e.g. 1.2.0)",
        "Invalid Version",
        [System.Windows.Forms.MessageBoxButtons]::OK,
        [System.Windows.Forms.MessageBoxIcon]::Error) | Out-Null
    Write-Host "  Invalid version format '$version'." -ForegroundColor Red; exit 1
}

$tag = "v$version"

# ── 4. Confirm ────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  Will create and push tag: $tag" -ForegroundColor Green
$confirm = [System.Windows.Forms.MessageBox]::Show(
    "Create and push tag: $tag`n`nThis will trigger the GitHub Actions release workflow.",
    "Confirm Release",
    [System.Windows.Forms.MessageBoxButtons]::OKCancel,
    [System.Windows.Forms.MessageBoxIcon]::Question)
if ($confirm -ne [System.Windows.Forms.DialogResult]::OK) {
    Write-Host "  Aborted." -ForegroundColor Yellow; exit 0
}

# ── 5. Create and push the tag ────────────────────────────────────────────────
git tag $tag
if ($LASTEXITCODE -ne 0) { Write-Host "  git tag failed" -ForegroundColor Red; exit 1 }

git push origin $tag
if ($LASTEXITCODE -ne 0) { Write-Host "  git push failed" -ForegroundColor Red; exit 1 }

Write-Host ""
Write-Host "  Tag $tag pushed — GitHub Actions release workflow is now running." -ForegroundColor Green
Write-Host "  https://github.com/codebureau/billable/actions" -ForegroundColor Cyan
Write-Host ""
