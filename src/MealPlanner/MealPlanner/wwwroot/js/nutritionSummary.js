// nutritionSummary.js
// window.nutritionData must be set before this script runs:
//   { allDays: DailyNutritionDto[], dailyTargets: MacroTargets, initialTab: string }

const { allDays, dailyTargets, initialTab } = window.nutritionData;

let activeTab = initialTab || 'weekly';
let barChart = null;

const MONTH_NAMES = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];

// ── Helpers ──────────────────────────────────────────────────────────────────

function parseLocalDate(isoString) {
    const [y, m, d] = isoString.split('-').map(Number);
    return new Date(y, m - 1, d);
}

const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

function sum(days) {
    return {
        calories: days.reduce((s, d) => s + d.calories, 0),
        protein:  days.reduce((s, d) => s + d.protein,  0),
        carbs:    days.reduce((s, d) => s + d.carbs,    0),
        fat:      days.reduce((s, d) => s + d.fat,      0),
    };
}

function capitalize(s) { return s.charAt(0).toUpperCase() + s.slice(1); }

function weeklyDays() { return allDays.slice(-7); }

function monthlyWeeks() {
    const splits = [[0,7],[7,14],[14,21],[21,30]];
    return splits.map(([start, end], i) => {
        const chunk = allDays.slice(start, end);
        const totals = sum(chunk);
        return { label: `W${i + 1}`, ...totals, goal: dailyTargets.calories * chunk.length, days: chunk.length };
    });
}

function macroColor(macro) {
    return { calories: '#80eef6', protein: '#5eff9b', carbs: '#00ffff', fat: '#ff98ff' }[macro] ?? '#aaa';
}

// ── Metric cards ─────────────────────────────────────────────────────────────

function updateCard(macro, actual, goal, unit) {
    const pct = goal > 0 ? Math.min((actual / goal) * 100, 100) : 0;
    const over = actual > goal;
    document.getElementById(`card-${macro}-actual`).textContent = actual.toLocaleString();
    document.getElementById(`card-${macro}-goal`).textContent   = `/ ${goal.toLocaleString()} ${unit}`;
    const bar = document.getElementById(`card-${macro}-bar`);
    bar.style.width           = `${pct}%`;
    bar.style.backgroundColor = over ? '#ff6b6b' : macroColor(macro);
}

// ── Bar chart ─────────────────────────────────────────────────────────────────

function updateBarChart(isWeekly, days) {
    const labels = days.map(d => {
        const dt = parseLocalDate(d.day);
        return isWeekly
            ? DAY_NAMES[dt.getDay()]
            : `${MONTH_NAMES[dt.getMonth()]} ${dt.getDate()}`;
    });
    const values = days.map(d => d.calories);
    const goals  = days.map(() => dailyTargets.calories);
    const textColor = getComputedStyle(document.documentElement).getPropertyValue('--text-primary').trim() || '#fff';

    if (barChart) barChart.destroy();
    barChart = new Chart(document.getElementById('barChart').getContext('2d'), {
        type: 'bar',
        data: {
            labels,
            datasets: [
                {
                    type: 'bar',
                    label: 'Calories',
                    data: values,
                    backgroundColor: '#80eef6',
                    order: 2,
                },
                {
                    type: 'line',
                    label: 'Goal',
                    data: goals,
                    borderColor: '#ff6b6b',
                    borderDash: [6, 4],
                    borderWidth: 2,
                    pointRadius: 0,
                    fill: false,
                    order: 1,
                },
            ],
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { color: textColor },
                },
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { color: textColor },
                },
                x: {
                    ticks: {
                        maxTicksLimit: isWeekly ? 7 : 8,
                        autoSkip: true,
                        maxRotation: 0,
                        color: textColor,
                    },
                },
            },
        },
    });
}

// ── Macro breakdown card ──────────────────────────────────────────────────────

const MACRO_COLORS = { calories: '#80eef6', protein: '#5eff9b', carbs: '#ffce56', fat: '#ff98ff' };

function updateMacroCard(totals, numDays) {
    ['calories', 'protein', 'carbs', 'fat'].forEach(m => {
        const actual    = totals[m];
        const goal      = dailyTargets[m] * numDays;
        const pct       = goal > 0 ? Math.min((actual / goal) * 100, 100) : 0;
        const over      = actual > goal;
        const actualEl  = document.getElementById(`macro-${m}-actual`);
        const goalEl    = document.getElementById(`macro-${m}-goal`);
        const bar       = document.getElementById(`macro-${m}-bar`);
        if (actualEl) actualEl.textContent = actual.toLocaleString();
        if (goalEl)   goalEl.textContent   = goal.toLocaleString();
        if (bar) {
            bar.style.width           = `${pct}%`;
            bar.style.backgroundColor = over ? '#ff6b6b' : MACRO_COLORS[m];
        }
    });
}

// ── Pills ─────────────────────────────────────────────────────────────────────

function indicator(actual, goal) {
    if (!goal) return '–';
    const r = actual / goal;
    if (r >= 0.9 && r <= 1.1) return '✓';
    return r > 1.1 ? '↑' : '↓';
}

