window.willWorkForCache = window.willWorkForCache || {};

window.willWorkForCache.setFieldValue = function (fieldId, newValue, alertIfSame) {
    // Get the input field and change the value using jQuery since it's built in
    var field = jQuery("#" + fieldId);

    // Scroll the whole 'row' of the table that the input is in into view
    field.closest("table")[0].scrollIntoView({ behavior: 'smooth' })

    if (field.val() == newValue) {
        if (alertIfSame == true) {
            alert("The value has not changed.");
        }
    }
    else {
        // Set the value of the field 
        field.val(newValue);
    }

    //field.focus(); field.blur();

    // Trigger the content editor to re-run validators to pick up this has/has not changed,
    // and flagging if it should be saved
    scContent.startValidators();
}

window.willWorkForCache.setFieldValueByName = function (fieldTitle, newValue, alertIfSame) {
    // The markup we are looking for contains an input with an ID like FIELDxxxxxx:
    //
    // <td class="scEditorFieldMarkerInputCell">
    //	  <div class="scEditorFieldLabel">Alt:</div>
    //	  <div style="">
    //		<input id="FIELD35937106" class="scContentControl" .....
    //    </div>
    //  </td>
    //
    // So first, identify the scEditorFieldLabel based on the text inside it 

    var matchingFieldLabels = jQuery('div.scEditorFieldLabel:contains("' + fieldTitle + '")').filter(function () {

        $title = jQuery(this);

        var nextChars = $title.html().substring(0, fieldTitle.length + 2);

        // Allow "TITLE:" - no help text, and no shared,etc tag
        if (nextChars == fieldTitle + ':')
            return true;

        // Allow "TITLE [shared..." - the [shared] is in a <span>
        if (nextChars == fieldTitle + "<s")
            return true;

        // Allow "TITLE - help text..." - the help text is after a space, and then a dash
        if (nextChars == fieldTitle + ' -')  // Title - Help text
            return true;

        return false;
    });

    // We must have exactly one match, otherwise there are too many / too few
    if (matchingFieldLabels.length != 1) {
        console.log("Unable to find a matching field label for title: " + fieldTitle, matchingFieldLabels);

        alert("Sorry, can't find the '" + fieldTitle + "' field to update.");
        return null;
    }

    // Now if we have one match, find the input field (the next element's child)
    var fieldId = $title.next().children('input,textarea').attr("id");
    console.log("Found field ID for title '" + fieldTitle + "' as: " + fieldId);

    window.willWorkForCache.setFieldValue(fieldId, newValue, alertIfSame);
}