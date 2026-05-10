document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('pantryPromptModal');
    const message = document.getElementById('pantryPromptMessage');
    const acceptBtn = document.getElementById('pantryPromptAccept');
    const declineBtn = document.getElementById('pantryPromptDecline');

    let pendingForm = null;
    let pendingChecked = false;

    document.querySelectorAll('.mealCompleteForm').forEach(function (form) {
        const checkbox = form.querySelector('.MealCheckBox');
        if (!checkbox) return;

        checkbox.addEventListener('change', function (e) {
            const nowChecked = checkbox.checked;
            const hasPantryMatch = form.dataset.hasPantryMatch === 'true';
            const hasAutoRemoved = form.dataset.hasAutoRemoved === 'true';

            const needsPrompt = (nowChecked && hasPantryMatch) || (!nowChecked && hasAutoRemoved);

            if (!needsPrompt) {
                form.submit();
                return;
            }

            e.preventDefault();
            pendingForm = form;
            pendingChecked = nowChecked;

            message.textContent = nowChecked
                ? 'Would you like to remove the matching ingredients from your pantry?'
                : 'Would you like to add the previously removed ingredients back to your pantry?';

            modal.style.display = 'flex';
        });
    });

    acceptBtn.addEventListener('click', function () {
        if (!pendingForm) return;
        if (pendingChecked) {
            pendingForm.querySelector('.removePantryInput').value = 'true';
        } else {
            pendingForm.querySelector('.restorePantryInput').value = 'true';
        }
        modal.style.display = 'none';
        pendingForm.submit();
        pendingForm = null;
    });

    declineBtn.addEventListener('click', function () {
        if (!pendingForm) return;
        modal.style.display = 'none';
        pendingForm.submit();
        pendingForm = null;
    });
});
