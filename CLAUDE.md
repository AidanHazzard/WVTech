# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**OneBite** is an ASP.NET Core 10 meal planning web application. Users can search recipes via the Edamam API, plan meals on a calendar, track nutrition/calories, manage shopping lists, and get meal recommendations based on their dietary goals.

**Team:** Willamette Valley Tech — Natalie Perez, Cooper Derville-Teer, Easton Pomrankey, Aidan Hazzard

**Sprint timeline** (2-week sprints, full release AES on 2026-05-28):
- Sprint 5: 2026-04-13 → 2026-04-27 ← *current*
- Sprint 6: 2026-04-27 → 2026-05-11
- Sprint 7: 2026-05-11 → 2026-05-25
- AES (full release): 2026-05-28
- Sprint 8: 2026-05-25 → 2026-06-08 (bug fixes, documentation)

---

## Commands

### Run the app
```bash
dotnet run --project ./src/MealPlanner/MealPlanner/MealPlanner.csproj
```

### Build
```bash
dotnet build src/MealPlanner/MealPlanner --configuration Release
```

### Unit tests (NUnit + Moq)
```bash
dotnet test ./src/MealPlanner/MealPlanner.Tests/MealPlanner.Tests.csproj
```

Run a specific test class or method:
```bash
dotnet test ./src/MealPlanner/MealPlanner.Tests/MealPlanner.Tests.csproj --filter "FullyQualifiedName~ClassName"
dotnet test ./src/MealPlanner/MealPlanner.Tests/MealPlanner.Tests.csproj --filter "FullyQualifiedName~ClassName.MethodName"
```

### Integration tests (BDD — Reqnroll + Selenium)
```bash
dotnet test ./src/MealPlanner/MealPlanner.IntegrationTests/Mealplanner.IntegrationTests.csproj
```

Integration tests require a running app and browser driver. ChromeDriver 146 and GeckoDriver 0.36 are included.

**Required environment variable:** `ConnectionString` must be set to a dedicated test database connection string before running integration tests. The test suite calls `EnsureDeleted()` on setup — **the entire database is wiped on every run**. Never point this at a dev or production database.

```bash
# Example (set before running tests)
export ConnectionString="Server=localhost;Database=MealPlannerTest;Trusted_Connection=true;"
```

The value is read from the `ConnectionString` environment variable first, then falls back to the NUnit test parameter `ConnectionString` if the env var is not set (`BDDSetup.cs` line 21).

### Database migrations
```bash
dotnet ef migrations add <MigrationName> --project ./src/MealPlanner/MealPlanner
dotnet ef database update --project ./src/MealPlanner/MealPlanner
```

---

## Architecture

**Pattern:** MVC + Repository Pattern + Dependency Injection

```
Controller → Service → Repository → EF Core DbContext → SQL Server
```

All DI registrations are in `Program.cs`. There are no service locator patterns — everything flows through constructor injection.

### Layers

- **Controllers** (`Controllers/`) — 9 controllers handling HTTP. Thin: they delegate to services and return views.
- **Services** (`Services/`) — All business logic lives here. Key services:
  - `EdamamService` — calls Edamam external API for recipe search/nutrition data. Disabled when `appsettings.Staging.json` sets `NoApi: "true"`.
  - `MealRecommendationService` — selects meals based on user calorie targets and dietary restrictions.
  - `NutritionProgressService` — aggregates daily calorie/macro data.
  - `SeedService` — loads 9 starter recipes on first run (IDs -1 to -9).
  - `EmailService` — sends verification emails via SMTP (credentials from Azure Key Vault in prod, user secrets in dev).
- **DAL** (`DAL/Abstract/` + `DAL/Concrete/`) — 8 repository interfaces with concrete EF Core implementations. Uses a generic `IRepository<T>` base.
- **Filters** (`Filters/`) — `ThemeFilter` is a global action filter that injects the user's dark/light theme preference into `ViewData` on every request.

### Database

`MealPlannerDBContext` extends `IdentityDbContext<User>`. ASP.NET Core Identity handles auth. Key domain models:

- `User` → `UserProfile`, `UserNutritionPreference`, `Meal`, `UserRecipe`, `UserDietaryRestriction`
- `Meal` → many-to-many `Recipe`
- `Recipe` → `Ingredient` → `IngredientBase` + `Measurement`
- `UserRecipe` carries a `VoteStatus` enum (UpVote/DownVote/None) for favorites

