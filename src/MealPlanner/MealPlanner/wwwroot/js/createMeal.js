let recipes = []

$(document).ready( () =>
    {
        $(document).on("click", "#recipeSearchRow", addRecipe);
    }
);

function addRecipe(event)
{
    // Get info from the search
    const recipeId = $("#recipeId", this).text();
    const recipeName = $("#recipeName", this).text();
    
    // Edit the form with hidden elements
    const recipeInputHtml = `<input id="recipe${recipes.length}" name="RecipeIds" type='hidden' value="${Number(recipeId)}">`;
    recipes.push(recipeId);
    $("#createMealForm").append(recipeInputHtml);

    // Edit the view
    const row = $("#addRecipeRow").contents().clone();
    $("#recipeName", row).text(recipeName);
    $("#createMealList").append(row);
}