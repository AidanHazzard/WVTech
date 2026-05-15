window.activeTagFilters = [];

document.addEventListener('DOMContentLoaded', function () {
    initRepeatToggle();
    initDayChips();
    initTagFilterDropdown();
});

// ── Repeat weekly toggle ──────────────────────────────────────

function initRepeatToggle() {
    const checkbox = document.getElementById('repeatWeeklyCheckbox');
    const visual   = document.getElementById('emRepeatToggle');
    const panel    = document.getElementById('emRepeatDaysPanel');
    if (!checkbox || !visual || !panel) return;

    visual.addEventListener('click', function () {
        checkbox.checked = !checkbox.checked;
        visual.classList.toggle('on', checkbox.checked);
        panel.classList.toggle('open', checkbox.checked);
    });
}

// ── Repeat day chips ──────────────────────────────────────────

function initDayChips() {
    document.querySelectorAll('.em-day-chip').forEach(function (chip) {
        chip.addEventListener('click', function (e) {
            e.preventDefault(); // stop browser auto-toggling the nested checkbox a second time
            this.classList.toggle('on');
            const cb = this.querySelector('input[type="checkbox"]');
            if (cb) cb.checked = this.classList.contains('on');
        });
    });
}

// ── Tag filter dropdown ───────────────────────────────────────
// Mirrors the dropdown built in createMeal.js; panel appended to body
// so it floats above any overflow:hidden ancestors.

function initTagFilterDropdown() {
    const container = document.getElementById('tagFilterDropdown');
    const tagFilter  = document.getElementById('tagFilter');
    if (!container || !tagFilter) return;

    container.innerHTML = `
        <button type="button" class="tfd-trigger" id="tfdTrigger">
            <span class="tfd-trigger-label" id="tfdLabel">All tags</span>
            <svg class="tfd-chevron" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"
                 fill="none" stroke="currentColor" stroke-width="2.5"
                 stroke-linecap="round" stroke-linejoin="round">
                <polyline points="6 9 12 15 18 9"/>
            </svg>
        </button>
    `;

    const panel = document.createElement('div');
    panel.className = 'tfd-panel';
    panel.id = 'tfdPanel';
    panel.innerHTML = `
        <div class="tfd-option tfd-all-option active" data-value="">
            <span class="tfd-check"></span>
            <span class="tfd-option-text">All tags</span>
        </div>
    `;
    document.body.appendChild(panel);

    const trigger    = document.getElementById('tfdTrigger');
    const label      = document.getElementById('tfdLabel');
    let selectedTags = [];

    function positionPanel() {
        const rect = trigger.getBoundingClientRect();
        panel.style.top   = `${rect.bottom + 4}px`;
        panel.style.left  = `${rect.left}px`;
        panel.style.width = `${Math.max(rect.width, 180)}px`;
    }

    function closePanel() {
        if (!panel.classList.contains('open')) return;
        panel.classList.remove('open');
        trigger.classList.remove('open');
        window.activeTagFilters = [...selectedTags];
        tagFilter.dispatchEvent(new Event('change'));
    }

    trigger.addEventListener('click', function (e) {
        e.stopPropagation();
        if (panel.classList.contains('open')) {
            closePanel();
        } else {
            positionPanel();
            panel.classList.add('open');
            trigger.classList.add('open');
        }
    });

    panel.addEventListener('click', function (e) {
        e.stopPropagation();
        const option = e.target.closest('.tfd-option');
        if (!option) return;
        const value = option.dataset.value;
        if (value === '') {
            selectedTags = [];
        } else {
            const idx = selectedTags.indexOf(value);
            if (idx === -1) selectedTags.push(value);
            else selectedTags.splice(idx, 1);
        }
        updateSelectionUI(selectedTags);
    });

    document.addEventListener('click', function () { closePanel(); });
    window.addEventListener('scroll', closePanel, { passive: true, capture: true });
    window.addEventListener('resize', closePanel, { passive: true });

    // Populate panel as recipeSearch.js's loadTags() appends options to #tagFilter
    const observer = new MutationObserver(function () {
        Array.from(tagFilter.options).forEach(function (opt) {
            if (!opt.value) return;
            if (panel.querySelector(`.tfd-option[data-value="${CSS.escape(opt.value)}"]`)) return;
            const optEl = document.createElement('div');
            optEl.className = 'tfd-option';
            optEl.dataset.value = opt.value;
            optEl.innerHTML = `<span class="tfd-check"></span><span class="tfd-option-text">${opt.textContent.trim()}</span>`;
            panel.appendChild(optEl);
        });
    });
    observer.observe(tagFilter, { childList: true });

    function updateSelectionUI(selected) {
        panel.querySelector('.tfd-all-option').classList.toggle('active', selected.length === 0);
        panel.querySelectorAll('.tfd-option:not(.tfd-all-option)').forEach(function (opt) {
            opt.classList.toggle('active', selected.includes(opt.dataset.value));
        });
        label.textContent = selected.length === 0
            ? 'All tags'
            : selected.length === 1 ? selected[0] : `${selected.length} tags`;
    }
}
