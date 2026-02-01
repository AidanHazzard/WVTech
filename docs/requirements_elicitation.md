# Requirements Elicitation

#### User Account

##### Elicitation

1. **Is the goal or outcome well defined? Does it make sense?**  
Yes, the goal is well defined and makes sense. The purpose of the User Account feature is to allow the user to create and sign into an account using an email or phone number. Once the user has created an account, the app is able to save any non-personal data such as favorite meals, dietary restrictions, etc.

2. **What is not clear from the given description?**  
It is not defined whether the app will support consistent login behavior, such as remembering the user’s sign-in information. Additionally, the feature doesn’t define whether any information besides login credentials will be stored.

3. **How about scope? Is it clear what is included and what isn't?**  
The scope for this feature is mostly clear, including creating an account, signing in, and saving specific data. However, account recovery, account verification, and consistent login behavior are not clearly defined.

4. **What do you not understand?**

- *Technical domain knowledge*  
  Decisions about email/phone verification, password creation rules, and how user data will be stored.

- *Business domain knowledge*  
  Required vs. optional information during account creation and whether users can save login information for future use.

5. **Is there something missing?**  
The option of “remember me” when creating an account. Without this feature, users who are unexpectedly logged out may become frustrated if they must repeatedly re-enter their information.

6. **Get answers to these questions.**  
Clarify authentication requirements and security expectations for the app.


---


#### Diet

##### Elicitation

1. **Is the goal or outcome well defined? Does it make sense?**  
Yes, the goal is well defined. The Diet feature allows the app to track a user’s nutrient intake based on meals and entered information over time. It also enables users to define dietary restrictions, allergies, and nutritional goals.

2. **What is not clear from the given description?**  
It is unclear how precise nutrient tracking needs to be. The meaning of “tolerance” for nutritional requirements is also unclear, along with how enforcement works and whether it only affects meal recommendations.

3. **How about scope? Is it clear what is included and what isn't?**  
The scope mostly includes nutrient tracking, dietary restrictions, allergies, and goals. However, it is unclear whether guidance will be medical-grade, include real-time adjustments, or remain informational. The distinction between simple tracking and live dietary enforcement needs clarification.

4. **What do you not understand?**

- *Technical domain knowledge*  
  How nutrient data is calculated and how tolerance is applied when evaluating meals against goals.

- *Business domain knowledge*  
  Which diet features are essential versus optional, whether modified diets are for medical, lifestyle, or accessibility purposes, and expectations for accuracy and disclaimers.

5. **Is there something missing?**  
There is no defined location for editing, resetting, or temporarily disabling dietary restrictions. It is also unclear whether the app will provide warnings or alerts when nutritional goals are consistently unmet.

6. **Get answers to these questions.**

- Define the level of nutritional detail required from users  
- Clarify how strictly dietary restrictions are enforced  
- Determine the balance between user responsibility and system guidance


---


#### Shopping List

##### Elicitation

1. **Is the goal or outcome well defined? Does it make sense?**  
Yes, the goal makes sense. This feature allows users to manually enter grocery items with quantities and notes, automatically add ingredients from upcoming meals, and export lists to third-party apps for ordering or pickup.

2. **What is not clear from the given description?**

- How pantry inventory is tracked before items are removed  
- How exporting to third-party services works (direct push, share link, etc.)  
- Whether limits exist for manual edits versus automatic changes

3. **How about scope? Is it clear what is included and what isn't?**  
Manual item entry, automatic calendar-based additions, and exporting are included. However, reminders, item sorting, and handling conflicts between manual and automatic changes are unclear.

4. **What do you not understand?**

- *Technical domain knowledge*  
  How pantry data is stored and synced  
  How automatic removal works  
  How lists are exported  
  How manual and automatic changes are resolved

- *Business domain knowledge*  
  Which grocery services are supported (ordering, pickup, delivery)  
  User expectations for pantry management beyond add/remove  
  Whether budget and nutrition are included  
  How often automatic updates occur and whether users can override them

5. **Is there something missing?**

- Clear workflow for maintaining pantry inventory to prevent incorrect removals  
- Rules to prevent conflicts between user edits and automated changes  
- Confirmation step before exporting shopping lists

6. **Get answers to these questions.**

- Define the pantry model and synchronization logic  
- Determine export method and supported platforms  
- Establish rules for manual versus automatic changes


--- 


#### Budget

##### Elicitation

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


---


#### Time Management

##### Elicitation

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


---


#### Pantry

##### Elicitation

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


---


#### Calendar
A user must be able to schedule the times and dates of meals on a calendar and view the meals they have scheduled previously

