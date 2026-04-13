$(document).on("click", ".mealRecipeItem", function (e) {
    const recipeId = $(this).data("recipe-id");
    window.location.href = `/FoodEntries/Recipes/${recipeId}`;
});

$(document).on("click", ".delete-recipe-btn", function (e) {
    e.stopPropagation();

    if (!confirm("Are you sure you want to delete this recipe?")) return;

    const recipeId = $(this).data("id");
    window._deleteStatus = null;

    fetch("/FoodEntries/DeleteRecipe", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: `id=${recipeId}`
    }).then(response => {
        window._deleteStatus = response.status;
        if (response.ok) {
            $(`[data-recipe-id="${recipeId}"]`).remove();
        } else {
            alert("Failed to delete recipe.");
        }
    });
});