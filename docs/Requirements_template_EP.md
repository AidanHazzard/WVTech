# Budget Requirements Workup

## Elicitation

1. Is the goal or outcome well defined?  Does it make sense?
    Yes, the goal/outcome is well defined as the budget aspect of our meal planner app has clear usage. 
    This makes sense becuse the average joe likes to budget their money and not overspend on uncessecary items.

2. What is not clear from the given description?
    What is not given from the clear decsription is how this will be specifically implimented into the project
    in terms of how we will aquire the overall cost of all of the recipes and ingridients for the recipes for 
    a users given location. Goroceries in seattle are more expensive than in salem.

3. How about scope?  Is it clear what is included and what isn't?
    what is includged is a general budget the user will provide in their user account or when they initially sign up.
    what is not clear is how we will decide how much items actually cost in the area  or if we will give a general 
    estimate. What we also do not know is what if the user doesnt have a large budget? Do we tell them they will be overbudget?
    Do we only provide recipes they can afford?

4. What do you not understand?

    * Technical domain knowledge
        - how pricing would work for any given area

    * Business domain knowledge
        - How grocery pricing works accross stores
        - How tax is included

5. Is there something missing?
    There may be missing details related to discounts, sales, or store-specific pricing. At this time, no additional missing requirements have been identified, but further elicitation may reveal more constraints.

6. Get answers to these questions.
 - Researching grocery and pricing API's.
 - Consulting documentation for barcode scanning and nutrition services.
 - Clarifying expectations with stakeholders on how precise budget estimates need to be.

## Analysis

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
        - Type: List<Ingrediants>
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

## Design and Modeling
Our first goal is to create a **data model** that will support the initial requirements.

1. Identify all entities;  for each entity, label its attributes; include concrete types
- User
    - UserId (UUID/Integer)
    - Location (String)
    - BudgetAmount (Decimal)
    - BudgetPeriod (Enum)
- Recipe
    - RecipeId (UUID/Integer)
    - Name (String)
    - EstimatedCost (Decimal)
- Ingredients
    - IngeredientsId (UUID/Integer)
    - Name (String)
    - Quantity (Decimal)
    - Unit (Enum)
    - EstimatedCost (Decimal)
- BudgetSummary
    - BudgetId (UUID/Integer)
    - TotalBudget (Decimal)
    - EstimatedSpending (Decimal)
    - RemainingBudget (Decimal)
    - OverBudgetFlag (Boolean)

2. Identify relationships between entities.  Write them out in English descriptions.
    - A User has one BudgetSummary
    - A User selects one or more Recipes
    - A Recipe contains one or more Ingredients
    - Each Ingredients contributes to the estimated cost of a Recipe
    - The combined estimated cost of selected Recipes contribues to the User's BudgetSummary

3. Draw these entities and relationships in an _informal_ Entity-Relation Diagram.
    User
     ├── has → BudgetSummary
     ├── selects → Recipe
     Recipe
     ├── contains → Ingredient
     Ingredient
     ├── contributes to → Recipe EstimatedCost
     BudgetSummary
     ├── tracks → EstimatedSpending
     ├── evaluates → OverBudgetFlag

4. If you have questions about something, return to elicitation and analysis before returning here.
    - Some aspects of the budget feature remain open and may require additional elicitation:
        - Whether the system should restrict recipe selection when user exceeds their budget or only provide warnings
        - Whether tax should be included in cost estimates
        - How pantry items may reduce estimated spending

## Analysis of the Design
The next step is to determine how well this design meets the requirements _and_ fits into the existing system.

1. Does it support all requirements/features/behaviors?
    * For each requirement, go through the steps to fulfill it.  Can it be done?  Correctly?  Easily?

    - User-defined budgets
        - The model supports user-entered budget amounts and time periods through the User and BudgetSummary entities.
    - Recipe and ingredient cost estimation
        - Estimated costs can be calculated at both the ingredient and recipe levels.
    - Budget tracking and comparison
        - The BudgetSummary entity allows the system to track estimated spending, remaining budget, and over-budget status.

2. Does it meet all non-functional requirements?
    * May need to look up specifications of systems, components, etc. to evaluate this.

    - Usability:
        - The budget feature is simple to understand and provides clear feedback to users.
    - Performance:
        - The use of estimated pricing avoids costly real-time pricing lookups.
    - Reliability:
        - The system does not rely on external grocery pricing services, reducing potential failures.
    - Extensibility:
        - The design allows future integration of pricing APIs or store-specific data without major restructuring.

# Time Management Requirements Workup

## Elicitation

1. Is the goal or outcome well defined?  Does it make sense?
    Yes, the goal is well defined. Time management is one of the main factors driving our application.

2. What is not clear from the given description?
    What specifically Time Management is refering to.

3. How about scope?  Is it clear what is included and what isn't?
    Im not sure exactly sure what is included in the scope of time management, this is likely going to be
    a function that makes it easy to select and plan meals. what is not clear is what time management is
    exactly. We know it will be assisting the user in creating an easier lifestyle as they will have to 
    plan and cook less with our application.

4. What do you not understand?

    * Technical domain knowledge
        - What specific functions will time management be?

    * Business domain knowledge
        - How timemanagemnet work or is measuered.

