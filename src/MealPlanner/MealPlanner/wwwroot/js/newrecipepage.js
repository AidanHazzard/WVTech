let numIngredients = 0;

// TODO: Change to JQuery
document.addEventListener('DOMContentLoaded', function () {
    const addButton = document.querySelector('#buttonAppend');

    //adds a new wrapper with the entrys in it
    addButton.addEventListener('click', function (e) {
        e.preventDefault();
        createInput();
    });
});

function createInput() {
    const container = document.querySelector('#AppendHere');
    //adds a wrapper that will wrap all of our stuff in it so the button can be inline and
    //so we can delete things later
    const inputWrapper = document.createElement('div');
    inputWrapper.className = 'row input-wrapper';
    inputWrapper.innerHTML = `                
            <div class="row">
                <input type="number" class="col-1 back2-textbox-partial mx-1" name="IngredientAmounts" placeholder="0">
                <select class="col-2 back2-textbox-partial mx-1" name="IngredientMeasurements">
                    <option selected>Select</option>
                    <option value="Count">Count</option>
                    <option value="Cup(s)">Cup(s)</option>
                    <option value="Ounce(s)">Ounce(s)</option>
                    <option value="Pound(s)">Pounds</option>
                    <option value="L">L</option>
                    <option value="KG">KG</option>
                </select>
                <input type="text" class="col back2-textbox-partial mx-1" placeholder="Enter Ingredient" name="Ingredients" required>
            </div>`

    //adds the delete button
    const deleteButton = document.createElement('button');
    deleteButton.type = 'button';
    deleteButton.className = 'deleteButton';

    //adds the delete image
    const deleteImg = document.createElement('img');
    deleteImg.src = '/images/icons/delete.png';
    deleteImg.alt = 'delete';
    deleteImg.className = 'deleteImage';
    deleteButton.appendChild(deleteImg);

    //when you click the delete button it deletes its wrapper
    deleteButton.addEventListener('click', function () {
        container.removeChild(inputWrapper);
    });

    //sets up the hierarchy
    inputWrapper.appendChild(deleteButton);
    container.appendChild(inputWrapper);
}