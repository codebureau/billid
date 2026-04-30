# Releasing

Releases are driven entirely by **Git tags** — there are no version numbers in the source code. Pushing a tag to `main` triggers the release workflow automatically.

---

## How it works

1. A tag (e.g. `v1.2.0`) is pushed to `main`
2. The [`release.yml`](https://github.com/codebureau/barebill/blob/main/.github/workflows/release.yml) workflow triggers
3. Tests run; if they pass, a self-contained `barebill.exe` is published for `win-x64`
4. A **GitHub Release** is created with the `.exe` attached and release notes auto-generated from merged PR titles

The binary has the tag version stamped into it (visible in **About barebill**) — no manual `.csproj` edits needed.

---

## Versioning scheme

barebill uses [Semantic Versioning](https://semver.org/):

| Increment | When to use |
|-----------|-------------|
| **Major** (`v2.0.0`) | Breaking change to data schema or fundamentally different behaviour |
| **Minor** (`v1.1.0`) | New feature shipped to users |
| **Patch** (`v1.0.1`) | Bug fix or minor tweak with no new feature |

Tags always use the `v` prefix: `v1.0.0`, `v1.1.0`, `v1.0.1`.

---

## Step-by-step: creating a release

### 1. Confirm `main` is ready

```powershell
git checkout main
git pull
dotnet test src/WorkTracking.slnx
```

All tests must pass before tagging.

### 2. Decide the version number

Look at the last tag to determine what changed since then:

```powershell
git log $(git describe --tags --abbrev=0)..HEAD --oneline
```

Apply the versioning rules above.

### 3. Create and push the tag

```powershell
git tag v1.1.0
git push origin v1.1.0
```

That's it — the release workflow handles everything else.

### 4. Verify the release

- Go to [Actions](https://github.com/codebureau/barebill/actions) and confirm the **Release** workflow passes
- Go to [Releases](https://github.com/codebureau/barebill/releases) and check that `barebill.exe` is attached and the release notes look correct

---

## Fixing a bad release

If a release needs to be pulled:

1. Delete the release on GitHub (Releases → Edit → Delete)
2. Delete the tag locally and remotely:
   ```powershell
   git tag -d v1.1.0
   git push origin :refs/tags/v1.1.0
   ```
3. Fix the issue, merge a PR to `main`, then re-tag

---

## Release notes

Release notes are **auto-generated** from merged PR titles since the previous tag. This means your PR titles matter — use the standard prefix format:

| Prefix | Appears under |
|--------|---------------|
| `feat:` | New Features |
| `fix:` | Bug Fixes |
| `docs:` | Documentation |

If a PR title needs to be corrected after merge, edit it on GitHub before tagging — the release notes are generated at tag time.
