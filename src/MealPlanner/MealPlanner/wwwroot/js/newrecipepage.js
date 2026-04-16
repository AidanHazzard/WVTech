let numIngredients = 0;

// TODO: Change to JQuery
document.addEventListener('DOMContentLoaded', function () {
    const addButton = document.querySelector('#buttonAppend');
    const container = document.querySelector('#AppendHere');

    //adds a new wrapper with the entrys in it
    addButton.addEventListener('click', function (e) {
        e.preventDefault();
        createInput();
    });

    //delete button
    container.addEventListener('click', function (e) {
        const deleteButton = e.target.closest('.deleteButton');
        if (deleteButton) {
            deleteButton.closest('.input-wrapper').remove();
        }
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
            </div>
            `

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

    //sets up the hierarchy
    inputWrapper.appendChild(deleteButton);
    container.appendChild(inputWrapper);
}

// --- Custom tag management ---
function addCustomTag(tagName) {
    const customTagsContainer = document.getElementById('custom-tags-container');
    if (!tagName || !tagName.trim()) return;

    const trimmed = tagName.trim();

    // Prevent duplicates (case-insensitive) across checkboxes and hidden inputs
    const allTagInputs = document.querySelectorAll('input[name="Tags"]');
    for (const inp of allTagInputs) {
        if (inp.value.toLowerCase() === trimmed.toLowerCase()) return;
    }

    const wrapper = document.createElement('div');
    wrapper.className = 'custom-tag-badge d-inline-flex align-items-center me-2 mb-2';
    wrapper.innerHTML = `
        <span class="badge me-1">${trimmed}</span>
        <input type="hidden" name="Tags" value="${trimmed}" />
        <button type="button" class="remove-custom-tag">x</button>
    `;
    wrapper.querySelector('.remove-custom-tag')
        .addEventListener('click', function () { wrapper.remove(); });
    customTagsContainer.appendChild(wrapper);
}

document.addEventListener('DOMContentLoaded', function () {
    const customTagInput = document.getElementById('custom-tag-input');
    const addTagBtn = document.getElementById('add-custom-tag-btn');

    if (addTagBtn) {
        addTagBtn.addEventListener('click', function () {
            addCustomTag(customTagInput.value);
            customTagInput.value = '';
        });
    }

    // Wire remove buttons for custom tags pre-rendered by EditRecipe view
    document.querySelectorAll('.remove-custom-tag').forEach(function (btn) {
        btn.addEventListener('click', function () { btn.closest('.custom-tag-badge').remove(); });
    });
});