document.addEventListener('DOMContentLoaded', function () {
    const leftArrow = document.getElementById('ALeft');
    const rightArrow = document.getElementById('ARight');
    const container = document.getElementById('back1DatesInnerContainer');

    const dateText = document.getElementById('modelsDate').textContent;
    const curDate = new Date(dateText);

    for (i = 400; i >= 1; i--) {
        let nextDate = new Date(curDate);
        nextDate.setDate(curDate.getDate() - i);
        MakeDate(nextDate);
        if (i === 4) {
            tail = nextDate;
        }
    }

    MakeSelected();

    for (i = 1; i < 400; i++) {
        let nextDate = new Date(curDate);
        nextDate.setDate(curDate.getDate() + i);
        MakeDate(nextDate);
    }

    document.getElementById('Selected').scrollIntoView({ behavior: 'instant', block: 'nearest', inline: 'center' });

    function MakeDate(nextDate) {
        nextDate = new Date(nextDate);
        const shortMonth = nextDate.toLocaleString('en-US', { month: 'short' });

        const newDate = document.createElement('form');
        newDate.action = '/Home/Index';
        newDate.method = 'get';

        const dateButton = document.createElement('button');
        dateButton.className = 'Date';
        dateButton.name = 'date';
        dateButton.value = nextDate.toISOString();

        const buttonsText = document.createElement('h2');
        buttonsText.className = 'back1Title';
        buttonsText.innerHTML = `${shortMonth}<br />${nextDate.getDate().toString()}`;

        dateButton.append(buttonsText);
        newDate.append(dateButton);
        container.append(newDate);
    }

    function MakeSelected() {
        const shortMonth = curDate.toLocaleString('en-US', { month: 'short' });

        const newDate = document.createElement('form');
        newDate.action = '/Home/Index';
        newDate.method = 'get';

        const dateDiv = document.createElement('div');
        dateDiv.id = 'Selected';

        const divText = document.createElement('h2');
        divText.className = 'back1Title';
        divText.innerHTML = `${shortMonth}<br />${curDate.getDate().toString()}`;

        dateDiv.append(divText);
        newDate.append(dateDiv);
        container.append(newDate);
    }

    let scrollInterval;
    
    function startScroll(direction) {
        scrollInterval = setInterval(() => {
            container.scrollBy({ left: direction * 10, behavior: 'auto' });
        }, 5);
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