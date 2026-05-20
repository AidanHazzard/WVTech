// Selection highlight on click
document.querySelectorAll('.sm-card').forEach(function (btn) {
    btn.addEventListener('click', function () {
        document.querySelectorAll('.sm-card').forEach(b => b.classList.remove('sm-card--selected'));
        btn.classList.add('sm-card--selected');
    });
});

// Mobile tap: first tap reveals X button (and macro overlay); second tap on card submits
var isTouchOnly = !window.matchMedia('(hover: hover)').matches;
if (isTouchOnly) {
    document.querySelectorAll('.sm-card-slot').forEach(function (slot) {
        var card = slot.querySelector('.sm-card');
        if (!card) return;

        card.addEventListener('click', function (e) {
            if (e.target.closest('.sm-delete-btn')) return;

            if (!slot.classList.contains('controls-visible')) {
                e.preventDefault();
                document.querySelectorAll('.sm-card-slot.controls-visible').forEach(function (s) {
                    s.classList.remove('controls-visible');
                    var o = s.querySelector('.sm-macro-overlay');
                    if (o) o.classList.remove('sm-macro-overlay--visible');
                });
                slot.classList.add('controls-visible');
                var overlay = card.querySelector('.sm-macro-overlay');
                if (overlay) overlay.classList.add('sm-macro-overlay--visible');
            }
        });
    });

    // Dismiss controls when tapping outside a slot
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.sm-card-slot')) {
            document.querySelectorAll('.sm-card-slot.controls-visible').forEach(function (s) {
                s.classList.remove('controls-visible');
                var o = s.querySelector('.sm-macro-overlay');
                if (o) o.classList.remove('sm-macro-overlay--visible');
            });
        }
    });
}
