function parseAmountFloat(str) {
  str = str.trim();
  const mixed = str.match(/^(\d+)\s+(\d+)\/(\d+)$/);
  if (mixed) return parseInt(mixed[1]) + parseInt(mixed[2]) / parseInt(mixed[3]);
  const frac = str.match(/^(\d+)\/(\d+)$/);
  if (frac) return parseInt(frac[1]) / parseInt(frac[2]);
  const num = parseFloat(str);
  return isNaN(num) ? 0 : num;
}
const today = new Date().toISOString().split("T")[0];

// Guard against double-submission when both change and blur fire together.
let _submitting = false;
function submitDateForm() {
  if (_submitting) return;
  _submitting = true;
  document.getElementById("dateRangeForm").submit();
}

// Shared debounce timer across both inputs so rapid section edits (MM→DD→YYYY)
// only trigger one submission after the user finishes all three sections.
let _debounceTimer = null;
function scheduleSubmit() {
  clearTimeout(_debounceTimer);
  _debounceTimer = setTimeout(submitDateForm, 600);
}

document.querySelectorAll(".date-range-input").forEach((input) => {
  // change fires whenever a section completes and the full date becomes valid.
  // Debounce so the user can edit all three sections before the form submits.
  input.addEventListener("change", function () {
    if (!this.value) {
      this.classList.add("date-invalid");
      clearTimeout(_debounceTimer);
      return;
    }
    this.classList.remove("date-invalid");
    scheduleSubmit();
  });

  // Show red border while the value is incomplete (value is "" mid-type).
  input.addEventListener("input", function () {
    if (this.value) {
      this.classList.remove("date-invalid");
    } else {
      this.classList.add("date-invalid");
    }
  });

  // On blur: user left the field — cancel any pending debounce and commit now.
  // If empty, revert to today first.
  input.addEventListener("blur", function () {
    clearTimeout(_debounceTimer);
    if (!this.value) {
      this.value = today;
      this.classList.remove("date-invalid");
    } else {
      this.classList.remove("date-invalid");
    }
    submitDateForm();
  });
});

function toggleSaveButton(input) {
  const btn = input.closest("form").querySelector(".qty-save");
  btn.style.display =
    parseFloat(input.value) !== parseFloat(input.dataset.original)
      ? "inline-block"
      : "none";
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
  const isDecimal = input.value.trim().includes('.') && !input.value.includes('/');
  let newDisplay;
  if (isDecimal) {
    if (newVal <= 0) {
      newDisplay = '0';
    } else {
      const decimals = (input.value.trim().split('.')[1] || '').length;
      newDisplay = newVal.toFixed(decimals);
    }
  } else {
    newDisplay = den > 1 ? formatWithDenominator(newVal, den) : formatAmountStr(newVal);
  }

  input.value = newDisplay;
  input.dataset.original = newDisplay;

  await fetch("/Shopping/UpdateItemAmountJson", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ ingredientBaseId, newAmount: newDisplay })
  }).catch(() => {});
});

document.addEventListener("focusout", function (e) {
  if (!e.target.matches(".qty-input")) return;
  const input = e.target;
  if (input.value.trim() !== input.dataset.original.trim()) {
    input.closest("form").submit();
  }
});

document.addEventListener("focusout", async function (e) {
  if (!e.target.matches(".measurement-inline-input")) return;
  const input = e.target;
  const newVal = input.value.trim();
  if (!newVal || newVal === input.dataset.original) return;

  const itemId = parseInt(input.dataset.itemId);
  const resp = await fetch("/Shopping/UpdateItemMeasurementJson", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ itemId, measurement: newVal })
  }).catch(() => null);

  if (resp && resp.ok) {
    const data = await resp.json();
    input.value = data.abbreviation;
    input.dataset.original = data.abbreviation;
  } else {
    input.value = input.dataset.original;
  }
});

let _dateRangeTimer = null;
let _dateRangeController = null;

document.querySelectorAll(".date-range-input").forEach((input) => {
  input.addEventListener("change", function () {
    clearTimeout(_dateRangeTimer);
    if (_dateRangeController) {
      _dateRangeController.abort();
      _dateRangeController = null;
    }

    const dateFrom = document.querySelector('[name="dateFrom"]').value;
    const dateTo = document.querySelector('[name="dateTo"]').value;
    if (!dateFrom || !dateTo) return;

    _dateRangeTimer = setTimeout(async () => {
      _dateRangeController = new AbortController();
      const container = document.getElementById("shopping-items-container");
      if (!container) return;

      container.style.opacity = "0.5";
      container.style.pointerEvents = "none";

      let ok = false;
      try {
        const resp = await fetch(
          `/Shopping/GetItemsPartial?dateFrom=${encodeURIComponent(dateFrom)}&dateTo=${encodeURIComponent(dateTo)}&_=${Date.now()}`,
          { credentials: "same-origin", signal: _dateRangeController.signal }
        );
        if (resp.ok) {
          container.innerHTML = await resp.text();
          ok = true;
        }
      } catch (err) {
        if (err.name === "AbortError") return;
      } finally {
        container.style.opacity = "";
        container.style.pointerEvents = "";
      }

      if (!ok) {
        document.getElementById("dateRangeForm").submit();
      }
    }, 300);
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
  message.className = "sl-store-msg";
  message.style.display = "block";

  if (stores.length === 0) {
    message.textContent = `No Kroger stores found within ${radius} miles of "${zip}". Try increasing the radius or check the zip code.`;
    message.className = "sl-alert sl-alert-danger";
    select.style.display = "none";
    document.getElementById("exportToKroger").style.display = "none";
  } else {
    select.style.display = "";
    document.getElementById("exportToKroger").style.display = "";
    const storeWord = stores.length === 1 ? "store" : "stores";
    const exactMatch = stores.some((s) => s.zipCode === zip);
    if (!exactMatch) {
      message.textContent = `No stores in ${zip}. Showing the nearest within ${radius} miles:`;
      message.className = "sl-alert sl-alert-danger";
    } else {
      message.textContent = `${stores.length} ${storeWord} found near ${zip}:`;
    }

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
    const btnText = btn?.querySelector(".buttonText");
    if (btnText) btnText.textContent = "Exporting...";
    if (btn) btn.disabled = true;
  });
}
