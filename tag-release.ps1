# tag-release.ps1 — create and push a semver release tag
# Designed to run from Visual Studio Tools menu (output piped to Output window).
# Uses GUI dialogs instead of Read-Host so it works without a console stdin.

Add-Type -AssemblyName Microsoft.VisualBasic
Add-Type -AssemblyName System.Windows.Forms

Set-Location $PSScriptRoot

# ── 1. Check working tree is clean ────────────────────────────────────────────
$status = git status --porcelain
if ($status) {
    $msg = "Uncommitted changes detected:`n$($status -join "`n")`n`nProceed anyway?"
    $result = [System.Windows.Forms.MessageBox]::Show(
        $msg, "Uncommitted Changes",
        [System.Windows.Forms.MessageBoxButtons]::YesNo,
        [System.Windows.Forms.MessageBoxIcon]::Warning)
    if ($result -ne [System.Windows.Forms.DialogResult]::Yes) {
        Write-Host "Aborted — commit or stash changes first."
        exit 1
    }
}

# ── 2. Get last tag for reference ─────────────────────────────────────────────
$lastTag = git describe --tags --abbrev=0 2>$null
$prompt = if ($lastTag) { "Last release: $lastTag`n`nEnter new version (e.g. 1.2.0):" } else { "Enter version (e.g. 1.0.0):" }

# ── 3. Prompt for new version via GUI input box ───────────────────────────────
$version = [Microsoft.VisualBasic.Interaction]::InputBox($prompt, "Tag Release", "")

if ([string]::IsNullOrWhiteSpace($version)) {
    Write-Host "Aborted."
    exit 0
}

$version = $version.Trim().TrimStart('v')

if ($version -notmatch '^\d+\.\d+\.\d+$') {
    [System.Windows.Forms.MessageBox]::Show(
        "Invalid format '$version' — expected MAJOR.MINOR.PATCH (e.g. 1.2.0)",
        "Invalid Version", [System.Windows.Forms.MessageBoxButtons]::OK,
        [System.Windows.Forms.MessageBoxIcon]::Error) | Out-Null
    exit 1
}

$tag = "v$version"

# ── 4. Confirm ────────────────────────────────────────────────────────────────
$confirm = [System.Windows.Forms.MessageBox]::Show(
    "Create and push tag: $tag`n`nThis will trigger the GitHub Actions release workflow.",
    "Confirm Release",
    [System.Windows.Forms.MessageBoxButtons]::OKCancel,
    [System.Windows.Forms.MessageBoxIcon]::Question)

if ($confirm -ne [System.Windows.Forms.DialogResult]::OK) {
    Write-Host "Aborted."
    exit 0
}

# ── 5. Create and push the tag ────────────────────────────────────────────────
git tag $tag
if ($LASTEXITCODE -ne 0) { Write-Host "ERROR: git tag failed."; exit 1 }

git push origin $tag
if ($LASTEXITCODE -ne 0) { Write-Host "ERROR: git push failed."; exit 1 }

Write-Host ""
Write-Host "Tag $tag pushed successfully."
Write-Host "GitHub Actions release workflow is now running."
Write-Host "https://github.com/codebureau/billable/actions"
