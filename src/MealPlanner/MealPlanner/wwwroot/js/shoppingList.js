function parseAmountFloat(str) {
  str = str.trim();
  const mixed = str.match(/^(\d+)\s+(\d+)\/(\d+)$/);
  if (mixed) return parseInt(mixed[1]) + parseInt(mixed[2]) / parseInt(mixed[3]);
  const frac = str.match(/^(\d+)\/(\d+)$/);
  if (frac) return parseInt(frac[1]) / parseInt(frac[2]);
  const num = parseFloat(str);
  return isNaN(num) ? 0 : num;
}

function formatAmountStr(val) {
  if (val <= 0) return "0";
  const whole = Math.floor(val);
  const frac = val - whole;
  const common = [[1,8],[1,4],[1,3],[3,8],[1,2],[5,8],[2,3],[3,4],[7,8]];
  if (frac < 0.05) return String(whole || "0");
  let best = null, bestDiff = Infinity;
  for (const [n, d] of common) {
    const diff = Math.abs(frac - n / d);
    if (diff < bestDiff) { bestDiff = diff; best = [n, d]; }
  }
  if (best && bestDiff < 0.1) {
    const fracStr = `${best[0]}/${best[1]}`;
    return whole > 0 ? `${whole} ${fracStr}` : fracStr;
  }
  return val % 1 === 0 ? String(val) : val.toFixed(2);
}

document.addEventListener("click", function (e) {
  const btn = e.target.closest(".qty-increment, .qty-decrement");
  if (!btn) return;
  const input = btn.closest(".qty-controls").querySelector(".qty-input");
  const val = parseAmountFloat(input.value);
  if (btn.classList.contains("qty-increment")) {
    input.value = formatAmountStr(val + 1);
  } else {
    input.value = formatAmountStr(Math.max(0, val - 1));
  }
  input.closest("form").submit();
});

document.querySelectorAll(".qty-input").forEach((input) => {
  input.addEventListener("blur", function () {
    if (input.value.trim() !== input.dataset.original.trim()) {
      input.closest("form").submit();
    }
  });
});

document.querySelectorAll(".date-range-input").forEach((input) => {
  input.addEventListener("change", function () {
    document.getElementById("dateRangeForm").submit();
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
