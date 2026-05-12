document.querySelectorAll(".date-range-input").forEach((input) => {
  input.addEventListener("change", function () {
    document.getElementById("dateRangeForm").submit();
  });
});

function toggleSaveButton(input) {
  const btn = input.closest("form").querySelector(".qty-save");
  btn.style.display =
    parseFloat(input.value) !== parseFloat(input.dataset.original)
      ? "inline-block"
      : "none";
}

document.querySelectorAll(".qty-input").forEach((input) => {
  input.addEventListener("input", function () {
    toggleSaveButton(this);
  });
});

const findStoresBtn = document.getElementById("findKrogerStores");
if (findStoresBtn) {
  findStoresBtn.addEventListener("click", async function () {
    const zip = document.getElementById("ZipCode").value.trim();
    if (!zip) return;

    const section = document.getElementById("krogerStoreSection");
    const message = document.getElementById("krogerStoreMessage");
    const select = document.getElementById("krogerStoreSelect");

    findStoresBtn.disabled = true;
    findStoresBtn.textContent = "Searching...";
    section.style.display = "none";
    message.style.display = "none";

    let stores = [];
    try {
      const [storesRes] = await Promise.all([
        fetch(`/Kroger/Stores?zipCode=${encodeURIComponent(zip)}`),
        fetch("/Kroger/SaveZip", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ zipCode: zip }),
        }),
      ]);
      stores = await storesRes.json();
    } catch {
      stores = [];
    }

    findStoresBtn.disabled = false;
    findStoresBtn.textContent = "Find Stores";
    section.style.display = "block";
    document.getElementById("zipCodeHidden").value = zip;
    select.innerHTML = "";

    message.style.display = "block";

    if (stores.length === 0) {
      message.textContent = `No Kroger stores found within 50 miles of "${zip}". Try a nearby zip code.`;
      select.style.display = "none";
      document.getElementById("exportToKroger").style.display = "none";
      return;
    }

    select.style.display = "";
    document.getElementById("exportToKroger").style.display = "";

    const exactMatch = stores.some((s) => s.zipCode === zip);
    if (!exactMatch) {
      message.textContent = `No Kroger in ${zip}. showing nearest store(s):`;
    } else {
      message.textContent = `Found ${stores.length} store(s) near ${zip}:`;
    }

    const storeTmpl = document.getElementById("tmpl-store-option");
    stores.forEach((store) => {
      const option = storeTmpl.content.cloneNode(true).querySelector("option");
      option.value = store.locationId;
      option.textContent = `${store.name} — ${store.addressLine1}, ${store.city}, ${store.state} ${store.zipCode}`;
      select.appendChild(option);
    });

    if (window.krogerLastStoreId) {
      select.value = window.krogerLastStoreId;
    }
  });
}

const exportForm = document.getElementById("krogerExportForm");
if (exportForm) {
  exportForm.addEventListener("submit", function () {
    const btn = document.getElementById("exportToKroger");
    const spinner = document.getElementById("exportSpinner");
    btn.disabled = true;
    btn.textContent = "Exporting...";
    if (spinner) spinner.style.display = "inline";
  });
}
