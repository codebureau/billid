# Contributing

Thanks for your interest in contributing to barebill.

---

## Getting started

1. Fork the repository on GitHub
2. Clone your fork:

    ```powershell
    git clone https://github.com/<your-username>/barebill
    cd barebill
    dotnet build src/WorkTracking.slnx
    ```

3. Confirm tests pass:

    ```powershell
    dotnet test src/WorkTracking.slnx
    ```

---

## Branch and PR workflow

- **One branch per issue** — create a branch named `feat/<issue-number>-<short-description>`
- **Never commit directly to `main`**
- Open a pull request targeting `main`; a human approves all PRs before merge
- Reference the issue in your PR body: `Closes #<issue-number>`

```powershell
git checkout main && git pull
git checkout -b feat/42-my-feature
# ... make changes ...
git push --set-upstream origin feat/42-my-feature
gh pr create --title "feat: my feature (#42)" --base main
```

## Releasing

See the [Releasing guide](releasing.md) for the full tag-based release workflow.

---

---

## Coding conventions

Key rules — see [`.github/copilot-instructions.md`](https://github.com/codebureau/barebill/blob/main/.github/copilot-instructions.md) for the full reference.

- C# 12, nullable reference types enabled everywhere
- `async`/`await` for all database operations — no `.Result` or `.Wait()`
- ViewModels must not reference any `Window`, `MessageBox`, or WPF types directly
- Use `IDialogService` for dialogs, `INavigationService` for navigation
- `DateOnly` in domain models; convert at the data layer boundary only
- All DB access through repository interfaces — no raw SQL outside repositories
- Schema changes are additive migrations only — never `DROP TABLE` or `DROP COLUMN`

---

## Running tests

```powershell
dotnet test src/WorkTracking.slnx
```

Repository integration tests use `SqliteTestFixture`, which creates a fresh temp-file database per test class. No test writes to the real app database.

Test naming convention: `MethodName_Scenario_ExpectedOutcome`

---

## Commit messages

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add invoice frequency warning (#42)
fix: resolve null reference in ClientListViewModel (#37)
docs: update getting started guide
refactor: extract invoice line calculation to Core
```
