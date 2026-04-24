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

Work through BDD scenarios **one at a time**. For each scenario:

1. Write the Gherkin scenario in the `.feature` file
2. Create any class/method **stubs** needed (interfaces, empty method bodies, etc.)
3. Write **unit tests** for those stubs — run them and confirm they **fail** (red)
4. Implement the minimum code to make the unit tests **pass** (green)
5. Confirm the BDD scenario itself passes end-to-end
6. Commit, then move to the next scenario

Never implement scenario N+1 before scenario N's BDD test passes. Never write all scenarios first and then implement everything — the per-scenario red→green cycle is the point.

Unit tests must cover: repository find-or-create logic, ViewModel conversion methods (RecipeFromRecipeVM, RecipeToRecipeVM, EditRecipeVMToModel), and controller constructor changes (update existing mocks when adding constructor parameters). Step definitions use `BDDSetup.Context` (not `BDDSetup.CreateContext()`).

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

### Jira

- **Instance:** https://homework5.atlassian.net
- **Ticket naming:** `WVT-{number}` (e.g. `WVT-99`). Feature files are named `wvt-{number}.feature` to match.
- **API:** `GET https://homework5.atlassian.net/rest/api/3/issue/WVT-{number}` — requires Basic Auth (email + API token). Token is stored in `CLAUDE.local.md` (gitignored).

### CI/CD

- `.github/workflows/integration-test.yml` — runs unit + integration tests on push/PR, targets .NET 10 preview.
- `.github/workflows/deploy_OneBite.yml` — deploys to the **OneBite** Azure Web App on pushes to `main`.

### Identity & auth details

- Email confirmation is required before sign-in.
- Passwords: 6+ chars, must include a digit and lowercase letter.
- Cookie auth: 30-day sliding expiration, HttpOnly + Secure + SameSite=Strict; unauthorized requests redirect to `/Login`.

---

## Repository Pattern & Data Access Layer

### Generic Repository (`IRepository<T>`)

The DAL uses a tiered repository pattern with a generic base (`IRepository<T>`) and specialized repositories that extend it:

```csharp
public interface IRepository<TEntity> where TEntity : class, new()
{
    public TEntity? Read(int id);
    public List<TEntity> ReadAll();
    public TEntity CreateOrUpdate(TEntity entity);
    public void Delete(TEntity entity);
    public bool Exists(int id);
}
```

- `Read(id)` returns `null` if not found — always null-check after calling it
- `CreateOrUpdate(entity)` adds new entities (id=0) and updates existing ones
- **Important:** `SaveChanges()` is NOT called in repositories; controllers call `_context.SaveChanges()` after repository operations

### Specialized Repositories

Repositories extend `Repository<T>` and add domain-specific methods:

```csharp
public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    public RecipeRepository(MealPlannerDBContext context) : base(context) { }
    
    public override Recipe CreateOrUpdate(Recipe recipe) { /* deduplication + validation */ }
    public List<Recipe> GetRecipesByName(string name) { /* search */ }
    public Task<Recipe?> ReadRecipeWithIngredientsAsync(int id) { /* eager load */ }
}
```

**Key patterns:**
- Override `CreateOrUpdate()` to add business logic (deduplication, validation, related entity resolution)
- Use `async Task<T>` for methods that do I/O (database queries)
- Return `List<T>`, not `IQueryable<T>` or `IEnumerable<T>` (execute queries in the repo)
- Access other DbSets via `context.Set<T>()` for deduplication/validation logic

**DI Registration** (in `Program.cs`):
```csharp
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IMealRepository, MealRepository>();
// ... other specialized repos
```

All repositories are `AddScoped` — one instance per HTTP request.

---

## Controller Design: Keep Them Thin

Controllers should be **HTTP handlers only** — they parse requests, validate input, delegate business logic to services/repositories, and return responses. No business logic should live in controllers.

### Controller Structure

```csharp
[Authorize]
public class MealController : Controller
{
    // Inject only repositories and services, never DbContext
    private readonly IRegistrationService _registrationService;
    private readonly IRecipeRepository _recipeRepo;
    private readonly IMealRepository _mealRepo;
    private readonly IMealRecommendationService? _recommendationService;
    private readonly MealPlannerDBContext _context;

    public MealController(
        IRegistrationService registrationService,
        IRecipeRepository recipeRepo,
        IMealRepository mealRepo,
        MealPlannerDBContext context,
        IMealRecommendationService? mealRecommendationService = null)
    {
        _registrationService = registrationService;
        _recipeRepo = recipeRepo;
        _mealRepo = mealRepo;
        _context = context;
        _recommendationService = mealRecommendationService;
    }
}
```