const INDICATOR_COLORS = { '✓': '#5eff9b', '↑': '#ff6b6b', '↓': '#ffce56', '–': '#aaa' };

function pillColors() {
    return {
        bg:        { '✓': 'rgba(94,255,155,0.20)',  '↑': 'rgba(255,107,107,0.20)', '↓': 'rgba(255,206,86,0.20)',  '–': 'rgba(128,128,128,0.10)' },
        border:    { '✓': 'rgba(94,255,155,0.60)',  '↑': 'rgba(255,107,107,0.60)', '↓': 'rgba(255,206,86,0.60)',  '–': 'rgba(128,128,128,0.30)' },
        indicator: { '✓': '#5eff9b', '↑': '#ff6b6b', '↓': '#ffce56', '–': '#aaa' },
    };
}

function updatePills(isWeekly, days) {
    const container = document.getElementById('pills-container');
    container.innerHTML = '';
    const { bg, border, indicator: indColors } = pillColors();

    const items = days.map(d => {
        const dt = parseLocalDate(d.day);
        const label = isWeekly
            ? DAY_NAMES[dt.getDay()]
            : `${MONTH_NAMES[dt.getMonth()]} ${dt.getDate()}`;
        return { label, actual: d.calories, goal: dailyTargets.calories };
    });

    items.forEach(({ label, actual, goal }) => {
        const ind = indicator(actual, goal);
        const pill = document.createElement('div');
        pill.className = 'nutrition-pill';
        pill.style.background  = bg[ind];
        pill.style.borderColor = border[ind];
        pill.innerHTML = `<span class="pill-label">${label}</span><span class="pill-indicator" style="color:${indColors[ind]}">${ind}</span>`;
        container.appendChild(pill);
    });
}

// ── Insights ──────────────────────────────────────────────────────────────────

function generateInsights(isWeekly, days, totals) {
    const period = isWeekly ? 'week' : 'month';

    // 1 — Goal hit rate
    let i1;
    if (isWeekly) {
        const hits = days.filter(d => {
            const r = dailyTargets.calories > 0 ? d.calories / dailyTargets.calories : 0;
            return r >= 0.9 && r <= 1.1;
        }).length;
        i1 = `You hit your daily calorie goal on ${hits} / ${days.length} days this ${period}.`;
    } else {
        const weeks = monthlyWeeks();
        const hits  = weeks.filter(w => { const r = w.goal > 0 ? w.calories / w.goal : 0; return r >= 0.9 && r <= 1.1; }).length;
        i1 = `You hit your weekly calorie goal on ${hits} / 4 weeks this ${period}.`;
    }

    // 2 — Most / least consistent macro
    const macros = ['protein', 'carbs', 'fat'];
    const consistency = macros.map(m => {
        const goal = dailyTargets[m] * days.length;
        return { macro: m, pct: goal > 0 ? (totals[m] / goal) * 100 : 0 };
    }).sort((a, b) => Math.abs(100 - a.pct) - Math.abs(100 - b.pct));
    const i2 = `Most consistent macro: ${capitalize(consistency[0].macro)} (${consistency[0].pct.toFixed(0)}% of goal). ` +
               `Least consistent: ${capitalize(consistency[2].macro)} (${consistency[2].pct.toFixed(0)}% of goal).`;

    document.getElementById('insight-1').textContent = i1;
    document.getElementById('insight-2').textContent = i2;
}

// ── Render ────────────────────────────────────────────────────────────────────

function render() {
    const isWeekly = activeTab === 'weekly';
    const days     = isWeekly ? weeklyDays() : allDays;
    const totals   = sum(days);
    const numDays  = days.length;
    const goal     = {
        calories: dailyTargets.calories * numDays,
        protein:  dailyTargets.protein  * numDays,
        carbs:    dailyTargets.carbs    * numDays,
        fat:      dailyTargets.fat      * numDays,
    };

    // Tabs
    ['weekly', 'monthly'].forEach(t => {
        const btn = document.getElementById(`tab-${t}`);
        const on  = t === activeTab;
        btn.classList.toggle('btn-primary',         on);
        btn.classList.toggle('btn-outline-primary', !on);
        btn.classList.toggle('active',              on);
    });

    // Cards
    updateCard('calories', totals.calories, goal.calories, 'kcal');
    updateCard('protein',  totals.protein,  goal.protein,  'g');
    updateCard('carbs',    totals.carbs,    goal.carbs,    'g');
    updateCard('fat',      totals.fat,      goal.fat,      'g');

    // Charts
    updateBarChart(isWeekly, days);

    // Macro breakdown card always shows today only
    const now = new Date();
    const todayStr = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
    const todayData = allDays.find(d => d.day === todayStr) || { calories: 0, protein: 0, carbs: 0, fat: 0 };
    updateMacroCard(todayData, 1);

    // Pills
    updatePills(isWeekly, days);

    // Insights
    generateInsights(isWeekly, days, totals);
}

// ── Boot ──────────────────────────────────────────────────────────────────────

document.getElementById('tab-weekly').addEventListener('click',  () => { activeTab = 'weekly';  render(); });
document.getElementById('tab-monthly').addEventListener('click', () => { activeTab = 'monthly'; render(); });

render();
