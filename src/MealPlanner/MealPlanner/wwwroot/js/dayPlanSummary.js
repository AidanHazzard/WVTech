function getCard(mealId) {
    return document.getElementById(`meal-card-${mealId}`);
}

function getForm(mealId) {
    return document.getElementById(`regenerate-form-${mealId}`);
}

function getRegenBtn(mealId) {
    return getCard(mealId)?.querySelector('[data-action="regenerate-meal"]');
}

function openForm(mealId) {
    getCard(mealId).querySelector('[data-recipe-list]').style.display = 'none';
    getForm(mealId).style.display = '';
    getRegenBtn(mealId).style.display = 'none';
}

function closeForm(mealId) {
    getForm(mealId).style.display = 'none';
    getRegenBtn(mealId).style.display = '';
    getCard(mealId).querySelector('[data-recipe-list]').style.display = '';
}

function initTagManager(formWrapper, availableTags) {
    const container = formWrapper.querySelector('.tags-container');
    const select = formWrapper.querySelector('.tag-select');
    const customInput = formWrapper.querySelector('.custom-tag-input');
    const addBtn = formWrapper.querySelector('.add-tag-btn');

    availableTags.forEach(t => {
        const opt = document.createElement('option');
        opt.value = t.id;
        opt.textContent = t.name;
        select.appendChild(opt);
    });

    function addPredefinedTag(tagId, tagName) {
        if (formWrapper.querySelector(`input[name="TagIds"][value="${tagId}"]`)) {
            select.value = '';
            return;
        }
        const opt = Array.from(select.options).find(o => o.value == tagId);
        if (opt) opt.remove();

        const pill = document.createElement('button');
        pill.type = 'button';
        pill.className = 'tag-pill badge rounded-pill recipe-tag recipe-tag-removable';
        pill.title = `Remove ${tagName}`;
        pill.innerHTML = `${tagName}<input type="hidden" name="TagIds" value="${tagId}" />`;
        pill.addEventListener('click', () => {
            const opt = document.createElement('option');
            opt.value = tagId;
            opt.textContent = tagName;
            const opts = Array.from(select.options).slice(1);
            const before = opts.find(o => o.textContent.toLowerCase() > tagName.toLowerCase());
            if (before) select.insertBefore(opt, before);
            else select.appendChild(opt);
            pill.remove();
        });
        container.appendChild(pill);
        select.value = '';
    }

    function addCustomTag(tagName) {
        if (!tagName.trim()) return;
        const existing = container.querySelector('.custom-tag-pill');
        if (existing) existing.remove();

        const pill = document.createElement('button');
        pill.type = 'button';
        pill.className = 'tag-pill badge rounded-pill recipe-tag recipe-tag-removable custom-tag-pill';
        pill.title = `Remove ${tagName}`;
        pill.innerHTML = `${tagName}<input type="hidden" name="CustomTagName" value="${tagName}" />`;
        pill.addEventListener('click', () => pill.remove());
        container.appendChild(pill);
    }

    select.addEventListener('change', () => {
        if (!select.value) return;
        addPredefinedTag(select.value, select.options[select.selectedIndex].textContent.trim());
    });

    addBtn.addEventListener('click', () => {
        addCustomTag(customInput.value.trim());
        customInput.value = '';
    });

    customInput.addEventListener('keydown', e => {
        if (e.key === 'Enter') {
            e.preventDefault();
            addCustomTag(customInput.value.trim());
            customInput.value = '';
        }
    });
}

// Wire up buttons and init tag managers
const availableTags = JSON.parse(document.getElementById('day-plan-summary').dataset.tags || '[]');

document.querySelectorAll('[data-action="regenerate-meal"]').forEach(btn => {
    btn.addEventListener('click', () => openForm(btn.dataset.mealId));
});

document.querySelectorAll('[data-action="cancel-regenerate"]').forEach(btn => {
    btn.addEventListener('click', () => closeForm(btn.dataset.mealId));
});

document.querySelectorAll('[id^="regenerate-form-"]').forEach(formWrapper => {
    initTagManager(formWrapper, availableTags);
});

document.querySelectorAll('[data-action="submit-regenerate"]').forEach(btn => {
    btn.addEventListener('click', async function () {
        const mealId = this.dataset.mealId;
        const form = getForm(mealId);
        const formData = new FormData(form.querySelector('form'));

        this.disabled = true;
        this.querySelector('.buttonText').textContent = 'Regenerating…';

        try {
            const response = await fetch(`/Meal/RegenerateMeal?mealId=${mealId}`, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) throw new Error(`Server error ${response.status}`);
            const data = await response.json();

            const card = getCard(mealId);
            card.querySelector('[data-meal-name]').textContent = data.title;

            const recipeList = card.querySelector('[data-recipe-list]');
            if (recipeList) {
                recipeList.innerHTML = data.recipes.length === 0
                    ? '<p class="text-muted mb-0">No recipes in this meal.</p>'
                    : data.recipes.map(r => `
                        <div class="mealRecipeItem d-flex align-items-center justify-content-between back2 back2-textbox mb-1"
                             style="cursor:pointer; margin-top:8px;"
                             onclick="location.href='/FoodEntries/Recipes/${r.id}'">
                            <h4 class="mb-0 flex-grow-1">${r.name}</h4>
                            <span class="ms-2 text-nowrap">${r.calories} cal</span>
                        </div>`).join('');
            }

            closeForm(mealId);
        } catch (e) {
            alert('Failed to regenerate meal. Please try again.');
        } finally {
            this.disabled = false;
            this.querySelector('.buttonText').textContent = 'Regenerate';
        }
    });
});
