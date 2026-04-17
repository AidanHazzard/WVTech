# New BDD Scenario

Convert acceptance criteria into Gherkin scenarios following the conventions of this project.

## Instructions

The user will provide acceptance criteria — either pasted directly or as a Jira ticket ID. Produce one or more Gherkin scenarios using Given/When/Then syntax.

### Conventions to follow

- **Feature file location:** `src/MealPlanner/MealPlanner.IntegrationTests/Features/`
- **File naming:** `wvt-{ticket-number}.feature` (e.g. `wvt-150.feature`). If no ticket number is given, ask for one before writing the file.
- **Scenario titles:** short, imperative, specific (e.g. `Scenario: User adds a recipe to an existing meal`)
- **Background:** use a `Background:` block for steps shared across all scenarios in the file (e.g. login, navigation to a page)
- **Given:** system/user state before the action
- **When:** the single action the user takes
- **Then:** the observable outcome, ideally assertable against the UI or database
- **Scenario Outline / Examples:** use when the same flow needs to be tested with multiple data values
- **No implementation detail in steps** — steps should describe behaviour, not CSS selectors or method names

### Output format

1. Show the complete `.feature` file content
2. List any step definitions that will need to be created in `src/MealPlanner/MealPlanner.IntegrationTests/StepDefinitions/`
3. Flag any acceptance criteria that are ambiguous or untestable as written, and suggest clarifying questions before writing tests for them

After the user confirms the scenarios are correct, write the `.feature` file to disk. Do not write it until the user has reviewed and approved.
