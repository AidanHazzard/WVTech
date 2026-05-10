let _undoState = null; // { mealId, removedRecipeId, addBackRecipeId, addBackName, addBackCalories }

$(document).on("click", ".mealRecipeItem", function (e) {
    if ($(e.target).closest(".delete-recipe-btn, [data-action='regenerate-recipe']").length) return;
    const recipeId = $(this).data("recipe-id");
    if (recipeId) window.location.href = `/FoodEntries/Recipes/${recipeId}`;
});

$(document).on("click", ".delete-recipe-btn", function (e) {
    e.stopPropagation();

    const $row = $(this).closest(".mealRecipeItem");
    const recipeId = $row.data("recipe-id");
    const mealId = $row.data("meal-id");

    showInlineConfirm(this, "Remove this recipe?", function () {
        fetch("/Meal/DeleteRecipeFromMeal", {
            method: "POST",
            keepalive: true,
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
});

$(document).on("click", "[data-action='regenerate-recipe']", async function (e) {
    e.stopPropagation();

    const btn = this;
    const recipeId = btn.dataset.recipeId;
    const mealId = btn.dataset.mealId;
    const $row = $(btn).closest(".mealRecipeItem");
    const oldName = $row.find("h4").first().text().trim();
    const oldCalories = parseInt($row.find(".cal-display").text()) || 0;

    hideRegenFeedback();
    btn.disabled = true;

    try {
        const response = await fetch(`/Meal/RegenerateRecipe?mealId=${mealId}&recipeId=${recipeId}`, {
            method: "POST"
        });

        if (!response.ok) throw new Error(`Server error ${response.status}`);
        const data = await response.json();

        if (data.noAlternative) {
            document.getElementById("regen-no-alt-msg").style.display = "";
            return;
        }

        // Update the row in place (server returns camelCase JSON)
        const newRecipe = data.newRecipe;
        $row.attr("data-recipe-id", newRecipe.id);
        $row.find("h4").first().text(newRecipe.name);
        $row.find("[data-action='regenerate-recipe']").attr("data-recipe-id", newRecipe.id);
        $row.attr("onclick", `location.href='/FoodEntries/Recipes/${newRecipe.id}'`);

        // Store undo state and show toast
        _undoState = {
            mealId: mealId,
            removedRecipeId: newRecipe.id,
            addBackRecipeId: parseInt(recipeId),
            addBackName: oldName,
            addBackCalories: oldCalories
        };
        document.getElementById("regen-undo-toast").style.display = "";
    } catch (err) {
        alert("Failed to regenerate recipe. Please try again.");
    } finally {
        btn.disabled = false;
    }
});

$(document).on("click", "[data-action='undo-regenerate-recipe']", async function () {
    if (!_undoState) return;

    const { mealId, removedRecipeId, addBackRecipeId, addBackName } = _undoState;

    try {
        const response = await fetch(
            `/Meal/SwapRecipe?mealId=${mealId}&removeRecipeId=${removedRecipeId}&addRecipeId=${addBackRecipeId}`,
            { method: "POST" }
        );

        if (!response.ok) throw new Error(`Server error ${response.status}`);

        // Restore the row
        const $row = $(`.mealRecipeItem[data-recipe-id='${removedRecipeId}']`);
        $row.attr("data-recipe-id", addBackRecipeId);
        $row.find("h4").first().text(addBackName);
        $row.find("[data-action='regenerate-recipe']").attr("data-recipe-id", addBackRecipeId);
        $row.attr("onclick", `location.href='/FoodEntries/Recipes/${addBackRecipeId}'`);

        _undoState = null;
        hideRegenFeedback();
    } catch (err) {
        alert("Failed to undo. Please try again.");
    }
});

function hideRegenFeedback() {
    document.getElementById("regen-no-alt-msg").style.display = "none";
    document.getElementById("regen-undo-toast").style.display = "none";
}
