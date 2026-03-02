$(document).ready(() =>
    {
        $(document).on("click", "#recipeSearchRow", goToView);
    }
);

function goToView(event)
{
    const id = $("#recipeId", this).text();
    location.href = `/FoodEntries/Recipes/${id}`;
}