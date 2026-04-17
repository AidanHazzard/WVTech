document.getElementById('showDayPlanWizard')?.addEventListener('click', function () {
    document.getElementById('dayPlanWizard').style.display = '';
    this.closest('.back1').style.paddingBottom = '';
});

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

        container.innerHTML += `
            <div class="back2 mb-3 p-3">
                <h5>Meal ${i + 1}</h5>
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
                </div>
            </div>`;
    }

    document.getElementById('step1').style.display = 'none';
    document.getElementById('step2').style.display = '';
});
