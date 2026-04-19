// To add a new unit: add an entry with its equivalent value in ounces.
const UNIT_CONVERSIONS = {
    "Cup(s)":   { label: "Cup(s)",   toOz: 8       },
    "Ounce(s)": { label: "Ounce(s)", toOz: 1       },
    "Pound(s)": { label: "Pound(s)", toOz: 16      },
    "L":        { label: "L",        toOz: 33.814  },
    "KG":       { label: "KG",       toOz: 35.274  }
};

function convertUnits(target) {
    document.querySelectorAll(".item-display").forEach(span => {
        const amount = parseFloat(span.dataset.amount);
        const measurement = span.dataset.measurement;
        const name = span.dataset.name;

        if (target === "original" || measurement === "Count") {
            span.textContent = formatAmount(amount) + " " + measurement + " of " + name;
            return;
        }

        const source = UNIT_CONVERSIONS[measurement];
        const dest = UNIT_CONVERSIONS[target];

        if (!source || !dest) {
            span.textContent = formatAmount(amount) + " " + measurement + " of " + name;
            return;
        }

        const converted = (amount * source.toOz) / dest.toOz;
        span.textContent = formatAmount(converted) + " " + dest.label + " of " + name;
    });
}

function formatAmount(value) {
    const rounded = Math.round(value * 100) / 100;
    return parseFloat(rounded.toFixed(2)).toString();
}
