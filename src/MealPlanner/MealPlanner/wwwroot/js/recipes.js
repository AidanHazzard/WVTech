document.addEventListener('DOMContentLoaded', function () {

    // ── Recipe search & tag filter ────────────────────────────

    const searchInput = document.getElementById('searchText');
    const filterChips = document.querySelectorAll('.filter-chip');
    const recipeRows  = document.querySelectorAll('#recipeRows .recipe-row');
    const errorDiv    = document.getElementById('error');

    let activeTag = '';

    function filterRows() {
        const query = searchInput ? searchInput.value.trim().toLowerCase() : '';
        let visibleCount = 0;

        recipeRows.forEach(row => {
            const name    = row.dataset.name  || '';
            const tags    = row.dataset.tags  || '';
            const tagList = tags.split(',').map(t => t.trim().toLowerCase());

            const matchesSearch = !query || name.includes(query);
            const matchesTag    = !activeTag || tagList.includes(activeTag.toLowerCase());
            const visible       = matchesSearch && matchesTag;

            row.style.display = visible ? '' : 'none';
            if (visible) {
                row.classList.add('recipeSearchRow');
                visibleCount++;
            } else {
                row.classList.remove('recipeSearchRow');
            }
        });

        if (errorDiv) {
            if (query && visibleCount === 0) {
                errorDiv.textContent = 'No recipes found, sorry!';
                errorDiv.style.display = '';
            } else {
                errorDiv.style.display = 'none';
            }
        }
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
