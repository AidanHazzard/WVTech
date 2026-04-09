const LOW_RATING_COLOR = "red";
const HIGH_RATING_COLOR = "green";
const API_ROUTE = "/api/recipe/";

$(document).ready(() => {$("#searchText").on("input", throttle(recipeSearchHandler, 1000));

    // Click on searched recipe
    $(document).on("click", ".recipeSearchRow", async function () {
        const $row = $(this);
        const recipeName = $row.find(".recipeName").text();
        const externalUri = $row.attr("externalUri");

        let recipeId = Number($row.find(".recipeId").text());

        recipeId = await resolveRecipeId(recipeId, recipeName, externalUri);
        if (!recipeId) return;

        const $mealForm = $("#editMealForm");
        if (!$mealForm.length) {
            console.warn("Meal form not found");
            return;
        }

        addRecipeToMealForm($mealForm, recipeId, recipeName);
    });
});

async function resolveRecipeId(recipeId, recipeName, externalUri) {
    if (recipeId || !externalUri) return recipeId;

    const response = await fetch(
        `${API_ROUTE}external?recipeName=${encodeURIComponent(recipeName)}&externalUri=${encodeURIComponent(externalUri)}`
    );

    if (!response.ok) {
        console.warn("Failed to fetch external recipe ID");
        return null;
    }

    const fetchedId = await response.json();
    return fetchedId;
}

function addRecipeToMealForm($mealForm, recipeId, recipeName) {

    // Hidden input (used when saving)
    const hiddenInput = $(`<input type="hidden" name="RecipeIds" value="${recipeId}" />`);
    $mealForm.append(hiddenInput);

    // Visible recipe row
    const $rowWrapper = $(`
        <div class="mealRecipeItem mb-2">
            <button type="button" class="buttonGrey">
                <h3 class="buttonText">${recipeName}</h3>
            </button>
        </div>
    `);

    $rowWrapper.find("button").on("click", () => {
        window.location.href = `/FoodEntries/Recipes/${recipeId}`;
    });

    $("#mealRecipeList").append($rowWrapper);
}

// Throttle function from https://www.geeksforgeeks.org/javascript/javascript-throttling/
function throttle(func, delay)
{
    let last = 0;
    return function (...args)
    {
        const search = $("#searchText").val();

        if (search.length < 3) return;
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
        const rating = (recipe.votePercentage * 100).toFixed(0) + "%";
        $(row).attr("externalUri", recipe.externalUri);
        if (recipe.externalUri) $(".row-end", row).text("");
        $(".recipeName", row).text(recipe.name);
        $(".recipeId", row).text(recipe.id);
        $(".recipeIdInput", row).val(recipe.id);
        $(".recipeRating", row).text(rating);
        $(".recipeRating", row).attr("style", `color: color-mix(in oklch, ${LOW_RATING_COLOR}, ${HIGH_RATING_COLOR} ${rating});`)
        $("#recipeResults").append(row);
    }
}