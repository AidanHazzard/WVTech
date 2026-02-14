$(document).ready(
    () => $("#searchText").on("input", throttle(recipeSearchHandler, 1000))
);

// Throttle function from https://www.geeksforgeeks.org/javascript/javascript-throttling/
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

    const respose = await fetch(`/api/recipe/search?name=${search}`);
    
    $("#recipeResults").text("")
    if (!respose.ok)
    {
        $("#error").show();
        $("#error").text("No recipes found, sorry!");
        return;
    }

    const recipes = await respose.json();
    const rowTemplate = $("#recipeResult");

    for (const i in recipes)
    {
        const recipe = recipes[i];
        const row = rowTemplate.contents().clone();

        $("#recipeName", row).text(recipe.name);
        $("#recipeResults").append(row);
    }
}