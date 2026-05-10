// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function () {
    const hamburgerBtn = document.getElementById('hamburgerBtn');
    const navbar = document.getElementById('mainNavbar');

    if (hamburgerBtn && navbar) {
        hamburgerBtn.addEventListener('click', function () {
            const isOpen = navbar.classList.toggle('nav-open');
            hamburgerBtn.setAttribute('aria-expanded', isOpen.toString());

            // Reset any open sub-dropdowns when the hamburger closes
            if (!isOpen) {
                navbar.querySelectorAll('.nav-dropdown.dropdown-open').forEach(function (d) {
                    d.classList.remove('dropdown-open');
                });
            }
        });
    }

    // Mobile sub-dropdown toggle (hover doesn't work on touch)
    document.querySelectorAll('.nav-dropdown').forEach(function (dropdown) {
        const btn = dropdown.querySelector('.nav-button');
        if (btn) {
            btn.addEventListener('click', function () {
                if (window.innerWidth <= 900) {
                    dropdown.classList.toggle('dropdown-open');
                }
            });
        }
    });
});
