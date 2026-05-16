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
                <input type="text" class="col-1 back2-textbox-partial mx-1" name="IngredientAmounts" placeholder="e.g. 1/2">
                <select class="col-2 back2-textbox-partial mx-1" name="IngredientMeasurements">
                    <option selected>Select</option>
                    <option value="Count">Count</option>
                    <option value="Teaspoon">tsp</option>
                    <option value="Tablespoon">tbsp</option>
                    <option value="Fluid Ounce">fl oz</option>
                    <option value="Cup">cup</option>
                    <option value="Pint">pt</option>
                    <option value="Quart">qt</option>
                    <option value="Gallon">gal</option>
                    <option value="Milliliter">mL</option>
                    <option value="Liter">L</option>
                    <option value="Ounce">oz</option>
                    <option value="Pound">lb</option>
                    <option value="Gram">g</option>
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

// --- Image preview ---
document.addEventListener('DOMContentLoaded', function () {
    const fileInput = document.getElementById('imageFile');
    const preview = document.getElementById('imagePreview');
    const currentImage = document.querySelector('img[alt="Current recipe image"]');
    const removeHidden = document.getElementById('removeImageHidden');
    const removeBtn = document.getElementById('removeImageBtn');
    const fileNameDisplay = document.querySelector('.file-name-display');

    if (fileInput && preview) {
        fileInput.addEventListener('change', function () {
            if (fileInput.files && fileInput.files[0]) {
                if (fileNameDisplay) fileNameDisplay.textContent = fileInput.files[0].name;
                preview.src = URL.createObjectURL(fileInput.files[0]);
                preview.style.display = 'block';
                if (currentImage) currentImage.style.display = 'none';
                if (removeHidden) removeHidden.value = 'false';
                if (removeBtn) removeBtn.style.display = '';
            } else {
                if (fileNameDisplay) fileNameDisplay.textContent = 'No file chosen';
                preview.style.display = 'none';
                preview.src = '';
                if (currentImage) {
                    if (!removeHidden || removeHidden.value !== 'true') {
                        currentImage.style.display = 'block';
                        if (removeBtn) removeBtn.style.display = '';
                    }
                } else {
                    if (removeBtn) removeBtn.style.display = 'none';
                }
            }
        });
    }

    if (removeBtn) {
        removeBtn.addEventListener('click', function () {
            if (fileInput) fileInput.value = '';
            if (fileNameDisplay) fileNameDisplay.textContent = 'No file chosen';
            if (preview) { preview.style.display = 'none'; preview.src = ''; }

            if (currentImage) {
                currentImage.style.display = 'none';
                if (removeHidden) removeHidden.value = 'true';
            }
            removeBtn.style.display = 'none';
        });
    }
});

// --- Tag management ---
function addTag(tagName, fromSelect) {
    const container = document.getElementById('tags-container');
    const select = document.getElementById('tag-select');
    if (!tagName || !tagName.trim()) return;

    const trimmed = tagName.trim();

    // Prevent duplicates (case-insensitive)
    for (const inp of document.querySelectorAll('input[name="Tags"]')) {
        if (inp.value.toLowerCase() === trimmed.toLowerCase()) {
            if (fromSelect) select.value = '';
            return;
        }
    }

    const wrapper = document.createElement('span');
    wrapper.className = 'tag-wrapper';

    const hiddenInput = document.createElement('input');
    hiddenInput.type = 'hidden';
    hiddenInput.name = 'Tags';
    hiddenInput.value = trimmed;

    const pill = document.createElement('button');
    pill.type = 'button';
    pill.className = 'tag-pill badge rounded-pill recipe-tag recipe-tag-removable';
    pill.title = `Remove ${trimmed}`;
    pill.textContent = trimmed;
    if (fromSelect) pill.dataset.predefined = 'true';

    pill.addEventListener('click', function () {
        // If this was a predefined tag, put its option back in the select
        if (pill.dataset.predefined === 'true' && select) {
            const opt = document.createElement('option');
            opt.value = trimmed;
            opt.textContent = trimmed;
            const opts = Array.from(select.options).slice(1); // skip placeholder
            const insertBefore = opts.find(o => o.value.toLowerCase() > trimmed.toLowerCase());
            if (insertBefore) select.insertBefore(opt, insertBefore);
            else select.appendChild(opt);
        }
        wrapper.remove();
    });

    wrapper.appendChild(pill);
    wrapper.appendChild(hiddenInput);
    container.appendChild(wrapper);

    if (fromSelect) {
        // Remove the chosen option from the dropdown
        const chosen = Array.from(select.options).find(o => o.value === trimmed);
        if (chosen) chosen.remove();
        select.value = '';
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const select = document.getElementById('tag-select');
    const customTagInput = document.getElementById('custom-tag-input');
    const addTagBtn = document.getElementById('add-custom-tag-btn');

    if (select) {
        select.addEventListener('change', function () {
            if (select.value) addTag(select.value, true);
        });
    }

    if (addTagBtn) {
        addTagBtn.addEventListener('click', function () {
            addTag(customTagInput.value, false);
            customTagInput.value = '';
        });
    }

    // Wire remove handler for tags pre-rendered by EditRecipe view
    document.querySelectorAll('.tag-wrapper').forEach(function (wrapper) {
        const pill = wrapper.querySelector('.tag-pill');
        const hiddenInput = wrapper.querySelector('input[name="Tags"]');
        if (!pill || !hiddenInput) return;
        const tagName = hiddenInput.value;
        pill.addEventListener('click', function () {
            if (pill.dataset.predefined === 'true' && select) {
                const opt = document.createElement('option');
                opt.value = tagName;
                opt.textContent = tagName;
                const opts = Array.from(select.options).slice(1);
                const insertBefore = opts.find(o => o.value.toLowerCase() > tagName.toLowerCase());
                if (insertBefore) select.insertBefore(opt, insertBefore);
                else select.appendChild(opt);
            }
            wrapper.remove();
        });
    });
});