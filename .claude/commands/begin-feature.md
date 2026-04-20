# Begin Feature

Start work on a new feature by syncing dev, creating a branch, fetching the Jira ticket, and converting its acceptance criteria into Gherkin scenarios.

## Usage

`/begin-feature <jira-ticket-id-or-feature-name>` (e.g. `/begin-feature WVT-150` or `/begin-feature "search for recipes"`)

## Instructions

### Step 0 — Load Jira credentials

Read `CLAUDE.local.md` and extract:
- **Email** — the value on the line starting with `- **Email:**`
- **API token** — the value on the line starting with `- **API token:**`

Use these for all Jira API calls in this skill. If either is missing, stop and tell the user to add them to `CLAUDE.local.md`.

### Step 1 — Resolve the Jira ticket

**If the argument looks like a Jira ticket ID (`WVT-\d+`):**
- Fetch it: `curl -s -u {email}:{token} https://homework5.atlassian.net/rest/api/3/issue/WVT-{number}`
- Extract: summary, description, and acceptance criteria (look in `description` → `content` blocks or a field named `acceptance criteria` / `Acceptance Criteria`)
- If the ticket is not found, stop and tell the user

**If a plain feature name is given instead:**
- Search the Jira backlog: `curl -s -u {email}:{token} "https://homework5.atlassian.net/rest/api/3/issuesearch?jql=project=WVT+AND+text~\"{feature-name}\"&fields=summary,status"`
- Present the top matches (key + summary + status) and ask the user to confirm which ticket to use
- If no matches are found, tell the user and ask them to provide a ticket ID directly
- Once confirmed, fetch the full ticket as above

### Step 2 — Sync dev and create the branch

```bash
git fetch origin
git checkout dev
git pull origin dev --ff-only
git checkout -b wvt-{number}
```

If `--ff-only` fails (dev has diverged), stop and tell the user to resolve it manually before continuing.

Branch name must be `wvt-{number}` in lowercase (e.g. `wvt-150`).

### Step 3 — Convert acceptance criteria to Gherkin

Using the acceptance criteria from the ticket, produce one or more Gherkin scenarios following project conventions:

- **Feature file location:** `src/MealPlanner/MealPlanner.IntegrationTests/Features/`
- **File naming:** `wvt-{ticket-number}.feature` (e.g. `wvt-150.feature`)
- **Feature header:** `Feature: {ticket summary}` followed by a blank line, then `# WVT-{number}`
- **Scenario titles:** short, imperative, specific (e.g. `Scenario: User adds a recipe to an existing meal`)
- **Background:** use a `Background:` block for steps shared across all scenarios (e.g. login, navigation)
- **Given:** system/user state before the action
- **When:** the single action the user takes
- **Then:** the observable outcome, assertable against the UI or database
- **Scenario Outline / Examples:** use when the same flow needs multiple data values
- **No implementation detail in steps** — steps describe behaviour, not CSS selectors or method names
- **Test users:** use Gary, Bob, Katy, or Jack (not 'alice')

**Assertions must be UI-only.** These are acceptance tests — every `Then` step must check something a real user can see in the browser. Never assert directly against the database. If you need to verify that a record was created, updated, or deleted, write a step that navigates to the page where that record would be visible and checks that it appears (or is absent) there.

### Step 4 — Present for review

Output:
1. The complete `.feature` file content
2. Any step definitions that will need to be created in `src/MealPlanner/MealPlanner.IntegrationTests/StepDefinitions/`
3. Any acceptance criteria that are ambiguous or untestable — flag them and suggest clarifying questions

Do **not** write any files until the user has reviewed and approved the scenarios.

### Step 5 — Write the feature file

After the user confirms, write the `.feature` file to disk and confirm which branch you're on.