The connection string key is `ConnectionStrings:DefaultConnection` (falls back to `ConnectionString` env var). Retry-on-failure is enabled for SQL Server.

### Configuration & Secrets

- **Development:** `dotnet user-secrets` (UserSecretsId: `17e3dcd6-db0f-4d3b-b020-0fafc60d666d`). Secrets needed: `EmailSettings:Password`, `Edamam:AppId`, `Edamam:ApiKey`.
- **Production:** Azure Key Vault at `https://onebitekeys.vault.azure.net/` accessed via `DefaultAzureCredential`.
- **Staging:** `appsettings.Staging.json` sets `NoApi: "true"`, which bypasses the Edamam API.

### Testing approach

- **Unit tests** use `Microsoft.EntityFrameworkCore.InMemory` and Moq to isolate every layer. Controller tests mock services; service tests mock repositories.
- **Integration tests** are BDD (Gherkin `.feature` files in `MealPlanner.IntegrationTests/Features/`). Feature files follow Jira ticket naming (`wvt-*.feature`). Step definitions are in `StepDefinitions/`.
- **JS tests** use Jest + Babel but are currently a placeholder.

### Development workflow (BDD + TDD)

For every feature, follow this order — never write implementation code before tests exist:

1. Translate acceptance criteria into Gherkin `.feature` files (Given/When/Then) in `MealPlanner.IntegrationTests/Features/`
2. Write step definitions and any relevant unit tests
3. Run the tests and confirm they **fail** (red)
4. Write the minimum implementation code to make them **pass** (green)

### Git workflow

This project uses a variation of Gitflow:

1. Sync `dev` with upstream before starting work
2. Create a feature branch off `dev`
3. Do all work on the feature branch
4. Before opening a PR: merge latest `dev` into the feature branch, resolve any conflicts, then run both unit tests and BDD integration tests
5. Open a PR from the feature branch into `dev` (never directly into `main`)
6. `main` is only updated by the maintainer at the end of each 2-week sprint

Never suggest opening a PR to `main` mid-sprint. Always target `dev`.

### External API reference

- **Edamam Recipe Search API v2** — https://api.edamam.com/doc/open-api/recipe-search-v2.yaml
  Used by `Services/EdamamService.cs`. Consult this when modifying recipe search, nutrition data parsing, or the nutrient key mappings (`ENERC_KCAL`, `PROCNT`, `CHOCDF`, `FAT`).

### CI/CD

- `.github/workflows/integration-test.yml` — runs unit + integration tests on push/PR, targets .NET 10 preview.
- `.github/workflows/deploy_OneBite.yml` — deploys to the **OneBite** Azure Web App on pushes to `main`.

### Identity & auth details

- Email confirmation is required before sign-in.
- Passwords: 6+ chars, must include a digit and lowercase letter.
- Cookie auth: 30-day sliding expiration, HttpOnly + Secure + SameSite=Strict; unauthorized requests redirect to `/Login`.

---

## Key Implementation Details

### NoApi feature flag
`NoApi: "true"` in config completely prevents `EdamamService` from being registered in DI — the HttpClient factory is never set up. Controllers and services receive a `null` injection and must null-check before calling external API methods. `FoodEntriesController` guards with `if (_externalRecipeService != null)`. `MealController` injects `IMealRecommendationService?` as nullable and returns HTTP 500 if it's null and the endpoint is called.

### Recipe deduplication in `RecipeRepository.CreateOrUpdate`
When saving a recipe with ingredients, the repository deduplicates `IngredientBase` and `Measurement` rows. It first queries the DB for an existing row by name; if not found, it checks a `HashSet` of already-added entries within the same request. This prevents unique-index violations on `IngredientBase.Name` and `Measurement.Name` when saving recipes with shared ingredients.

### Ingredient auto-include
`OnModelCreating` configures `Ingredient` to auto-include `IngredientBase` and `Measurement` on every query. There's no need to `.Include()` these manually, but be aware of the performance implication when loading large collections of recipes.

### Weekly repeating meals
`Meal.RepeatRule = "Weekly"` causes `MealRepository.GetUserMealsByDateAsync` to return the meal for any future date that shares the same `DayOfWeek`. The query unions exact-date meals with weekly-repeat meals and deduplicates by Id.

