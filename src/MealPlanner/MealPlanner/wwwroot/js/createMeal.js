const API_ROUTE = "/api/recipe/";

window.activeTagFilters = [];

$(document).ready(() => {
    $(document).on("click", ".recipeSearchRow", toggleRecipe);
    $(document).on("click", ".nm-recipe-remove-btn", removeSelectedRecipe);
    initTagFilterDropdown();
});

function updateSelectedCard() {
    const $card = $("#selectedRecipesCard");
    const hasItems = $("#selectedRecipesList .nm-recipe-row").length > 0;
    $card.toggleClass("open", hasItems);
}

async function toggleRecipe() {
    const $row = $(this);

    let recipeId = Number($(".recipeId", this).text());
    const recipeName = $(".recipeName", this).text();

    // Resolve external URI to a local recipe ID
    const externalUri = encodeURIComponent($row.attr("externaluri") || "");
    if (!recipeId && externalUri && externalUri !== "undefined") {
        const url = API_ROUTE + `external?recipeName=${encodeURIComponent(recipeName)}&externalUri=${externalUri}`;
        const response = await fetch(url);
        if (!response.ok) return;
        recipeId = await response.json();
        // Store resolved ID so repeated clicks don't re-fetch
        $(".recipeId", this).text(recipeId);
        $(".recipeIdInput", this).val(recipeId);
    }

    if (!recipeId) return;

    const alreadySelected = $row.hasClass("selected");

    if (alreadySelected) {
        // Deselect: remove hidden input and card row
        $row.removeClass("selected");
        $(`#createMealForm input[name="RecipeIds"][value="${recipeId}"]`).remove();
        $(`#selectedRecipesList .nm-recipe-row[data-id="${recipeId}"]`).remove();
    } else {
        // Select: add hidden input and card row
        $row.addClass("selected");
        const $hidden = $(`<input type="hidden" name="RecipeIds" value="${recipeId}" />`);
        $("#createMealForm").append($hidden);
        addToSelectedCard(recipeId, recipeName);
    }

    updateSelectedCard();
}

function addToSelectedCard(recipeId, recipeName) {
    const $row = $(`<div class="nm-recipe-row" data-id="${recipeId}">`)
        .append(`<span class="nm-recipe-name">${$("<span>").text(recipeName).html()}</span>`)
        .append(`<button type="button" class="nm-recipe-remove-btn" data-id="${recipeId}" title="Remove recipe"><i class="ti ti-x"></i></button>`);
    $("#selectedRecipesList").append($row);
}

function removeSelectedRecipe() {
    const recipeId = $(this).data("id");
    // Remove card row
    $(this).closest(".nm-recipe-row").remove();
    // Remove hidden form input
    $(`#createMealForm input[name="RecipeIds"][value="${recipeId}"]`).remove();
    // Uncheck in search results
    $(`.recipeSearchRow`).each(function () {
        if (Number($(".recipeId", this).text()) === recipeId) {
            $(this).removeClass("selected");
        }
    });
    updateSelectedCard();
}

// ── Tag filter dropdown ───────────────────────────────────────
// Builds a multi-select dropdown in #tagFilterDropdown.
// recipeSearch.js populates #tagFilter with <option>s via loadTags();
// we watch those mutations to add matching rows to the dropdown panel.
// Selected tags are stored in window.activeTagFilters and a change event
// on #tagFilter triggers recipeSearchHandler in recipeSearch.js.

function initTagFilterDropdown() {
    const container = document.getElementById("tagFilterDropdown");
    const tagFilter  = document.getElementById("tagFilter");
    if (!container || !tagFilter) return;

    container.innerHTML = `
        <button type="button" class="tfd-trigger" id="tfdTrigger">
            <span class="tfd-trigger-label" id="tfdLabel">All tags</span>
            <svg class="tfd-chevron" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"
                 fill="none" stroke="currentColor" stroke-width="2.5"
                 stroke-linecap="round" stroke-linejoin="round">
                <polyline points="6 9 12 15 18 9"/>
            </svg>
        </button>
    `;

    // Append panel to body so it escapes any overflow:hidden ancestor
    const panel = document.createElement("div");
    panel.className = "tfd-panel";
    panel.id = "tfdPanel";
    panel.innerHTML = `
        <div class="tfd-option tfd-all-option active" data-value="">
            <span class="tfd-check"></span>
            <span class="tfd-option-text">All tags</span>
        </div>
    `;
    document.body.appendChild(panel);

    const trigger    = document.getElementById("tfdTrigger");
    const label      = document.getElementById("tfdLabel");
    let selectedTags = [];

    function positionPanel() {
        const rect = trigger.getBoundingClientRect();
        panel.style.top   = `${rect.bottom + 4}px`;
        panel.style.left  = `${rect.left}px`;
        panel.style.width = `${Math.max(rect.width, 180)}px`;
    }

    function closePanel() {
        if (!panel.classList.contains("open")) return;
        panel.classList.remove("open");
        trigger.classList.remove("open");
        // Fire search only when the panel is committed/closed
        window.activeTagFilters = [...selectedTags];
        tagFilter.dispatchEvent(new Event("change"));
    }

    trigger.addEventListener("click", (e) => {
        e.stopPropagation();
        const isOpen = panel.classList.contains("open");
        if (isOpen) {
            closePanel();
        } else {
            positionPanel();
            panel.classList.add("open");
            trigger.classList.add("open");
        }
    });

    // Clicks inside the panel toggle selections but never close or bubble
    panel.addEventListener("click", (e) => {
        e.stopPropagation();
        const option = e.target.closest(".tfd-option");
        if (!option) return;
        const value = option.dataset.value;
        if (value === "") {
            selectedTags = [];
        } else {
            const idx = selectedTags.indexOf(value);
            if (idx === -1) selectedTags.push(value);
            else selectedTags.splice(idx, 1);
        }
        updateSelectionUI(selectedTags);
    });

    document.addEventListener("click", () => closePanel());

    window.addEventListener("scroll", closePanel, { passive: true, capture: true });
    window.addEventListener("resize", closePanel, { passive: true });

    // Watch #tagFilter for options added by loadTags() in recipeSearch.js
    const observer = new MutationObserver(() => {
        Array.from(tagFilter.options).forEach(opt => {
            if (!opt.value) return;
            if (panel.querySelector(`.tfd-option[data-value="${CSS.escape(opt.value)}"]`)) return;
            const optEl = document.createElement("div");
            optEl.className = "tfd-option";
            optEl.dataset.value = opt.value;
            optEl.innerHTML = `<span class="tfd-check"></span><span class="tfd-option-text">${opt.textContent.trim()}</span>`;
            panel.appendChild(optEl);
        });
    });
    observer.observe(tagFilter, { childList: true });

    function updateSelectionUI(selected) {
        panel.querySelector(".tfd-all-option").classList.toggle("active", selected.length === 0);
        panel.querySelectorAll(".tfd-option:not(.tfd-all-option)").forEach(opt => {
            opt.classList.toggle("active", selected.includes(opt.dataset.value));
        });
        label.textContent = selected.length === 0
            ? "All tags"
            : selected.length === 1 ? selected[0] : `${selected.length} tags`;
    }
}
