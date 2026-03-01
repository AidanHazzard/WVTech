$(document).ready(
    () => $("#searchText").on("input", throttle(recipeSearchHandler, 1000))
);

function throttle(func, delay)
{
    let last = 0;
    return function (...args)
    {
        let now = Date.now();
        if (now - last >= delay)
        {
            func.apply(this, args);
            last = now;
        }
    }
}

async function recipeSearchHandler(event)
{
    $("#error").hide();
    const search = $("#searchText").val();

    if (search.length < 3)
    {
        return;
    }

    const response = await fetch(`/api/recipe/search?name=${search}`);

    $("#recipeResults").text("");
    if (!response.ok)
    {
        $("#error").show();
        $("#error").text("No recipes found, sorry!");
        return;
    }

    const recipes = await response.json();
    const rowTemplate = $("#recipeResult");

    for (const i in recipes)
    {
        const recipe = recipes[i];
        const row = rowTemplate.contents().clone(true);
    
        $(".recipeName", row).text(recipe.name);
        $(".recipeId", row).text(recipe.id);
        $(".recipeIdInput", row).val(recipe.id);
    
        $("#recipeResults").append(row);
    }
}