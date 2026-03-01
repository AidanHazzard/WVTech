$(document).ready(() =>
{
    $(document).on("click", "#recipeSearchRow", goToView);
});
    
function goToView(event)
{
    if ($(event.target).closest(".favoriteForm").length > 0) return;
    
    const id = $("#recipeId", this).text();
    location.href = `/FoodEntries/Recipes/${id}`;
}