### Theme injection
`ThemeFilter` (registered globally in `Program.cs`) stores `"dark"` or `"light"` in `HttpContext.Items["Theme"]` after querying `UserProfile.IsDarkTheme`. Views read this value. `UserSettingsController.ToggleTheme` is decorated with `[IgnoreAntiforgeryToken]` because it's called via `themeToggle.js` without a form token.

### External recipe caching
When a user selects an external Edamam recipe, the frontend calls `POST /api/recipe/external`, which fetches the full recipe from Edamam and stores it locally with the original `ExternalUri`. Subsequent searches find it locally. Local recipe search (`RecipeRepository.GetRecipesByName`) explicitly excludes rows where `ExternalUri != null`.

### `UserRecipe` lifecycle
`UserRecipe` rows are deleted when they become "redundant" — i.e., `UserFavorite = false`, `UserOwner = false`, and `UserVote = NoVote`. The `Redundant()` method on the model drives this. Vote and favorite changes call this check before deciding whether to delete or update.

### Recommendation algorithm (`MealRecommendationService`)
1. Load already-planned meals for the target date and subtract their calories from the user's `CalorieTarget`.
2. Prioritize the user's up-voted recipes (excluding those already in the day's plan).
3. Fill remaining slots from all other recipes ordered by vote percentage (excluding down-voted).
4. Stop when adding the next recipe would exceed the remaining calorie budget, or 5 recipes are reached.
`GetOneRecipeRecommendation` only returns a recipe if total calories stay under the target.

### Seeded data
`SeedService` (called at startup before `app.Run()`) runs `Database.MigrateAsync()`, then seeds:
- **Dietary restrictions** (8 types) if the table is empty.
- **Roles**: Admin, User.
- **Admin user**: `admin@codehub.com` / `Admin@123`.
The 9 seed recipes (IDs -1 to -9) are seeded via `OnModelCreating` `HasData`, not `SeedService`.

### BDD integration test conventions
- Test users are created with `PasswordHasher` directly against the DB (not via the registration flow).
- Password for all test users: `1234!Abcd`.
- Email format: `{username}@fakeemail.com`.
- `BDDSetup.CreateContext()` gives tests direct DB access for assertions.
- `SharedSteps.cs` handles login/logout lifecycle around each scenario.

### Client-side JS endpoints
| File | Endpoints called |
|---|---|
| `recipeSearch.js` | `GET /api/recipe/search`, `POST /api/recipe/external`, `DELETE /Meal/DeleteRecipeFromMeal` |
| `recipeVote.js` | `PUT /api/recipe/vote`, `GET /api/recipe/rating` |
| `themeToggle.js` | `POST /UserSettings/ToggleTheme` |
| `viewMeal.js` | `DELETE /Meal/DeleteRecipeFromMeal` |

---

## Known Bugs & Issues

### Critical

**`FavoritesController.Add()` — null recipe passed to repository**
`_recipeRepository.Read(recipeId)` returns `null` for unknown IDs. The result is passed directly to `_userRecipeRepository.AddFavoriteAsync(user, recipe)` without a null check. `AddFavoriteAsync` immediately dereferences `recipe.Id`, causing a `NullReferenceException`. Fix: return `NotFound()` if `recipe == null`.
_File: `Controllers/FavoritesController.cs` lines 30–44_

**`FavoritesController` — CSRF missing on Add/Remove**
Both `Add` and `Remove` POST actions are missing `[ValidateAntiForgeryToken]`. Any authenticated user can be tricked into adding/removing favorites via a cross-site request.
_File: `Controllers/FavoritesController.cs` lines 30–59_

**`MealController` — CSRF missing on multiple POST actions**
`NewMeal`, `GenerateMeal`, `EditMeal`, `AddRecipeToMeal`, and `DeleteRecipeFromMeal` are all POST actions without `[ValidateAntiForgeryToken]`. Note: `DeleteRecipeFromMeal` is called via JS `fetch`, so it needs `[IgnoreAntiForgeryToken]` or a JS-supplied token, not simply ignoring the issue.
_File: `Controllers/MealController.cs` lines 70, 121, 203, 255, 296_

**`FoodEntriesController` — CSRF missing on RecipeAdded and RecipeEditFinished**
Both POST actions that create/modify recipes lack `[ValidateAntiForgeryToken]`.
_File: `Controllers/FoodEntriesController.cs` lines 129, 180_

### High

