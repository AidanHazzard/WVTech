const toggle = document.getElementById('themeToggle');
const html = document.documentElement;

toggle.checked = html.getAttribute('data-theme') === 'light';

toggle.addEventListener('change', function () {
    if (this.checked) {
        html.setAttribute('data-theme', 'light');
    } else {
        html.removeAttribute('data-theme');
    }

    fetch('/UserSettings/ToggleTheme', {
        method: 'POST'
    });
});