let num_recipes = 0

$(document).ready(() => {
    $(document).on("click", ".recipeSearchRow", addRecipe);
}
);

function addRecipe(event) {
    // Get info from the search
    const recipeId = $(".recipeId", this).text();
    const recipeName = $(".recipeName", this).text();
    
    // Edit the form with hidden elements
    const recipeInputHtml = `<input id="recipe${num_recipes}" name="RecipeIds" type='hidden' value="${Number(recipeId)}">`;
    num_recipes++;
    $("#createMealForm").append(recipeInputHtml);

    // Edit the view
    const row = document.getElementById("addRecipeRow").content.cloneNode(true);
    $("#recipeName", row).text(recipeName);
    $(".recipeIdInput", row).val(Number(recipeId));
    $("#createMealList").append(row);
}