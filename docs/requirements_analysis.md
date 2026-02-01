# Requirements Elicitation Analysis

#### User Account

1. For each attribute, term, entity, relationship, activity ... precisely determine its bounds, limitations, types and constraints in both form and function. Write them down.

##### Entity: User

**UserId**
  - Type: UUID/Integer  
  - Constraint: Unique, required

**Email**
  - Type: String  
  - Constraint: Optional (required if PhoneNumber not provided), unique

**PhoneNumber**
  - Type: String  
  - Constraint: Optional (required if Email not provided), unique

**PasswordHash**
  - Type: String  
  - Constraint: Required, never store plain-text password

**CreatedAt**
  - Type: DateTime  
  - Constraint: Required

**LastLoginAt**
  - Type: DateTime  
  - Constraint: Optional

**IsVerified**
  - Type: Boolean  
  - Constraint: Optional (depends on whether verification is required)


##### Entity: UserSession (only if “Remember Me” / persistent login is implemented)

**SessionId**
  - Type: UUID/Integer  
  - Constraint: Unique, required

**UserId**
  - Type: UUID/Integer  
  - Constraint: Required, FK → User.UserId

**RefreshToken**
  - Type: String  
  - Constraint: Required if persistent login enabled, must be stored securely

**ExpiresAt**
  - Type: DateTime  
  - Constraint: Required

**IsRevoked**
  - Type: Boolean  
  - Constraint: Required


##### Entity: UserPreference

**PreferenceId**
  - Type: UUID/Integer  
  - Constraint: Unique, required

**UserId**
  - Type: UUID/Integer  
  - Constraint: Required, FK → User.UserId

**PreferenceKey**
  - Type: String  
  - Constraint: Required

**PreferenceValue**
  - Type: String  
  - Constraint: Required


2. ##### Conflicts / Alignment

There is room for potential conflict between convenience and security. A “remember me” system makes login easier but requires security controls. Supporting both email and phone login also increases complexity because verification and recovery workflows can differ.


3. ##### Missing Items

Account verification, password reset/account recovery, changing email/phone number, and account deletion are not clearly defined.


4. ##### Elicitation Follow-Up

Further elicitation is needed to decide whether verification is required, whether persistent login is going to be factored, and what recovery flows should be supported.


---


#### Diet — Analysis

Go through all the information gathered during the previous round of elicitation.

1. For each attribute, term, entity, relationship, activity ... precisely determine its bounds, limitations, types and constraints in both form and function. Write them down.

##### Entity: DietaryRestriction

**RestrictionId**
  - Type: UUID/Integer  
  - Constraint: Unique, required

**UserId**
  - Type: UUID/Integer  
  - Constraint: Required, FK → User.UserId

**RestrictionType**
  - Type: Enum (Allergy, DietPreference, IngredientAvoidance, TextureModifiedDiet)  
  - Constraint: Required

**Value**
  - Type: String  
  - Constraint: Required

**IsActive**
  - Type: Boolean  
  - Constraint: Required


##### Entity: NutritionGoal

**GoalId**
  - Type: UUID/Integer  
  - Constraint: Unique, required

**UserId**
  - Type: UUID/Integer  
  - Constraint: Required, FK → User.UserId

**Nutrient**
  - Type: Enum (Calories, Protein, Carbs, Fat, etc.)  
  - Constraint: Required

**GoalType**
  - Type: Enum (Min, Max, TargetRange)  
  - Constraint: Required

**MinValue**
  - Type: Decimal  
  - Constraint: Optional (required for Min/Range)

**MaxValue**
  - Type: Decimal  
  - Constraint: Optional (required for Max/Range)

**Unit**
  - Type: Enum (kcal, g, mg)  
  - Constraint: Required

**Period**
  - Type: Enum (Daily, Weekly, PerMeal)  
  - Constraint: Required


##### Entity: DietTolerance

**ToleranceId**
  - Type: UUID/Integer  
  - Constraint: Unique, required

**UserId**
  - Type: UUID/Integer  
  - Constraint: Required, FK → User.UserId

**TolerancePercent**
  - Type: Integer (0–100)  
  - Constraint: Required

**AppliesTo**
  - Type: Enum (CaloriesOnly, MacrosOnly, AllGoals)  
  - Constraint: Required


##### Entity: NutrientTimingPreference

**TimingId**
  - Type: UUID/Integer  
  - Constraint: Unique, required

**UserId**
  - Type: UUID/Integer  
  - Constraint: Required, FK → User.UserId

**TimeWindow**
  - Type: Enum (Breakfast, Lunch, Dinner, Snack, CustomRange)  
  - Constraint: Required

