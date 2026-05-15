document.addEventListener('DOMContentLoaded', function () {

    // ── Recipe search & tag filter ────────────────────────────

    const searchInput = document.getElementById('recipeSearch');
    const filterChips = document.querySelectorAll('.filter-chip');
    const recipeRows  = document.querySelectorAll('#recipeRows .recipe-row');

    let activeTag = '';

    function filterRows() {
        const query = searchInput ? searchInput.value.trim().toLowerCase() : '';
        recipeRows.forEach(row => {
            const name    = row.dataset.name  || '';
            const tags    = row.dataset.tags  || '';
            const tagList = tags.split(',').map(t => t.trim().toLowerCase());

            const matchesSearch = !query || name.includes(query);
            const matchesTag    = !activeTag || tagList.includes(activeTag.toLowerCase());

            row.style.display = (matchesSearch && matchesTag) ? '' : 'none';
        });
    }

    if (searchInput) {
        searchInput.addEventListener('input', filterRows);
    }

    filterChips.forEach(chip => {
        chip.addEventListener('click', () => {
            filterChips.forEach(c => c.classList.remove('active'));
            chip.classList.add('active');
            activeTag = chip.dataset.tag || '';
            filterRows();
        });
    });

    // ── Shopping item checkbox toggle ────────────────────────

    document.querySelectorAll('.shopping-item-row').forEach(row => {
        row.addEventListener('click', () => {
            row.classList.toggle('checked');
        });
    });

});