##### Elicitation

1. Is the goal or outcome well defined?  Does it make sense?

The goal of the Calendar feature makes sense and is well defined, its to provide a place for the user to create their meal plan.

2. What is not clear from the given description?

It is not clear what the time range is on the calendar, whether it should be seperated into days/weeks/months.

3. How about scope?  Is it clear what is included and what isn't?

The scope is clear. The calendar is limited to setting the time and dates of meals.

4. What do you not understand?
    * Technical domain knowledge
    * Business domain knowledge
5. Is there something missing?
- What time range(s) should the calendar display? 
    * The calendar should display both individual days and whole weeks
- What information should about meals should the calendar display?
    * The meals should have a name, nutritional info, date and time
6. Get answers to these questions.


---


#### Recommendation System
When a user adds a meal to the calendar, the system must recommend meals to user based on their budget, time budget, diet, pantry, and preferences inferred from favorited recipes and recipe ratings they have given.

##### Elicitation

1. Is the goal or outcome well defined?  Does it make sense?

The goal makes sense but is not well defined. The goal here is to make a recommendation algorithm for recipes based on the given categories.

2. What is not clear from the given description?

It is unclear how many meals the system should recommend; at what point in the process does the system recommend meals; what the GUI to interact with the recommend meals looks like; and what the user can do with the recommended meals. It is also unclear how the preferences are inferred from favorite recipes and ratings.

3. How about scope?  Is it clear what is included and what isn't?

The scope is unclear. It is clear what information is used in the recommendation of meals, and that this requirement only is to recommend meals. However it is unclear in that the definition of "meals" is undefined and how the system infers recipe preferences

4. What do you not understand?
    - Technical domain knowledge
		* What algorithm to use to recommend meals?
			* A DNN of some sort might work. Other recommendation algorithms as well such as collaborative filtering. No matter what, we're diving into machine learning for this
			* Combining budget and diet together looks a lot like a multi-dimensional knapsack problem
			* One way to do this is to first filter recipes by what the user can have, have a recommendation algorithm sort recipes and another algorithm to solve our budget/diet knapsack problem to create a meal. Further research is required but this knapsack problem seems like it should be NP-hard. A greedy algorithm that can provide multiple different solutions for the user to pick from if that is so.
		* How to get inferred preferences from favorited recipes and recipe ratings?
			* See 5
    - Business domain knowledge	
		* Meals is plural, how many meals should be recommended to the user?
			* 3 is a good number. Could be user provided.
		* How should the system take into account diet when recommending meals?
		* How should the system balance all the factors given?
			* User provided priorities!
		* How should a user's time budget be determined? Sometimes people have more time to make food than other times.
		* Sometimes a person is allergic to an ingredient only if its raw, how do we handle this in terms of filtering recipes?
5. Is there something missing?
	* How does the system infer recipe preferences?
		* Sounds like a job for AI. This is what DNNs are good at. I would expect a correlation between favorited recipes/recipe ratings and food preferences. However, a user may give a recipe a good rating because its cheap or easy to make, not necessarily because they find it tastes better than others, which could complicate the goal of this analytical model limited to the given information.
	* What type of data is used to determine "food preferences"
		* A tag system for recipes, describing what type of food the recipe makes.
6. Get answers to these questions.


---


#### Recipe Book
A user must be able to search and view recipes provided by the app.

##### Elicitation

1. Is the goal or outcome well defined?  Does it make sense?

The goal is well-defined and makes sense.

2. What is not clear from the given description?

It is not clear how a search of recipes is done, and where the recipes originate from, e.g. external API, self-hosted db, saved on the user's device.

3. How about scope?  Is it clear what is included and what isn't?

The scope is clear. Only searching and viewing of recipes is included

4. What do you not understand?
	- Technical domain knowledge
		- What categories is searching done by?
	    		- Tags to determine what type of food the recipe makes
			- Ingredients
			- Nutritional info
		- Metric or Imperial?
			- Por que no los dos? Store information as metric and convert to imperial to display, shop, and search if the user wishes.
  	- Business domain knowledge
  		- What nutrional info to include?
  			- Macros: protein, carbs, fats
  			- Caloric content
  			- Vitamins
5. Is there something missing?
	* Where do the recipes come from?
		- Recipe API, user provided, we (the developers) can also provide our own
	* What is a recipe comprised of?
		- A name
		- Tags for searching, e.g. Breakfast, Italian, Easy
		- A list of ingredients and amounts
		- Nutritional info
		- Instructions
		- A description of the recipe would be useful for users
6. Get answers to these questions.

