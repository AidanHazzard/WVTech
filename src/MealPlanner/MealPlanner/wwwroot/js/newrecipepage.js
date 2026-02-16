document.addEventListener('DOMContentLoaded', function () {


    const addButton = document.querySelector('#buttonAppend');
    const container = document.querySelector('#AppendHere');


    function createInput() {
        //adds a wrapper that will wrap all of our stuff in it so the button can be inline and
        //so we can delete things later
        const inputWrapper = document.createElement('div');
        inputWrapper.className = 'input-wrapper';

        //adds the text feild
        const newInput = document.createElement('input');
        newInput.type = 'text';
        newInput.className = 'back2-textbox textStopAtButton';
        newInput.placeholder = 'Enter Ingredient';
        newInput.name = 'Ingredients';
        newInput.required = true;

        //adds the delete button
        const deleteButton = document.createElement('button');
        deleteButton.type = 'button';
        deleteButton.className = 'deleteButton';

        //adds the deelete image
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
        inputWrapper.appendChild(newInput);
        inputWrapper.appendChild(deleteButton);
        container.appendChild(inputWrapper);
    }

    //adds a new wrapper with the entrys in it
    addButton.addEventListener('click', function (e) {
        e.preventDefault();
        createInput();
    });
});
