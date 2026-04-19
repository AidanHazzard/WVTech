document.querySelectorAll('[data-action="regenerate-meal"]').forEach(btn => {
    btn.addEventListener('click', () => {
        const form = document.getElementById(`regenerate-form-${btn.dataset.mealId}`);
        form.style.display = form.style.display === 'none' ? '' : 'none';
    });
});

document.querySelectorAll('[data-action="cancel-regenerate"]').forEach(btn => {
    btn.addEventListener('click', () => {
        document.getElementById(`regenerate-form-${btn.dataset.mealId}`).style.display = 'none';
    });
});

document.querySelectorAll('[data-action="submit-regenerate"]').forEach(btn => {
    btn.addEventListener('click', async function () {
        const mealId = this.dataset.mealId;
        const form = document.getElementById(`regenerate-form-${mealId}`);
        const formData = new FormData(form.querySelector('form'));

        btn.disabled = true;
        btn.textContent = 'Regenerating…';

        try {
            const response = await fetch(`/Meal/RegenerateMeal?mealId=${mealId}`, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) throw new Error(`Server error ${response.status}`);
            const data = await response.json();

            const card = document.getElementById(`meal-card-${mealId}`);
            card.querySelector('[data-meal-name]').textContent = data.title;

            const recipeList = card.querySelector('[data-recipe-list]');
            if (recipeList) {
                recipeList.innerHTML = data.recipes
                    .map(r => `<li>${r.name} (${r.calories} cal)</li>`)
                    .join('');
            }

            form.style.display = 'none';
        } catch (e) {
            alert('Failed to regenerate meal. Please try again.');
        } finally {
            btn.disabled = false;
            btn.textContent = 'Regenerate';
        }
    });
});
