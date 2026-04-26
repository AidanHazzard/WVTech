$(document).on("click", ".mealRecipeItem", function (e) {
    if ($(e.target).closest(".delete-recipe-btn").length) return;
    const recipeId = $(this).data("recipe-id");
    if (recipeId) window.location.href = `/FoodEntries/Recipes/${recipeId}`;
});

$(document).on("click", ".delete-recipe-btn", function (e) {
    e.stopPropagation();

    const $row = $(this).closest(".mealRecipeItem");
    const recipeId = $row.data("recipe-id");
    const mealId = $row.data("meal-id");

    showInlineConfirm(this, "Remove this recipe?", function () {
        fetch("/Meal/DeleteRecipeFromMeal", {
            method: "POST",
            keepalive: true,
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: `mealId=${mealId}&recipeId=${recipeId}`
        }).then(response => {
            if (response.ok) {
                $row.remove();
            } else {
                alert("Failed to remove recipe.");
            }
        });
    });
});
