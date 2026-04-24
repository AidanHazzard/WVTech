# Pre-PR Checklist

Run this before opening any PR to `dev`. Executes the full pre-PR workflow for this project.

## Steps

1. **Identify the current branch** — confirm you are on a feature branch (not `dev` or `main`).

2. **Merge latest `dev`** — fetch and perform a dry-run merge to check for conflicts before touching the working tree:
   ```
   git fetch origin dev
   git merge --no-commit --no-ff origin/dev
   ```
   - If the dry-run reports conflicts, run `git merge --abort` to restore the working tree, report the conflicting files, and stop. Do not proceed until the user resolves the conflicts manually.
   - If the dry-run is clean (no conflicts), run `git merge --abort` to undo the staged dry-run, then perform the real merge:
     ```
     git merge origin/dev
     ```
   - If the branch was already up to date, proceed directly.

3. **Run unit tests**:
   ```
   dotnet test ./src/MealPlanner/MealPlanner.Tests/MealPlanner.Tests.csproj
   ```
   Report pass/fail and any failing test names.

4. **Run BDD integration tests**:
   Before running, check for the test database connection string in this order:
   1. If a test connection string is recorded in `CLAUDE.local.md`, use it to set the `ConnectionString` environment variable automatically before running the tests.
   2. If it is not in `CLAUDE.local.md`, check whether the `ConnectionString` environment variable is already set.
   3. If neither is available, stop and warn the user — the test suite calls `EnsureDeleted()` on startup and will wipe whatever database it connects to. Never proceed without a confirmed test database connection string.

   After resolving the connection string, if it is not already saved in `CLAUDE.local.md`, ask the user: *"Would you like me to save this connection string to CLAUDE.local.md so it can be reused? It is gitignored and will not be committed."* Only write it if the user confirms.

   The integration test suite starts the app itself as part of `BDDSetup` — do not ask the user to run the app manually before executing this step.
   ```
   dotnet test ./src/MealPlanner/MealPlanner.IntegrationTests/Mealplanner.IntegrationTests.csproj
   ```
   Report pass/fail and any failing scenario names.

5. **Summary** — report one of:
   - ✅ All checks passed. Safe to open a PR from `[branch]` → `dev`.
   - ❌ Checks failed. List what needs to be fixed before the PR can be opened.

Do not open or suggest opening the PR yourself. Just report the result and let the user decide.