**`EdamamService` — missing nutrient keys cause `KeyNotFoundException`**
`TotalNutrients?["ENERC_KCAL"]` uses dictionary indexer, not `.TryGetValue`. If the API response omits a nutrient key entirely, this throws `KeyNotFoundException` rather than defaulting to 0. The null-conditional only guards against a null `TotalNutrients` dict, not a missing key.
_File: `Services/EdamamService.cs` lines 67–70_

**`EdamamService` — API errors surface as unhandled exceptions**
`SearchExternalRecipesByName` and `GetExternalRecipeByURI` both `throw new Exception(...)` on non-200 responses. Callers catch with broad `catch (Exception)` and log to console only, silently returning incomplete results to the user.
_File: `Services/EdamamService.cs` lines 24–72_

**`Program.cs` — Azure Key Vault failure crashes startup**
`secretClient.GetSecret(...)` calls are not wrapped in try/catch. Any authentication failure or network error during startup throws and prevents the app from starting entirely.
_File: `Program.cs` lines 19–30_

### Medium

**`ThemeFilter` — no default theme when `UserProfile` is missing**
If a user is authenticated but has no `UserProfile` row, `HttpContext.Items["Theme"]` is never set. Views that read this key will either throw or render without a theme. Fix: set a default `"light"` when `profile == null`.
_File: `Filters/ThemeFilter.cs` lines 16–30_

**`MealRepository.GetUserMealsByDateAsync()` — weekly meals fetched twice**
A meal where `StartTime` falls on the queried date AND `RepeatRule == "Weekly"` appears in both the exact-date query and the weekly-repeat query. `GroupBy(m => m.Id)` deduplicates correctly, but the meal is loaded from the DB twice and EF tracks two instances unnecessarily.
_File: `DAL/Concrete/MealRepository.cs` lines 15–43_

### Configuration keys reference
| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | Primary DB connection string |
| `NoApi` | `"true"` disables Edamam API registration |
| `EmailSettings:SmtpServer` | SMTP host (smtp.gmail.com) |
| `EmailSettings:Port` | SMTP port (587) |
| `EmailSettings:SenderEmail` | From address (wvtech26@gmail.com) |
| `EmailSettings:Username` | SMTP auth username |
| `EmailSettings:Password` | SMTP auth password (secret) |
| `Edamam:AppId` / `Edamam:ApiKey` | Edamam API credentials (secret) |
| `AzureKeyVault:VaultUri` | `https://onebitekeys.vault.azure.net/` |

---

## Local Dev Setup

`Properties/launchSettings.json` is gitignored — each developer configures their own or uses `dotnet run` directly. There is no shared launch profile in the repo.

Steps to get running locally:
1. Install .NET 10 SDK
2. Set up SQL Server locally and note the connection string
3. Set user secrets:
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>" --project ./src/MealPlanner/MealPlanner
   dotnet user-secrets set "EmailSettings:Password" "<gmail-app-password>" --project ./src/MealPlanner/MealPlanner
   dotnet user-secrets set "Edamam:AppId" "<app-id>" --project ./src/MealPlanner/MealPlanner
   dotnet user-secrets set "Edamam:ApiKey" "<api-key>" --project ./src/MealPlanner/MealPlanner
   ```
4. `dotnet run --project ./src/MealPlanner/MealPlanner/MealPlanner.csproj`
5. On first run, `SeedService` auto-migrates the DB and seeds dietary restrictions, roles, and the admin user (`admin@codehub.com` / `Admin@123`).

To run without the Edamam API (no credentials needed), set `NoApi: "true"` in `appsettings.Development.json`. Recommendation and external recipe features will be unavailable.

---

## Planned Features Not Yet Implemented

These appeared in the requirements analysis (`docs/requirements_analysis.md`) but are not in the current codebase. They may come up in future sprints:

- **Pantry tracking** — inventory of ingredients with quantities, expiration dates, and optional auto-deduction when a recipe is used.
- **Budget tracking** — per-week/month budget limit, estimated cost per recipe/ingredient, over-budget flag.
- **Meal type scheduling** — Breakfast/Lunch/Dinner categorization on the calendar (currently meals have only a title and time, no meal-type enum).
- **Prep/cook time on recipes** — `EstimatedCookTime` field was in requirements but `Recipe` model only has macros and directions.
- **Nutrient timing preferences** — time-window targets (e.g., high protein at breakfast). Current `UserNutritionPreference` only has daily totals.
- **Tags on recipes** — filtering/searching by tag was in requirements; no `Tag` entity exists yet.
- **Notification/reminder system** — flagged as unclear in requirements; not implemented.