### Naming & DI Conventions

- **Field names:** `_camelCase` with underscore prefix
- **Inject repositories + services:** Never inject `DbContext` directly (goal: migrate all DB work to repositories)
- **Nullable optional dependencies:** Use `?` for features that may not be registered:
  ```csharp
  private readonly IMealRecommendationService? _recommendationService;
  // Constructor parameter with default null
  public MealController(..., IMealRecommendationService? mealRecommendationService = null)
  ```
  - Check before use: `if (_recommendationService != null) { ... }`

### Thin Controller Patterns

**Pattern 1: GET action → delegate to repo → render view**
```csharp
[HttpGet]
public async Task<IActionResult> NewMeal()
{
    ViewBag.AvailableTags = await _tagRepo.GetTagsByPopularityAsync();
    return View(new CreateMealViewModel { /* ... */ });
}
```

**Pattern 2: POST action → validate → call service/repo → persist → redirect**
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> NewMeal(CreateMealViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    var user = await _registrationService.FindUserByClaimAsync(User);
    if (user == null) return Challenge();

    // Do work via service/repo
    var meal = new Meal { /* ... */ };
    _mealRepo.CreateOrUpdate(meal);
    _context.SaveChanges(); // Controller's job
    
    return RedirectToAction("PlannerHome");
}
```

**Pattern 3: Handle null reads**
```csharp
var recipe = _recipeRepository.Read(recipeId);
if (recipe == null) return NotFound(); // Always check after Read()
```

### Authorization & CSRF

- `[Authorize]` on the controller redirects unauthorized requests to `/Login`
- POST actions MUST have `[ValidateAntiForgeryToken]` unless the endpoint has no side effects
- JS `fetch` calls that POST need either:
  - Token included in request body (from `@Html.AntiForgeryToken()`)
  - Or `[IgnoreAntiForgeryToken]` if the endpoint is idempotent/safe

**Known issue:** Several POST actions are missing `[ValidateAntiForgeryToken]` (see Known Bugs section below)

---

## Service Layer: Business Logic Home

Services encapsulate business logic. Controllers delegate complex operations to services, which in turn use repositories for data access.

### Service Dependencies

```csharp
public class MealRecommendationService : IMealRecommendationService
{
    private IUserRecipeRepository _userRecipeRepository;
    private IRecipeRepository _recipeRepository;
    private IMealRepository _mealRepository;
    private IUserNutritionPreferenceRepository _nutrionRepository;
    private IExternalRecipeService? _externalRecipeService;

    public MealRecommendationService(
        IUserRecipeRepository userRecipeRepository,
        IRecipeRepository recipeRepository,
        IMealRepository mealRepository,
        IUserNutritionPreferenceRepository nutritionRepository,
        IExternalRecipeService? externalRecipeService = null)
    {
        _userRecipeRepository = userRecipeRepository;
        _recipeRepository = recipeRepository;
        _mealRepository = mealRepository;
        _nutrionRepository = nutritionRepository;
        _externalRecipeService = externalRecipeService;
    }
}
```

**Services inject:**
- **Repositories** (for data access) — never `DbContext`
- **Other services** (for orchestration)
- **Optional dependencies** (marked `?`) for features that may be disabled

### Async Conventions

If a service calls an `async` repository method, the service method must also be `async Task<T>`:

```csharp
// ✅ Correct: repo is async, so service is async
public async Task<List<Recipe>> GetRecommendedRecipesForUser(User user, DateTime date)
{
    var recipes = await _recipeRepository.GetRecipesByVoteAsync(...);
    return recipes;
}

// ❌ Wrong: can't await in sync method
public List<Recipe> GetRecommendedRecipes(User user, DateTime date)
{
    var recipes = await _repo.GetRecipesAsync(); // Error
}
```

### Service Responsibilities

**What services DO:**
- Complex filtering, sorting, aggregation (e.g., recommendation algorithm)
- Multi-step workflows (e.g., user registration)
- Calculation/transformation logic
- External API integration (e.g., Edamam)

**What services DON'T do:**
- Call `SaveChanges()` (controller does that)
- Inject `DbContext` directly
- Return HTTP status codes or objects
- Handle HTTP concerns (headers, cookies, etc.)

### DI Registration

In `Program.cs`:
```csharp
builder.Services.AddScoped<INutritionProgressService, NutritionProgressService>();
builder.Services.AddScoped<IMealRecommendationService, MealRecommendationService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
builder.Services.AddScoped<ShoppingListService>();
builder.Services.AddTransient<IEmailService, EmailService>();
```

- `AddScoped` — one instance per HTTP request (most services)
- `AddTransient` — new instance every time (stateless utilities)

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
