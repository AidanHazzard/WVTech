// Selection highlight on click
document.querySelectorAll('.sm-card').forEach(function (btn) {
    btn.addEventListener('click', function () {
        document.querySelectorAll('.sm-card').forEach(b => b.classList.remove('sm-card--selected'));
        btn.classList.add('sm-card--selected');
    });
});

// Mobile tap: first tap reveals macro overlay; second tap submits
var isTouchOnly = !window.matchMedia('(hover: hover)').matches;
if (isTouchOnly) {
    document.querySelectorAll('.sm-card--has-meal').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            var overlay = btn.querySelector('.sm-macro-overlay');
            if (!overlay) return;
            if (!overlay.classList.contains('sm-macro-overlay--visible')) {
                e.preventDefault();
                document.querySelectorAll('.sm-macro-overlay--visible').forEach(function (o) {
                    o.classList.remove('sm-macro-overlay--visible');
                });
                overlay.classList.add('sm-macro-overlay--visible');
            }
        });
    });

    // Dismiss overlay when tapping outside a card
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.sm-card--has-meal')) {
            document.querySelectorAll('.sm-macro-overlay--visible').forEach(function (o) {
                o.classList.remove('sm-macro-overlay--visible');
            });
        }
    });
}