5. Is there something missing?
    Yes, it is unclear what specifically time management is, rather its a umbrella term for multiple 
    features or functions.

6. Get answers to these questions.
    Meet with team to get a better idea of what Time Management is.

## Analysis

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

## Design and Modeling
Our first goal is to create a **data model** that will support the initial requirements.

1. Identify all entities;  for each entity, label its attributes; include concrete types
    - User (UserID)
    - Calendar (CalendarID, StartDate, EndDate)
    - MealPlan (MealPlanID, Date, MealType, EstimatedPrepTime)
    - Recipe (RecipeID, Name, EstimatedCookTime)

2. Identify relationships between entities.  Write them out in English descriptions.
    - A User has one Calendar
    - A Calendar contains multiple MealPlans
    - A MealPlan is assigned to one Recipe
    - A User schedules meals by assigning recipes to calendar dates

3. Draw these entities and relationships in an _informal_ Entity-Relation Diagram.
    User
     ├── has → Calendar
    Calendar
     ├── contains → MealPlan
    MealPlan
     ├── references → Recipe

4. If you have questions about something, return to elicitation and analysis before returning here.
    Open questions remain regarding reminders and how time estimates should be presented to the user.

## Analysis of the Design
The next step is to determine how well this design meets the requirements _and_ fits into the existing system.

1. Does it support all requirements/features/behaviors?
    * For each requirement, go through the steps to fulfill it.  Can it be done?  Correctly?  Easily?
    - Meal scheduling and planning
        - The model allows users to assign recipes to specific dates and meal types through the Calendar and MealPlan entities.
    - Reduced daily decision-making
        - By planning meals in advance, users can view upcoming meals without needing to decide what to cook each day.
    - Estimated time awareness
        - Estimated cooking or preparation time can be displayed using the Recipe and MealPlan entities to help users plan their time more effectively.

2. Does it meet all non-functional requirements?
    * May need to look up specifications of systems, components, etc. to evaluate this.
        - Usability: 
            - Simple calendar-based scheduling is easy for users to understand
        - Performance: 
            - Minimal processing required for scheduling meals
        - Reliability: 
            - No reliance on external services
        - Extensibility: 
            - Additional features such as reminders can be added later

# Pantry Requirements Workup

## Elicitation

1. Is the goal or outcome well defined?  Does it make sense?
    Yes, the outcome of the pantry is well defined and it makes sense as it allows the user to track what
    they already have.

2. What is not clear from the given description?
    How we will make this efficent for the user, becuase usually people have a lot of miscellaneous things in their pantrys.

3. How about scope?  Is it clear what is included and what isn't?
    Not exactly, the pantry is a function that will help users track their pantry so that they can spend less on food.

4. What do you not understand?

    * Technical domain knowledge
        - how barcode scanning works

    * Business domain knowledge
        - How pantry inventory should be updated when items are partially used
        - How expiration dates should be handled, if at all

5. Is there something missing?
    Yes, it is unclear whether the pantry feature will track expiration dates, quantities remaining, or simply whether an item exists. It is also unclear if pantry items should automatically be deducted when recipes are selected.

6. Get answers to these questions.
    - Research basic barcode scanning capabilities and APIs
    - Discuss with the team whether pantry tracking will be manual or automated
    - Clarify how detailed pantry tracking should be (quantity-based vs item-based)

## Analysis

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

## Design and Modeling
Our first goal is to create a **data model** that will support the initial requirements.

1. Identify all entities;  for each entity, label its attributes; include concrete types
    - User (UserID)
    - Pantry (PantryID)
    - PantryItem (PantryItemID, Name, Quantity, Unit, ExpirationDate)
    - Ingredient (IngredientID, Name)

2. Identify relationships between entities.  Write them out in English descriptions.
    - A User has one Pantry
    - A Pantry contains many PantryItems
    - A PantryItem may correspond to an Ingredient
    - Ingredients from recipes can be compared against PantryItems

3. Draw these entities and relationships in an _informal_ Entity-Relation Diagram.
    User
     ├── has → Pantry
    Pantry
     ├── contains → PantryItem
    PantryItem
     ├── corresponds to → Ingredient

4. If you have questions about something, return to elicitation and analysis before returning here.
    Clarification is still needed on automation and expiration tracking.

## Analysis of the Design
The next step is to determine how well this design meets the requirements _and_ fits into the existing system.

1. Does it support all requirements/features/behaviors?
    * For each requirement, go through the steps to fulfill it.  Can it be done?  Correctly?  Easily?
    - Pantry inventory tracking
        - The model allows users to add, update, and remove pantry items through the Pantry and PantryItem entities.
    - Ingredient comparison
        - Pantry items can be compared against recipe ingredients to determine what the user already has.
    - Reduced food spending
        - By identifying existing pantry items, the system helps users avoid purchasing unnecessary ingredients.

2. Does it meet all non-functional requirements?
    * May need to look up specifications of systems, components, etc. to evaluate this.
    - Usability: 
        - Simple item-based tracking is easy for users to maintain
    - Performance: 
        - Pantry checks require minimal processing
    - Reliability: 
        - No external service dependencies
    - Extensibility: 
        - Barcode scanning and expiration tracking can be added later