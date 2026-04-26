const pendingTags = new Set();

function addPendingTag(name, form, pendingContainer) {
    name = name.trim();
    if (!name || pendingTags.has(name)) return;

    pendingTags.add(name);

    const pill = document.createElement('span');
    pill.className = 'food-pref-pending-pill badge bg-secondary me-1';
    pill.textContent = name + ' ×';
    pill.style.cursor = 'pointer';
    pill.dataset.tagName = name;
    pill.addEventListener('click', function () {
        removePendingTag(name, form, pill);
    });
    pendingContainer.appendChild(pill);

    const hidden = document.createElement('input');
    hidden.type = 'hidden';
    hidden.name = 'NewPreferences';
    hidden.value = name;
    hidden.dataset.pendingTag = name;
    form.appendChild(hidden);
}

function removePendingTag(name, form, pill) {
    pendingTags.delete(name);
    pill.remove();
    const hidden = form.querySelector(`input[data-pending-tag="${CSS.escape(name)}"]`);
    if (hidden) hidden.remove();
}

document.addEventListener('DOMContentLoaded', function () {
    const select = document.getElementById('food-pref-select');
    const customInput = document.getElementById('food-pref-custom-input');
    const addBtn = document.getElementById('food-pref-add-btn');
    const pendingContainer = document.getElementById('food-pref-pending-container');
    const form = document.getElementById('food-pref-form');

    if (select) {
        select.addEventListener('change', function () {
            if (select.value) {
                addPendingTag(select.value, form, pendingContainer);
                select.value = '';
            }
        });
    }

    if (addBtn) {
        addBtn.addEventListener('click', function () {
            addPendingTag(customInput.value, form, pendingContainer);
            customInput.value = '';
        });
    }
});
