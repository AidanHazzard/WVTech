document.addEventListener('DOMContentLoaded', function () {
    const leftArrow = document.getElementById('ALeft');
    const rightArrow = document.getElementById('ARight');
    const container = document.getElementById('back1DatesInnerContainer');

    const scrollSpeed = 5;
    let scrollInterval;

    function startScroll(direction) {
        scrollInterval = setInterval(() => {
            container.scrollBy({ left: direction * scrollSpeed, behavior: 'auto' });
        }, 10);
    }

    function stopScroll() {
        clearInterval(scrollInterval);
    }

    //Left arrow events
    leftArrow.addEventListener('mousedown', () => startScroll(-1));
    leftArrow.addEventListener('touchstart', () => startScroll(-1));
    leftArrow.addEventListener('mouseup', stopScroll);
    leftArrow.addEventListener('mouseleave', stopScroll);
    leftArrow.addEventListener('touchend', stopScroll);

    //Right arrow events
    rightArrow.addEventListener('mousedown', () => startScroll(1));
    rightArrow.addEventListener('touchstart', () => startScroll(1));
    rightArrow.addEventListener('mouseup', stopScroll);
    rightArrow.addEventListener('mouseleave', stopScroll);
    rightArrow.addEventListener('touchend', stopScroll);
});