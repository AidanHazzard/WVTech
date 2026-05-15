let mealSections = [];
let currentMeal = 0;

function showMeal(index) {
    mealSections.forEach((s, i) => s.style.display = i === index ? '' : 'none');

    const prev     = document.getElementById('btnPrevMeal');
    const next     = document.getElementById('btnNextMeal');
    const generate = document.getElementById('btnGeneratePlan');

    prev.style.display     = index > 0 ? '' : 'none';
    next.style.display     = index < mealSections.length - 1 ? '' : 'none';
    generate.style.display = index === mealSections.length - 1 ? '' : 'none';

    document.getElementById('mealStepLabel').textContent = `Meal ${index + 1} of ${mealSections.length}`;

    const bar = document.getElementById('gdpProgressBar');
    if (bar) {
        bar.innerHTML = '';
        for (let i = 0; i < mealSections.length; i++) {
            const seg = document.createElement('div');
            seg.className = 'gdp-progress-seg' + (i <= index ? ' filled' : '');
            bar.appendChild(seg);
        }
    }
}

function initTagManager(section, mealIndex, availableTags) {
    const container = section.querySelector('.tags-container');
    const select = section.querySelector('.tag-select');
    const customInput = section.querySelector('.custom-tag-input');
    const addBtn = section.querySelector('.add-tag-btn');

    availableTags.forEach(t => {
        const opt = document.createElement('option');
        opt.value = t.id;
        opt.textContent = t.name;
        select.appendChild(opt);
    });

    function addPredefinedTag(tagId, tagName) {
        if (section.querySelector(`input[name="MealPreferences[${mealIndex}].TagIds"][value="${tagId}"]`)) {
            select.value = '';
            return;
        }
        const opt = Array.from(select.options).find(o => o.value == tagId);
        if (opt) opt.remove();

        const pill = document.createElement('button');
        pill.type = 'button';
        pill.className = 'tag-pill badge rounded-pill recipe-tag recipe-tag-removable';
        pill.title = `Remove ${tagName}`;
        pill.dataset.predefined = 'true';
        pill.innerHTML = `${tagName}<input type="hidden" name="MealPreferences[${mealIndex}].TagIds" value="${tagId}" />`;
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
        pill.innerHTML = `${tagName}<input type="hidden" name="MealPreferences[${mealIndex}].CustomTagName" value="${tagName}" />`;
        pill.addEventListener('click', () => pill.remove());
        container.appendChild(pill);
    }

    select.addEventListener('change', () => {
        if (!select.value) return;
        addPredefinedTag(select.value, select.options[select.selectedIndex].textContent);
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

document.querySelector('[data-action="next-step"]').addEventListener('click', function () {
    const availableTags = JSON.parse(document.getElementById('dayPlanForm').dataset.tags);
    const count = parseInt(document.getElementById('MealCount').value) || 1;
    const container = document.getElementById('mealPreferenceSections');
    container.innerHTML = '';

    document.getElementById('hiddenMonth').value = document.getElementById('dayPlanMonth').value;
    document.getElementById('hiddenDay').value = document.getElementById('dayPlanDay').value;

    for (let i = 0; i < count; i++) {
        const section = document.createElement('div');
        section.style.display = 'none';
        section.innerHTML = `
            <div class="gdp-fields">
                <div class="gdp-field">
                    <label class="gdp-label">Meal title (optional)</label>
                    <input type="text"
                           name="MealPreferences[${i}].Title"
                           class="gdp-input"
                           placeholder="e.g. Lunch, Weekend Brunch…" />
                </div>
                <div class="gdp-field">
                    <label class="gdp-label">Size</label>
                    <select name="MealPreferences[${i}].Size" class="gdp-input gdp-select">
                        <option value="Small">Small</option>
                        <option value="Average" selected>Average</option>
                        <option value="Large">Large</option>
                    </select>
                </div>
                <div class="gdp-field">
                    <label class="gdp-label">Tags</label>
                    <div class="tag-area"></div>
                </div>
            </div>`;

        const tagClone = document.getElementById('mealTagTemplate').content.cloneNode(true);
        section.querySelector('.tag-area').appendChild(tagClone);
        container.appendChild(section);
        initTagManager(section, i, availableTags);
    }

    mealSections = Array.from(container.children);
    currentMeal = 0;
    showMeal(0);

    document.getElementById('step1').style.display = 'none';
    document.getElementById('step2').style.display = '';
});

document.getElementById('btnPrevMeal')?.addEventListener('click', function () {
    if (currentMeal > 0) showMeal(--currentMeal);
});

document.getElementById('btnNextMeal')?.addEventListener('click', function () {
    if (currentMeal < mealSections.length - 1) showMeal(++currentMeal);
});
