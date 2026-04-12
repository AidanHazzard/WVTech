$(document).on("click", ".delete-recipe-btn", function () {
    if (!confirm("Are you sure you want to remove this recipe?")) return;
    
    const $row = $(this).closest(".mealRecipeItem");
    const recipeId = $row.data("recipe-id");
    const mealId = $row.data("meal-id");

    fetch("/Meal/DeleteRecipeFromMeal", {
        method: "POST",
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