**Nutrient**
  - Type: Enum (Protein, Carbs, Fat, Calories)  
  - Constraint: Required

**DesiredAmount**
  - Type: Decimal  
  - Constraint: Optional

**Unit**
  - Type: Enum (kcal, g, mg)  
  - Constraint: Required


2. ##### Conflicts / Alignment

Strict allergies can conflict with flexible tolerance rules, so allergies must override tolerance. Planned meals vs. actual consumption also need clear labeling to avoid misleading nutrient summaries.

3. ##### Missing Items

The nutrient list is undefined, along with editing/disable controls for restrictions and whether alerts are sent when goals are not being met.

4. ##### Elicitation Follow-Up

Further elicitation is required to clarify nutrient precision, tolerance behavior, and whether consumption is planned or logged.


---


#### Shopping List — Analysis

Go through all the information gathered during the previous round of elicitation.

1. For each attribute, term, entity, relationship, activity ... precisely determine its bounds, limitations, types and constraints in both form and function. Write them down.

##### Entity: ShoppingList

**ShoppingListId**
  - Type: UUID/Integer  
  - Constraint: Unique, required

**UserId**
  - Type: UUID/Integer  
  - Constraint: Required, FK → User.UserId

**CreatedAt**
  - Type: DateTime  
  - Constraint: Required

**Status**
  - Type: Enum (Active, Archived)  
  - Constraint: Required


##### Entity: ShoppingListItem

**ItemId**
  - Type: UUID/Integer  
  - Constraint: Unique, required

**ShoppingListId**
  - Type: UUID/Integer  
  - Constraint: Required, FK → ShoppingList.ShoppingListId

**Name**
  - Type: String  
  - Constraint: Required

**Quantity**
  - Type: Decimal  
  - Constraint: Optional/Required (depends on MVP decision)

**Unit**
  - Type: String/Enum  
  - Constraint: Optional

**Notes**
  - Type: String  
  - Constraint: Optional

**Source**
  - Type: Enum (Manual, CalendarIngredient, PantrySuggestion)  
  - Constraint: Required

**IsCheckedOff**
  - Type: Boolean  
  - Constraint: Required


##### Entity: PantryItem

**PantryItemId**
  - Type: UUID/Integer  
  - Constraint: Unique, required

**UserId**
  - Type: UUID/Integer  
  - Constraint: Required, FK → User.UserId

**Name**
  - Type: String  
  - Constraint: Required

**Quantity**
  - Type: Decimal  
  - Constraint: Optional

**Unit**
  - Type: String/Enum  
  - Constraint: Optional


2. ##### Conflicts / Alignment

Automatic pantry removal may conflict with user intent, and manual edits may conflict with auto-added ingredients unless merge rules are defined.

3. ##### Missing Items

Unit conversion, duplicate handling, export confirmation, and pantry synchronization rules are not defined.

4. ##### Elicitation Follow-Up

Further elicitation is needed to define auto-removal behavior, duplicate merging, and export expectations.


---


#### Pantry - Analysis

Go through all the information gathered during the previous round of elicitation.  
1. For each attribute, term, entity, relationship, activity ... precisely determine its bounds, limitations, types and constraints in both form and function.  Write them down.
- Entity: User
    - UserID
        - Type: UUID/Integer
        - Constraint: Unique, required
- Entity: Pantry
    - PantryID
        - Type: UUID/Integer
        - Constraint: Unique
- Entity: PantryItem
    - PantryItemID
        - Type: UUID/Integer
        - Constraint: Unique
    - ItemName
        - Type: String
        - Constraint: Required
    - ItemQuantity
        - Type: Decimal
        - Constraint: Must be greater than or equal to 0
    - ItemUnit
        - Type: Enum (Items, Cups, Lbs, etc.)
    - ItemExpirationDate
        - Type: Date
        - Constraint: Optional
- Entity: Ingredient
    - IngredientID
        - Type: UUID/Integer
        - Constraint: Unique
    - IngredientName
        - Type: String
        - Constraint: None


2. Do they work together or are there some conflicting requirements, specifications or behaviors?
    The entities work together without major conflicts. A potential conflict may arise if pantry quantities are expected to update automatically when recipes are selected, as this may require additional user confirmation or assumptions about actual usage.

3. Have you discovered if something is missing?
    Yes. It is still unclear whether expiration tracking and automatic quantity deduction are required features.

4. Return to Elicitation activities if unanswered questions remain.
    Additional elicitation is needed to determine how detailed pantry tracking should be and whether automation is expected.


