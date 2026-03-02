$(document).ready(() => {
    $(document).on("click", ".recipeSearchRow", goToView);
});

function goToView(event) {
    // If the click was inside a favorite button/form, don't navigate
    if ($(event.target).closest(".favoriteForm").length > 0) return;

    const id = $(this).find(".recipeId").text().trim();
    if (!id) return;

    location.href = `/FoodEntries/Recipes/${id}`;
}