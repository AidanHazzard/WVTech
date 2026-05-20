document.addEventListener('DOMContentLoaded', function () {
    const dropdown = document.getElementById('cartDropdown');
    if (!dropdown) return;

    const btn = dropdown.querySelector('.top-nav-dropdown-btn');

    btn.addEventListener('click', function (e) {
        e.stopPropagation();
        dropdown.classList.toggle('open');
    });

    document.addEventListener('click', function () {
        dropdown.classList.remove('open');
    });

    dropdown.querySelector('.top-nav-dropdown-menu').addEventListener('click', function (e) {
        e.stopPropagation();
    });
});