---


#### Budget - Analysis

Go through all the information gathered during the previous round of elicitation.  

1. For each attribute, term, entity, relationship, activity ... precisely determine its bounds, limitations, types and constraints in both form and function.  Write them down.
- Entity: User
    - UserID
        - Type: UUID/Integer
        - Constraint: Unique, required
    - Location
        - Type: String (City/state or zip code)
        - Constraint: Optional(used for price estimates)
    - BudgetAmount
        - Type: Decimal
        - Constraint: must be greater than or equal to 0
    - BudgetPeriod
        - Type: Enum (Weekly, Biweekly, Monthly)
        - Constraint: Required
- Entity: Recipe
    - RecipeId
        - Type: UUID/Integer
        - Constraint: Unique, required
    - Name
        - Type: String
        - Constraints: None
    - Ingredients
        - Type: List\<Ingredients>
        - Constraint: None
    - EsitmatedCost
        - Type: Decimal
        - Constraint: Derived value not user entered
- Entity: Ingredient
    - IngredientId
        - Type: UUID/Integer/Id?
        - Constraint: Unique
    - Name
        - Type: String
        - Constraints: None
    - Quantity
        - Type: Decimal
        - Constraint: None
    - Unit
        - Type: Enum (Cups, Lbs, Items, etc.)
    - EstimatedCost
        - Type: Decimal
        - Constraint: Estimated average cost not actual cost
- Entity: BudgetSummary
    - BudgetId
        - Type: UUID/Integer/Id
        - Constraint: Unique
    - TotalBudget
        - Type: Decimal
        - Constraint: Potentially always changing
    - EstimatedSpending
        - Type: Decimal
        - Constraint: Estamated spening not actual apending
    - RemianingBudget
        - Type: Decimal
        - Constraint: None
    - OverBudgetFlag
        - Type: Boolean
        - Constraint: Doesn't apply if the user does not have a budget limit

2. Do they work together or are there some conflicting requirements, specifications or behaviors?
    There is potential conflict between providing accurate pricing and maintaining simplicity. Exact pricing may require real-time store data, while estimates may reduce accuracy but improve usability and performance.

3. Have you discovered if something is missing?  
    Further clarification is needed on whether tax, store selection, and price fluctuations should be included in budget calculations.

4. Return to Elicitation activities if unanswered questions remain.
    If real-time pricing or store-specific accuracy is required, additional reaserch will need to be done.


---


#### Time Management - Analysis

Go through all the information gathered during the previous round of elicitation.  

1. For each attribute, term, entity, relationship, activity ... precisely determine its bounds, limitations, types and constraints in both form and function.  Write them down.

- Entity: User
    - UserID
        - Type: UUID/Integer
        - Constraint: Unique, required
- Entity: Calendar
    - CalendarID
        - Type: UUID/Integer
        - Constraint: Unique
    - StartDate
        - Type: DateTime
        - Constraint: Required
    - EndDate
        - Type: DateTime
        - Constraint: Required
- Entity: MealPlan
     - MealPlanID
        - Type: UUID/Integer
        - Constraint: Unique
    - Date
        - Type: Date
        - Constraint: Must fall within calendar range
    - MealType
        - Type: Enum (Breakfast, Lunch, Dinner)
        - Constraint: Required
    - EstimatedPrepTime
        - Type: Integer (minutes)
        - Constraint: Optional, estimated value
- Entity: Recipe
    - RecipeID
        - Type: UUID/Integer
        - Constraint: Unique
    - RecipeName
        - Type: String
        - Constraint: None
    - EstimatedCookTime
        - Type: Integer (minutes)
        - Constraint: Estimated, not exact

2. Do they work together or are there some conflicting requirements, specifications or behaviors?
    The entities and activities work together without major conflict. A potential issue may arise if detailed time tracking is expected, as time estimates are approximate and may vary between users. However, this does not prevent the system from supporting basic scheduling and planning.

3. Have you discovered if something is missing?
    Yes. It is still unclear whether features such as reminders, notifications, or time-based alerts are part of the time management requirements.

4. Return to Elicitation activities if unanswered questions remain.
    Further elicitation is needed to determine whether reminder notifications or more detailed time tracking should be included.


---


#### Recipe Book - Analysis

Go through all the information gathered during the previous round of elicitation.  

1. For each attribute, term, entity, relationship, activity ... precisely determine its bounds, limitations, types and constraints in both form and function.  Write them down.

