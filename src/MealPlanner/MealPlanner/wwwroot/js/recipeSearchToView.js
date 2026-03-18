const API_ROUTE = "/api/recipe/";

$(document).ready(() => {
    $(document).on("click", ".recipeSearchRow", goToView);
});

async function goToView(event) {
    if ($(event.target).closest(".favoriteForm").length > 0) return;

    let id = Number($(this).find(".recipeId").text().trim());
    
    const externalUri = encodeURIComponent($(this).attr("externaluri"));
    if (!id && externalUri) 
    {
        const recipeName = encodeURIComponent($(".recipeName", this).text());
        const externalUrl = API_ROUTE + `external?recipeName=${recipeName}&externalUri=${externalUri}`;
        
        const response = await fetch(externalUrl);
        if (!response.ok) return;
        id = await response.json();
    }

    location.href = `/FoodEntries/Recipes/${id}`;
}