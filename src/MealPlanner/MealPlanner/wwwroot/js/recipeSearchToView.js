$(document).ready(() => {
    $(document).on("click", ".recipeSearchRow", goToView);
});

function goToView(event) {
    if ($(event.target).closest(".favoriteForm").length > 0) return;

    const id = $(this).find(".recipeId").text().trim();
    if (!id) return;

    location.href = `/FoodEntries/Recipes/${id}`;
}