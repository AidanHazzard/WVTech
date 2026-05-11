function showInlineConfirm(triggerBtn, message, onConfirm) {
    const parent = triggerBtn.closest('.mealRecipeItem, form') || triggerBtn.parentElement;

    if (parent.querySelector('.inline-confirm')) return;

    triggerBtn.style.display = 'none';

    const div = document.createElement('div');
    div.className = 'inline-confirm d-flex align-items-center gap-2';
    div.innerHTML = `
        <span class="fw-semibold">${message}</span>
        <button type="button" class="btn btn-danger btn-sm inline-confirm-yes">Yes</button>
        <button type="button" class="btn btn-secondary btn-sm inline-confirm-no">Cancel</button>
    `;

    div.querySelector('.inline-confirm-yes').addEventListener('click', function (e) {
        e.stopPropagation();
        div.remove();
        onConfirm();
    });

    div.querySelector('.inline-confirm-no').addEventListener('click', function (e) {
        e.stopPropagation();
        div.remove();
        triggerBtn.style.display = '';
    });

    triggerBtn.insertAdjacentElement('afterend', div);
}

// Handle .btn-delete-meal clicks on EditMeal and PlannerHome pages
document.addEventListener('DOMContentLoaded', function () {
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-delete-meal');
        if (!btn) return;
        e.preventDefault();

        showInlineConfirm(btn, 'Delete this meal?', function () {
            btn.closest('form').submit();
        });
    });
});
