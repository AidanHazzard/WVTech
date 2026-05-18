// Handle .btn-delete-meal clicks on EditMeal and PlannerHome pages
document.addEventListener('DOMContentLoaded', function () {
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-delete-meal');
        if (!btn) return;
        e.preventDefault();

        // If the form contains an .inline-confirm element, let it handle the confirmation
        if (btn.closest('form') && btn.closest('form').querySelector('.inline-confirm')) return;

        showDeleteModal('Delete this meal?', function () {
            btn.closest('form').submit();
        });
    });
});
