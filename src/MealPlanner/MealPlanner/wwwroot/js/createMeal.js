let num_recipes = 0
const API_ROUTE = "/api/recipe/";

$(document).ready(() => {
    $(document).on("click", ".recipeSearchRow", addRecipe);
});

async function addRecipe() {
    // Get info from the search
    let recipeId = Number($(".recipeId", this).text());
    const recipeName = $(".recipeName", this).text();

    // Get recipe id from external source
    const externalUri = encodeURIComponent($(this).attr("externaluri"));
    if (!recipeId && externalUri)
    {
        const externalUrl = API_ROUTE + `external?recipeName=${encodeURIComponent(recipeName)}&externalUri=${externalUri}`;

        const response = await fetch(externalUrl);
        if (!response.ok) return;
        recipeId = await response.json();
    }

    // Check for duplicate
    const isDuplicate = Array.from($("#createMealForm input[name='RecipeIds']"))
        .some(input => Number(input.value) === Number(recipeId));

    if (isDuplicate) {
        alert("This recipe is already in the meal.");
        return;
}

    // Edit the form with hidden elements
    const recipeInputHtml = `<input id="recipe${num_recipes}" name="RecipeIds" type='hidden' value="${Number(recipeId)}">`;
    num_recipes++;
    $("#createMealForm").append(recipeInputHtml);

    // Edit the view
    const row = document.getElementById("addRecipeRow").content.cloneNode(true);
    $("#recipeName", row).text(recipeName);
    $(".recipeIdInput", row).val(recipeId);
    $(".mealRecipeItem", row).attr("onclick", `location.href='/FoodEntries/Recipes/${recipeId}'`);

    // Attach delete handler directly so it fires despite button's stopPropagation
    const $btn = $(".delete-recipe-btn", row);
    $btn.on("click", function(e) {
        e.stopPropagation();
        const $row = $(this).closest(".mealRecipeItem");
        const rId = $row.find(".recipeIdInput").val();
        showInlineConfirm(this, "Remove this recipe?", function () {
            $row.remove();
            $(`#createMealForm input[value="${rId}"]`).remove();
        });
    });

    $("#createMealList").append(row);
}
