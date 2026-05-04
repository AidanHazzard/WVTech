function toggleSaveButton(input) {
    const btn = input.closest('form').querySelector('.qty-save');
    btn.style.display = parseFloat(input.value) !== parseFloat(input.dataset.original) ? 'inline-block' : 'none';
}

document.getElementById('findKrogerStores').addEventListener('click', async function () {
    const zip = document.getElementById('ZipCode').value.trim();
    if (!zip) return;

    const spinner = document.getElementById('krogerStoreSpinner');
    const section = document.getElementById('krogerStoreSection');
    const message = document.getElementById('krogerStoreMessage');
    const select = document.getElementById('krogerStoreSelect');

    spinner.style.display = 'inline';
    section.style.display = 'none';
    message.style.display = 'none';

    let stores = [];
    try {
        const [storesRes] = await Promise.all([
            fetch(`/Kroger/Stores?zipCode=${encodeURIComponent(zip)}`),
            fetch('/Kroger/SaveZip', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ zipCode: zip })
            })
        ]);
        stores = await storesRes.json();
    } catch {
        stores = [];
    }

    spinner.style.display = 'none';
    section.style.display = 'block';
    document.getElementById('zipCodeHidden').value = zip;
    select.innerHTML = '';

    message.style.display = 'block';

    if (stores.length === 0) {
        message.textContent = `No Kroger stores found within 50 miles of "${zip}". Try a nearby zip code.`;
        message.style.color = '#dc3545';
        select.style.display = 'none';
        document.getElementById('exportToKroger').style.display = 'none';
        return;
    }

    select.style.display = '';
    document.getElementById('exportToKroger').style.display = '';

    const exactMatch = stores.some(s => s.zipCode === zip);
    if (!exactMatch) {
        message.textContent = `No Kroger in ${zip} — showing nearest store(s):`;
        message.style.color = '#ffc107';
    } else {
        message.textContent = `Found ${stores.length} store(s) near ${zip}:`;
        message.style.color = 'var(--text-primary)';
    }

    stores.forEach(store => {
        const option = document.createElement('option');
        option.value = store.locationId;
        option.textContent = `${store.name} — ${store.addressLine1}, ${store.city}, ${store.state} ${store.zipCode}`;
        select.appendChild(option);
    });
});

document.querySelectorAll('.qty-input').forEach(input => {
    input.addEventListener('input', function () {
        toggleSaveButton(this);
    });
});
