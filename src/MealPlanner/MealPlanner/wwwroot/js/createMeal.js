let num_recipes = 0
const API_ROUTE = "/api/recipe/";

$(document).ready(() => {
    $(document).on("click", ".recipeSearchRow", addRecipe);

    $(document).on("click", ".delete-recipe-btn", function () {
        if (!confirm("Are you sure you want to remove this recipe?")) return;
        const $row = $(this).closest(".mealRecipeItem");
        const recipeId = $row.find(".recipeIdInput").val();
        $row.remove();
        $(`#createMealForm input[value="${recipeId}"]`).remove();
    });
});

async function addRecipe(event) {
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
    console.log(document.getElementById("addRecipeRow").content.cloneNode(true));
    $("#createMealList").append(row);
}