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
