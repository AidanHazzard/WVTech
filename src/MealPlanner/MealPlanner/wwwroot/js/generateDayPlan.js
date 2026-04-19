let mealSections = [];
let currentMeal = 0;

function showMeal(index) {
    mealSections.forEach((s, i) => s.style.display = i === index ? '' : 'none');
    document.getElementById('btnPrevMeal').style.display = index > 0 ? '' : 'none';
    document.getElementById('btnNextMeal').style.display = index < mealSections.length - 1 ? '' : 'none';
    document.getElementById('btnGeneratePlan').style.display = index === mealSections.length - 1 ? '' : 'none';
    document.getElementById('mealStepLabel').textContent = `Meal ${index + 1} of ${mealSections.length}`;
}

document.querySelector('[data-action="next-step"]').addEventListener('click', function () {
    const availableTags = JSON.parse(document.getElementById('dayPlanForm').dataset.tags);
    const count = parseInt(document.getElementById('MealCount').value) || 1;
    const container = document.getElementById('mealPreferenceSections');
    container.innerHTML = '';

    const today = new Date();
    document.getElementById('hiddenMonth').value = today.getMonth() + 1;
    document.getElementById('hiddenDay').value = today.getDate();

    for (let i = 0; i < count; i++) {
        const tagOptions = availableTags
            .map(t => `<option value="${t.id}">${t.name}</option>`)
            .join('');

        const section = document.createElement('div');
        section.className = 'back2 mb-3 p-3';
        section.style.display = 'none';
        section.innerHTML = `
            <div class="row g-3 mb-3">
                <div class="col-md-8">
                    <label class="form-label">Meal title (optional)</label>
                    <input type="text"
                           id="MealPreferences_${i}__Title"
                           name="MealPreferences[${i}].Title"
                           class="form-control"
                           placeholder="e.g. Lunch, Weekend Brunch…" />
                </div>
            </div>
            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">Size</label>
                    <select id="MealPreferences_${i}__Size"
                            name="MealPreferences[${i}].Size"
                            class="form-control">
                        <option value="Small">Small</option>
                        <option value="Average" selected>Average</option>
                        <option value="Large">Large</option>
                    </select>
                </div>
                <div class="col-md-8">
                    <label class="form-label">Food type (tags)</label>
                    <select id="MealPreferences_${i}__TagIds"
                            name="MealPreferences[${i}].TagIds"
                            class="form-control"
                            multiple>
                        ${tagOptions}
                    </select>
                    <input type="text"
                           id="MealPreferences_${i}__CustomTagName"
                           name="MealPreferences[${i}].CustomTagName"
                           class="form-control mt-2"
                           placeholder="Or type a custom tag…" />
                </div>
            </div>`;
        container.appendChild(section);
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
