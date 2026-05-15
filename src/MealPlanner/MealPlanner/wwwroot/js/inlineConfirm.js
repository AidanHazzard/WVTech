// Handle .btn-delete-meal clicks on EditMeal and PlannerHome pages
document.addEventListener('DOMContentLoaded', function () {
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btn-delete-meal');
        if (!btn) return;
        e.preventDefault();

        showDeleteModal('Delete this meal?', function () {
            btn.closest('form').submit();
        });
    });
});
