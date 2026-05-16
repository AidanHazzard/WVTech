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

function getStep(displayStr) {
  const fracMatch = displayStr.trim().match(/(\d+)\/(\d+)$/);
  if (fracMatch) return 1 / parseInt(fracMatch[2]);
  return 1;
}

function formatWithDenominator(val, den) {
  if (val <= 0) return "0";
  const whole = Math.floor(val);
  const frac = val - whole;
  if (frac < 0.001) return String(whole);
  const num = Math.round(frac * den);
  if (num >= den) return String(whole + 1);
  return whole > 0 ? `${whole} ${num}/${den}` : `${num}/${den}`;
}

document.addEventListener("click", async function (e) {
  const btn = e.target.closest(".qty-increment, .qty-decrement");
  if (!btn) return;
  const input = btn.closest(".qty-controls").querySelector(".qty-input");
  const form = input.closest("form");
  const ingredientBaseId = parseInt(form.querySelector('[name="ingredientBaseId"]').value);

  const val = parseAmountFloat(input.value);
  const step = getStep(input.value);
  const den = step < 1 ? Math.round(1 / step) : 1;
  const newVal = btn.classList.contains("qty-increment")
    ? val + step
    : Math.max(0, val - step);
  const newDisplay = den > 1 ? formatWithDenominator(newVal, den) : formatAmountStr(newVal);

  input.value = newDisplay;
  input.dataset.original = newDisplay;

  await fetch("/Shopping/UpdateItemAmountJson", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ ingredientBaseId, newAmount: newDisplay })
  }).catch(() => {});
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

const radiusSlider = document.getElementById("radiusSlider");
const radiusLabel = document.getElementById("radiusLabel");
if (radiusSlider && radiusLabel) {
  radiusSlider.addEventListener("input", function () {
    radiusLabel.textContent = `${radiusSlider.value} mi`;
  });
  radiusSlider.addEventListener("change", function () {
    const storeSection = document.getElementById("krogerStoreSection");
    const zip = document.getElementById("ZipCode")?.value.trim();
    if (zip && storeSection && storeSection.style.display !== "none") {
      document.getElementById("findKrogerStores")?.click();
    }
  });
}

const zipInput = document.getElementById("ZipCode");
if (zipInput) {
  zipInput.addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
      e.preventDefault();
      document.getElementById("findKrogerStores")?.click();
    }
  });
}

let storeCardOpened = false;

function populateStores(stores, zip, radius) {
  const section = document.getElementById("krogerStoreSection");
  const message = document.getElementById("krogerStoreMessage");
  const select = document.getElementById("krogerStoreSelect");

  document.getElementById("zipCodeHidden").value = zip;
  select.innerHTML = "";
  message.style.display = "block";

  if (stores.length === 0) {
    message.textContent = `No Kroger stores found within ${radius} miles of "${zip}". Try increasing the radius or a nearby zip code.`;
    select.style.display = "none";
    document.getElementById("exportToKroger").style.display = "none";
  } else {
    select.style.display = "";
    document.getElementById("exportToKroger").style.display = "";
    const exactMatch = stores.some((s) => s.zipCode === zip);
    message.textContent = exactMatch
      ? `Found ${stores.length} store(s) within ${radius} miles of ${zip}:`
      : `No Kroger in ${zip}. Showing nearest store(s) within ${radius} miles:`;

    const storeTmpl = document.getElementById("tmpl-store-option");
    stores.forEach((store) => {
      const option = storeTmpl.content.cloneNode(true).querySelector("option");
      option.value = store.locationId;
      option.textContent = `${store.name} — ${store.addressLine1}, ${store.city}, ${store.state} ${store.zipCode}`;
      select.appendChild(option);
    });
    if (window.krogerLastStoreId) select.value = window.krogerLastStoreId;
  }

  if (!storeCardOpened) {
    section.style.display = "block";
    requestAnimationFrame(() => section.classList.add("slide-open"));
    storeCardOpened = true;
  }
}

const findStoresBtn = document.getElementById("findKrogerStores");
if (findStoresBtn) {
  findStoresBtn.addEventListener("click", async function () {
    const zip = document.getElementById("ZipCode").value.trim();
    if (!zip) return;

    const findBtnLabel = findStoresBtn.querySelector(".buttonText");
    findStoresBtn.disabled = true;
    if (findBtnLabel) findBtnLabel.textContent = "Searching...";

    const radius = document.getElementById("radiusSlider")?.value ?? 50;
    let stores = [];
    try {
      const [storesRes] = await Promise.all([
        fetch(`/Kroger/Stores?zipCode=${encodeURIComponent(zip)}&radiusInMiles=${radius}`),
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
    if (findBtnLabel) findBtnLabel.textContent = "Find Stores";
    populateStores(stores, zip, radius);
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
