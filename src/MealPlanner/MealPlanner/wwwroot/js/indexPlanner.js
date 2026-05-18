document.addEventListener('DOMContentLoaded', function () {
    const leftArrow = document.getElementById('ALeft');
    const rightArrow = document.getElementById('ARight');
    const container = document.getElementById('back1DatesInnerContainer');

    const dateText = document.getElementById('modelsDate').textContent;
    const curDate = new Date(dateText);

    for (let i = 400; i >= 1; i--) {
        let nextDate = new Date(curDate);
        nextDate.setDate(curDate.getDate() - i);
        MakeDate(nextDate);
    }

    MakeSelected();

    for (let i = 1; i < 400; i++) {
        let nextDate = new Date(curDate);
        nextDate.setDate(curDate.getDate() + i);
        MakeDate(nextDate);
    }

    document.getElementById('Selected').scrollIntoView({ behavior: 'instant', block: 'nearest', inline: 'center' });

    // ── Day-nav arrows ───────────────────────────────────────────
    function navigateToDay(date) {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        window.location.href = `/Home/Index?date=${y}-${m}-${d}`;
    }

    const dayNavPrev = document.getElementById('dayNavPrev');
    const dayNavNext = document.getElementById('dayNavNext');

    if (dayNavPrev) {
        dayNavPrev.addEventListener('click', function () {
            const prev = new Date(curDate);
            prev.setDate(curDate.getDate() - 1);
            navigateToDay(prev);
        });
    }

    if (dayNavNext) {
        dayNavNext.addEventListener('click', function () {
            const next = new Date(curDate);
            next.setDate(curDate.getDate() + 1);
            navigateToDay(next);
        });
    }

    function MakeDate(nextDate) {
        nextDate = new Date(nextDate);
        const shortMonth = nextDate.toLocaleString('en-US', { month: 'short' });

        const form = document.createElement('form');
        form.action = '/Home/Index';
        form.method = 'get';

        const btn = document.createElement('button');
        btn.className = 'date-chip';
        btn.name = 'date';
        btn.value = nextDate.toISOString();
        btn.textContent = `${shortMonth} ${nextDate.getDate()}`;

        form.append(btn);
        container.append(form);
    }

    function MakeSelected() {
        const shortMonth = curDate.toLocaleString('en-US', { month: 'short' });

        const form = document.createElement('form');
        form.action = '/Home/Index';
        form.method = 'get';

        const div = document.createElement('div');
        div.id = 'Selected';
        div.className = 'date-chip selected';
        div.textContent = `${shortMonth} ${curDate.getDate()}`;

        form.append(div);
        container.append(form);
    }

    let scrollInterval;

    function startScroll(direction) {
        scrollInterval = setInterval(() => {
            container.scrollBy({ left: direction * 10, behavior: 'auto' });
        }, 5);
    }

    function stopScroll() {
        clearInterval(scrollInterval);
    }

    leftArrow.addEventListener('mousedown', () => startScroll(-1));
    leftArrow.addEventListener('touchstart', () => startScroll(-1));
    leftArrow.addEventListener('mouseup', stopScroll);
    leftArrow.addEventListener('mouseleave', stopScroll);
    leftArrow.addEventListener('touchend', stopScroll);

    rightArrow.addEventListener('mousedown', () => startScroll(1));
    rightArrow.addEventListener('touchstart', () => startScroll(1));
    rightArrow.addEventListener('mouseup', stopScroll);
    rightArrow.addEventListener('mouseleave', stopScroll);
    rightArrow.addEventListener('touchend', stopScroll);

    let isDragging = false;
    let hasDragged = false;
    let dragStartX = 0;
    let scrollStartLeft = 0;

    container.addEventListener('mousedown', (e) => {
        isDragging = true;
        hasDragged = false;
        dragStartX = e.clientX;
        scrollStartLeft = container.scrollLeft;
        container.classList.add('dragging');
        e.preventDefault();
    });

    window.addEventListener('mousemove', (e) => {
        if (!isDragging) return;
        const dx = e.clientX - dragStartX;
        if (Math.abs(dx) > 5) hasDragged = true;
        container.scrollLeft = scrollStartLeft - dx;
    });

    window.addEventListener('mouseup', () => {
        if (!isDragging) return;
        isDragging = false;
        container.classList.remove('dragging');
    });

    container.addEventListener('click', (e) => {
        if (hasDragged) {
            e.preventDefault();
            e.stopPropagation();
            hasDragged = false;
        }
    }, true);

    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', function (e) {
            const checkbox = this.querySelector('.MealCheckBox');
            if (checkbox && checkbox.checked) {
                e.preventDefault();
            }
        });
    });

    // Update card visual state when the completion checkbox changes
    document.querySelectorAll('.MealCheckBox').forEach(function (checkbox) {
        checkbox.addEventListener('change', function () {
            const card = this.closest('.meal-card--filled');
            if (card) {
                card.classList.toggle('meal-card--checked', this.checked);
            }
        });
    });

    // ── Repeat-delete modal ─────────────────────────────────────
    const repeatModal = document.getElementById('repeatDeleteModal');
    const rdmThisOnly = document.getElementById('rdmThisOnly');
    const rdmAllFuture = document.getElementById('rdmAllFuture');
    const rdmCancel = document.getElementById('rdmCancel');
    let pendingDeleteForm = null;

    function showRepeatModal(form) {
        pendingDeleteForm = form;
        repeatModal.style.display = 'flex';
    }

    function closeRepeatModal() {
        repeatModal.style.display = 'none';
        pendingDeleteForm = null;
    }

    if (repeatModal) {
        rdmThisOnly.addEventListener('click', function () {
            if (!pendingDeleteForm) return;
            const form = pendingDeleteForm;
            closeRepeatModal();
            form.submit();
        });

        rdmAllFuture.addEventListener('click', function () {
            if (!pendingDeleteForm) return;
            const form = pendingDeleteForm;
            let deleteAllInput = form.querySelector('.rdm-delete-all-input');
            if (!deleteAllInput) {
                deleteAllInput = document.createElement('input');
                deleteAllInput.type = 'hidden';
                deleteAllInput.name = 'deleteAll';
                deleteAllInput.className = 'rdm-delete-all-input';
                form.appendChild(deleteAllInput);
            }
            deleteAllInput.value = 'true';
            closeRepeatModal();
            form.submit();
        });

        rdmCancel.addEventListener('click', closeRepeatModal);

        repeatModal.addEventListener('click', function (e) {
            if (e.target === repeatModal) closeRepeatModal();
        });
    }

    // ── Empty meal slot deletion ────────────────────────────────
    const mealGrid = document.querySelector('.meal-cards-grid');
    if (mealGrid) {
        let suppressOverlayNav = false;

        // Touch: first tap shows overlay, second tap navigates
        mealGrid.addEventListener('touchstart', function (e) {
            const card = e.target.closest('.meal-card--filled');
            if (!card) return;

            // Tapping the delete or complete area — don't interfere
            if (e.target.closest('.meal-card-delete-form') || e.target.closest('.meal-card-complete-btn') || e.target.closest('.mealCompleteForm')) return;

            if (!card.classList.contains('overlay-visible')) {
                card.classList.add('overlay-visible');
                suppressOverlayNav = true;
                e.preventDefault();
            }
        }, { passive: false });

        // Dismiss overlay when tapping outside a card
        document.addEventListener('touchstart', function (e) {
            if (!e.target.closest('.meal-card--filled')) {
                document.querySelectorAll('.meal-card--filled.overlay-visible').forEach(c => c.classList.remove('overlay-visible'));
            }
        });

        mealGrid.addEventListener('click', function (e) {
            // Overlay / edit button clicked on a filled card → navigate to Edit Meal
            const overlayClick = e.target.closest('.meal-card-overlay');
            if (overlayClick) {
                if (suppressOverlayNav) { suppressOverlayNav = false; return; }
                const card = overlayClick.closest('.meal-card--filled');
                if (card && card.dataset.editUrl) {
                    const url = new URL(card.dataset.editUrl, window.location.href);
                    url.searchParams.set('returnUrl', window.location.href);
                    window.location.href = url.toString();
                }
                return;
            }

            // Delete button clicked — intercept repeat meals to show modal
            const deleteBtn = e.target.closest('.meal-card-delete-btn');
            if (deleteBtn) {
                e.stopPropagation();
                const card = deleteBtn.closest('.meal-card--filled');
                if (card && card.dataset.repeat === 'true') {
                    e.preventDefault();
                    const form = card.querySelector('.meal-card-delete-form');
                    if (form) showRepeatModal(form);
                }
                return;
            }

            // Delete form area clicked → don't navigate via overlay
            if (e.target.closest('.meal-card-delete-form')) return;

            // Complete form area clicked → don't navigate via overlay
            if (e.target.closest('.mealCompleteForm')) return;

            // Subtle delete button clicked → show inline confirm
            const emptyDeleteBtn = e.target.closest('.meal-card-empty-delete-btn');
            if (emptyDeleteBtn) {
                e.stopPropagation();
                const card = emptyDeleteBtn.closest('.meal-card--empty');
                if (!card) return;
                card.querySelector('.meal-card-normal-content').style.display = 'none';
                card.querySelector('.meal-card-empty-delete-btn').style.display = 'none';
                card.querySelector('.meal-card-confirm-content').style.display = '';
                card.classList.add('confirming');
                return;
            }

            // Cancel → restore card
            const cancelBtn = e.target.closest('.meal-card-confirm-cancel');
            if (cancelBtn) {
                const card = cancelBtn.closest('.meal-card--empty');
                if (!card) return;
                card.querySelector('.meal-card-confirm-content').style.display = 'none';
                card.querySelector('.meal-card-normal-content').style.display = '';
                card.querySelector('.meal-card-empty-delete-btn').style.display = '';
                card.classList.remove('confirming');
                return;
            }

            // Remove → delete card, check if grid is now empty
            const removeBtn = e.target.closest('.meal-card-confirm-remove');
            if (removeBtn) {
                const card = removeBtn.closest('.meal-card--empty');
                if (!card) return;
                card.remove();
                if (mealGrid.querySelectorAll('.meal-card').length === 0) {
                    mealGrid.style.display = 'none';
                    const emptyState = document.getElementById('meal-empty-state');
                    if (emptyState) emptyState.style.display = '';
                }
            }
        });
    }
});
