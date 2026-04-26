const LIQUID_UNITS = {
    "fl oz":     { toFlOz: 1            },
    "Cup(s)":    { toFlOz: 8            },
    "Pint(s)":   { toFlOz: 16           },
    "Quart(s)":  { toFlOz: 32           },
    "Gallon(s)": { toFlOz: 128          },
    "mL":        { toFlOz: 1 / 29.5735  },
    "L":         { toFlOz: 1 / 0.0295735 }
};

const SOLID_UNITS = {
    "oz":       { toOz: 1            },
    "Ounce(s)": { toOz: 1            },
    "lb":       { toOz: 16           },
    "Pound(s)": { toOz: 16           },
    "g":        { toOz: 1 / 28.3495  },
    "kg":       { toOz: 1 / 0.0283495 },
    "KG":       { toOz: 1 / 0.0283495 }
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

        if (LIQUID_UNITS[measurement]) {
            const destKey = target === "us" ? "fl oz" : target === "metric" ? "mL" : null;
            if (!destKey) { span.textContent = formatAmount(amount) + " " + measurement + " of " + name; return; }
            const converted = (amount * LIQUID_UNITS[measurement].toFlOz) / LIQUID_UNITS[destKey].toFlOz;
            span.textContent = formatAmount(converted) + " " + destKey + " of " + name;
        } else if (SOLID_UNITS[measurement]) {
            const destKey = target === "us" ? "oz" : target === "metric" ? "g" : null;
            if (!destKey) { span.textContent = formatAmount(amount) + " " + measurement + " of " + name; return; }
            const converted = (amount * SOLID_UNITS[measurement].toOz) / SOLID_UNITS[destKey].toOz;
            span.textContent = formatAmount(converted) + " " + destKey + " of " + name;
        } else {
            span.textContent = formatAmount(amount) + " " + measurement + " of " + name;
        }
    });
}

function formatAmount(value) {
    const rounded = Math.round(value * 100) / 100;
    return parseFloat(rounded.toFixed(2)).toString();
}
