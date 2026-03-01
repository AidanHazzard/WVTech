$(document).ready(() => {
    $(document).on("click", ".recipeSearchRow", goToView);
});

function goToView(event) {
    const id = $(this).find(".recipeId").text().trim();
    location.href = `/FoodEntries/Recipes/${id}`;
}