- Entity: Recipe
	- Name
		- Type: string
		- Constraint: Required
	- Tags
		- Type: List\<Tag>
		- Constraint: None
	- Ingredients
		- Type: List\<Ingredients>
		- Constraint: Must not be empty
	- Instructions
		- Type: string
		- Constraint: None
	- Description
		- Type: string
		- Constraint: None
	- Calories
		- Type: uint
		- Constraint: None
	- Nutrients
		- Type: Dictionary\<Nutrient, uint>
		- Constraint: None

- Entity: Ingredient
	- Base
		- Type: IngredientBase
		- Constraint: Required
	- Measurement
		- Type: Measurement
		- Constraint: Required
	- Amount
		- Type: uint
		- Constraint: Required
		- Notes: Amounts can be both countable (number of apples) and fractional (cups of flour), but if we use metric to store ingredient information we would only require Amount to be a whole number

- Entity: IngredientBase
	- id
		- Type: uint
		- Constraint: Required, Unique
	- Name
		- Type: string
		- Constraint: Required, Unique, case insensitive

- Entity: Measurement
	- id
		- Type: uint
		- Constraint: Unique, Required
	- Name
		- Type: string
		- Constraint: Unique, Required, case insensitive

- Entity: Tag
	- id
		- Type: uint
		- Constraint: Unique, Required
	- Name
		- Type: string
		- Constraint: Unique, Required, case insensitive

- Entity: Nutrient
	- id
		- Type: uint
		- Constraint: Unique, Required
	- Name
		- Type: string
		- Constraint: Unique, Required, case insensitive
	- Measurement
		- Type: Measurement
		- Constraint: Required


---


#### Calendar - Analysis

Go through all the information gathered during the previous round of elicitation.  

1. For each attribute, term, entity, relationship, activity ... precisely determine its bounds, limitations, types and constraints in both form and function.  Write them down.

- Entity: User
	- Calendar
		- Type: List\<Day>
		- Constraint: None
- Entity: Day
	- Date
		- Type: DateTime
		- Constraint: Required
	- Meals
		- Type: List\<Meal>
		- Constraint: None
- Entity: Meal
	- Time
		- Type: DateTime
		- Constraint: Required


---


## Meal Recommendation - Analysis

Go through all the information gathered during the previous round of elicitation.  

1. For each attribute, term, entity, relationship, activity ... precisely determine its bounds, limitations, types and constraints in both form and function.  Write them down.

- Entity: Meal
	- Recipes
		- Type: List\<Recipe>
		- Constraint: Required
	- Time
		- Type: DateTime
		- Constraint: Required

- Entity: Recipe
	- EstimatedCost
		- Type: float
		- Constraint: non-negative, stored client-side as prices are local to user
	- Calories
		- Type: uint
		- Constraint: None
	- Nutrients
		- Type: Dictionary\<Nutrient, uint>
		- Constraint: None
		- Note: the Nutrient entity is sufficiently defined for this elsewhere, so it will not be reiterated
	- EstimatedCookingTime
		- Type: uint
		- Constraint: number of seconds
	- Favorite
		- Type: bool
		- Default: false
		- Constraint: stored client-side
	- SiteRating
		- Type: float
		- Constraint: between 1-5
	- UserRating
		- Type: float
		- Constraint: between 1-5, stored client-side

- Entity: User
	- Budget
		- Type: float
		- Constraint: non-negative
	- BudgetPriority
		- Type: float
		- Constraint: between 0 and 1
	- Diet
		- Type: Diet
		- Constraint: Required
	- FoodPreferences
		- Type: Dictionary\<Tag, float>
		- Constraint: value is between 0 and 1, default 0.5
		- Note: the Tag entity is sufficiently defined for this elsewhere, so it will not be reiterated 
	- FoodPreferencePriority
		- Type: float
		- Constraint: between 0 and 1
	- MinRecipeRating
		- Type: float
		- Constraint: between 1 and 5

- Entity: Diet
	- CaloriesPerDay
		- Type: uint
		- Constraint: None
	- CalorieGoalTolerance
		- Type: uint
		- Constraint: None
	- CalorieGoalPriority
		- Type: float
		- Constraint: between 0 and 1
	- NutrientsPerDay
		- Type: Dictionary\<Nutrient, uint>
		- Constraint: None
	- NutrientsGoalTolerance
		- Type: Dictionary\<Nutrient, uint>
		- Constraint: None
	- NutrientGoalPriority
		- Type: Dictionary\<Nutrient, float>
		- Constraint: value is between 0 and 1
	- IngredientRestrictions
		- Type: List\<BaseIngredient>
		- Constraint: None
		- Note: the BaseIngredient entity is sufficiently defined for this elsewhere, so it will not be reiterated 