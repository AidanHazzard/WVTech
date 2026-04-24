let mealSections = [];
let currentMeal = 0;

function showMeal(index) {
    mealSections.forEach((s, i) => s.style.display = i === index ? '' : 'none');

    const prev = document.getElementById('btnPrevMeal');
    const next = document.getElementById('btnNextMeal');
    const generate = document.getElementById('btnGeneratePlan');

    prev.style.display = index > 0 ? '' : 'none';
    next.style.display = index < mealSections.length - 1 ? '' : 'none';
    generate.style.display = index === mealSections.length - 1 ? '' : 'none';

    // Rounding: :first-child/:last-child counts hidden elements, so set explicitly.
    const visible = [prev, next, generate].filter(b => b.style.display !== 'none');
    visible.forEach((btn, i) => {
        if (visible.length === 1) btn.style.borderRadius = '20px';
        else if (i === 0) btn.style.borderRadius = '20px 0 0 20px';
        else if (i === visible.length - 1) btn.style.borderRadius = '0 20px 20px 0';
        else btn.style.borderRadius = '0';
    });

    document.getElementById('mealStepLabel').textContent = `Meal ${index + 1} of ${mealSections.length}`;
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
            <div class="d-flex flex-column gap-3 mb-3">
                <div>
                    <label class="form-label">Meal title (optional)</label>
                    <input type="text"
                           name="MealPreferences[${i}].Title"
                           class="back2-textbox"
                           placeholder="e.g. Lunch, Weekend Brunch…" />
                </div>
                <div>
                    <label class="form-label">Size</label>
                    <select name="MealPreferences[${i}].Size" class="back2-textbox">
                        <option value="Small">Small</option>
                        <option value="Average" selected>Average</option>
                        <option value="Large">Large</option>
                    </select>
                </div>
                <div>
                    <label class="form-label">Tags</label>
                    <div class="tag-area mt-1"></div>
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
