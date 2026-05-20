$(document).on("click", ".recipe-row", function (e) {
    if ($(e.target).closest('.recipe-row-actions').length) return;
    const recipeId = $(this).data("recipe-id");
    if (recipeId) window.location.href = `/FoodEntries/Recipes/${recipeId}`;
});

// Fallback for any legacy .mealRecipeItem elements on other pages
$(document).on("click", ".mealRecipeItem:not(.recipe-row)", function () {
    const recipeId = $(this).data("recipe-id");
    if (recipeId) window.location.href = `/FoodEntries/Recipes/${recipeId}`;
});

$(document).on("click", ".delete-recipe-btn", function (e) {
    e.stopPropagation();

    const recipeId = $(this).data("id");

    showDeleteModal("Delete this recipe?", function () {
        fetch("/FoodEntries/DeleteRecipe", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: `id=${recipeId}`
        }).then(response => {
            if (response.ok) {
                $(`[data-recipe-id="${recipeId}"]`).remove();
            } else {
                alert("Failed to delete recipe.");
            }
        });
    });
});
