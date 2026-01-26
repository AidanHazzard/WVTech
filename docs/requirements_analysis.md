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
