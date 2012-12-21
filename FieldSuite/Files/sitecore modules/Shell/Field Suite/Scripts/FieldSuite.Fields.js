//*****************************************************
// Author: Tim Braga - Velir
// Email: tim.braga@velir.com
// Date: 03/22/2012
// FieldSuite Abstract Fields
//*****************************************************

FieldSuite = [];
FieldSuite.Fields = [];

// <summary>
// Selects all available items for the passed field
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.SelectAll = function (fieldId) {
	var selector = '#' + fieldId + ' .unSelectedItems div.velirItem';

	//iterate over all the anchors in 'unSelectedItems' and add this class 'selected'
	$$(selector).each(function (item) {
		var firstChild = item.down('.velirFieldSelected');
		if (!FieldSuite.Html.HasClass(firstChild, 'selected')) {
			FieldSuite.Html.AddClass(firstChild, 'selected');
		}
	});

	return false;
}

// <summary>
// Deselects all available items for the passed field
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.DeSelectAll = function (fieldId) {
	var selector = '#' + fieldId + ' .unSelectedItems div.selected';
	//iterate over all the anchors in 'unSelectedItems' and remove this class 'selected'
	FieldSuite.Fields.HighlightedAvailableItems(fieldId).each(function (item) {
		FieldSuite.Html.RemoveClass(item.down('.velirFieldSelected'), 'selected');
	});
}

// <summary>
// It will launch the Edit Modal for the selected item of the passed field.
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.EditItem = function (fieldId) {
	var selectedItems = FieldSuite.Fields.AllHighlightedItems(fieldId);
	if (selectedItems.length == 0) {
		alert('Please select an item');
		return;
	}

	if (selectedItems.length > 1) {
		alert('Please select a single item');
		return;
	}

	var selectedItem = selectedItems[0];
	var itemId = $(selectedItem).readAttribute('data_id')
	if (itemId == null || itemId == '') {
		console.log('FieldSuite.Fields.EditItem - No Item Id Found');
		return;
	}

	//launch edit form
	scForm.invoke('fieldsuite:edititem(id=' + itemId + ',fieldId=' + fieldId + ')');
}

// <summary>
// It will remove the selected item from the list.
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.RemoveItem = function (fieldId, itemId) {
    var highlightedItems = FieldSuite.Fields.HighlightedSelectedItems(fieldId);
    if (highlightedItems == null || highlightedItems.length == 0) {

        if (itemId) {
            var wasFound = false;
            $$('#' + fieldId + '_SelectedItems div.velirItem').each(function (item) {
                var id = $(item).readAttribute('data_id');
                if (id === itemId) {
                    wasFound = true;
                    item.remove();
                    throw $break;
                }
            });
        }

        if (!wasFound) {
            alert('Please select an item');
            return;
        }
    } else {
        //remove html
        selectedValues[0].remove();
    }

    //reconcile list
    FieldSuite.Fields.ReconcileFieldValue(fieldId);
    return false;
}

// <summary>
// It will change the Content Manager's selected item to the 
// selected item of the passed field.
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.GotoItem = function (fieldId) {
	var selectedItems = FieldSuite.Fields.AllHighlightedItems(fieldId);
	if (selectedItems.length == 0) {
		alert('Please select an item');
		return;
	}

	if (selectedItems.length > 1) {
		alert('Please select a single item');
		return;
	}

	var selectedItem = selectedItems[0];
	var itemId = $(selectedItem).readAttribute('data_id')
	if (itemId == null || itemId == '') {
		console.log('FieldSuite.Fields.EditItem - No Item Id Found');
		return;
	}

	//launch edit form
	scForm.invoke('item:load(id=' + itemId + ')');
}

// <summary>
// It will update the inner html of the field gutter
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.UpdateFieldGutter = function (fieldId, html) {

	var fieldGutterElement = scForm.browser.getControl(fieldId + '_fieldGutterDiv');
	if (fieldGutterElement == null) {
		return;
	}

	//if no html is sent back, set field gutter to nothing
	if (html == null || html == '') {
		fieldGutterElement.innerHTML = '';
		return false;
	}

	fieldGutterElement.innerHTML = FieldSuite.Html.HtmlDecode(html);
	return false;
}

// <summary>
// It will update the inner html of the items field gutter
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.UpdateItemFieldGutter = function (fieldId, html, itemId) {

	//get fieldgutter for this item
	var fieldGutterElement = scForm.browser.getControl(fieldId + '_' + itemId + '_fieldGutterDiv');
	if (fieldGutterElement == null) {
		return;
	}

	//if no html is sent back, set field gutter to nothing
	if (html == null || html == '') {
		fieldGutterElement.innerHTML = '';
		return false;
	}

	fieldGutterElement.innerHTML = FieldSuite.Html.HtmlDecode(html);
	return false;
}

// <summary>
// It will update the source of the template icon
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.UpdateTemplateIcon = function (fieldId, imageSrc) {

	if (imageSrc == null || imageSrc == '') {
		imageSrc = '/sitecore/images/blank.gif';
	}

	//find template image
	var imgElement = document.getElementById(fieldId + '_templateIconImg');
	if (imgElement == null) {
		console.log('Template Image Element is null: ' + fieldId + '_templateIconImg');
		return;
	}
	
	//reset template icon
	document.getElementById(fieldId + '_templateIconImg').src = imageSrc
}

// <summary>
// Open Item Comparer
// </summary>
// <param name="itemId">ID of the item to open</param>
FieldSuite.Fields.OpenItemComparer = function (itemId, fieldId) {
	if (itemId == null || itemId == '') {
		return;
	}

	//open item comparer
	scForm.invoke('fieldsuite:openitemcomparer(id=' + itemId + ',fieldId=' + fieldId + ')');
}

// <summary>
// Determines if an item should be available or selected and
// moves the element appropiately
// </summary>
// <param name="fieldId">ID of the Field</param>
// <param name="sender">The clicked element</param>
FieldSuite.Fields.ToggleItem = function (fieldId, sender) {

	//toggle between adding and removing the item
	var moveLeft = false;
	var arr = $(sender).ancestors();
	arr.each(function (node) {
		if (FieldSuite.Html.HasClass(node, 'selectedItems')) {
			moveLeft = true;
			$break;
		}
	});

	if (moveLeft) {
		FieldSuite.Fields.MoveItemLeft(fieldId, sender);
	}
	else {
		FieldSuite.Fields.MoveItemRight(fieldId, sender);
	}
}

// <summary>
// Moves the item to the available list, left
// </summary>
// <param name="fieldId">ID of the Field</param>
// <param name="sender">The clicked element</param>
FieldSuite.Fields.MoveItemLeft = function (fieldId, sender) {
	if (sender == null || fieldId == null || fieldId == '') {
		console.log('FieldSuite.Fields.MoveItemLeft - The sender or the field id is null');
		return;
	}

	var velirItem = null;

	//get item element
	var arr = $(sender).ancestors();
	arr.each(function (node) {
		if (FieldSuite.Html.HasClass(node, 'velirItem')) {
			velirItem = node;
			$break;
		}
	});

	if (velirItem == null) {
		console.log('FieldSuite.Fields.MoveItemLeft - VelirItem is null');
		return;
	}

	var unSelectedItemsElement = $$('#' + fieldId + ' .unSelectedItems');
	if (unSelectedItemsElement == null) {
		console.log('FieldSuite.Fields.MoveItemLeft - unSelectedItemsElement is null');
		return;
	}

	//move html to destination
	FieldSuite.Html.Move(velirItem, unSelectedItemsElement[0]);

	//remove selected class
	FieldSuite.Html.RemoveClass(velirItem.down('.velirFieldSelected'), 'selected');

	//also move any selected items
	var currentItemId = velirItem.readAttribute('data_id');
	var highlightedItems = FieldSuite.Fields.HighlightedSelectedItems(fieldId);
	var i = 0;
	while (i < highlightedItems.length) {
		var velirItem = highlightedItems[i];
		var selectedItem = velirItem.down('.velirFieldSelected');
		var itemId = $(velirItem).readAttribute('data_id');
		if (itemId != currentItemId) {
			//move html to destination and remove selected class
			FieldSuite.Html.Move(velirItem, unSelectedItemsElement[0]);
			FieldSuite.Html.RemoveClass(velirItem.down('.velirFieldSelected'), 'selected');
		}
		i++;
	}

	//reconcile list
	FieldSuite.Fields.ReconcileFieldValue(fieldId);
	return false;
}

// <summary>
// Moves the item to the selected list, right
// </summary>
// <param name="fieldId">ID of the Field</param>
// <param name="sender">The clicked element</param>
FieldSuite.Fields.MoveItemRight = function (fieldId, sender) {
	if (sender == null || fieldId == null || fieldId == '') {
		console.log('FieldSuite.Fields.MoveItemRight - The sender or the field id is null');
		return;
	}

	var velirItem = $(sender).up('.velirItem');
	if (velirItem == null) {
		console.log('FieldSuite.Fields.MoveItemRight - VelirItem is null');
		return;
	}

	var selectedItemsElement = $$('#' + fieldId + ' .selectedItems');
	if (selectedItemsElement == null) {
		console.log('FieldSuite.Fields.MoveItemRight - selectedItemsElement is null');
		return;
	}

	//move html to destination
	FieldSuite.Html.Move(velirItem, selectedItemsElement[0]);

	//remove selected class
	FieldSuite.Html.RemoveClass(velirItem.down('.velirFieldSelected'), 'selected');

	//also move any selected items
	var currentItemId = velirItem.readAttribute('data_id');
	var highlightedLeftItems = FieldSuite.Fields.HighlightedAvailableItems(fieldId)
	var i = 0;
	while (i < highlightedLeftItems.length) {
		var velirItem = highlightedLeftItems[i];
		var selectedItem = velirItem.down('.velirFieldSelected');

		var itemId = $(velirItem).readAttribute('data_id');
		if (itemId != currentItemId) {
			//move html to destination and remove selected class
			FieldSuite.Html.Move(velirItem, selectedItemsElement[0]);
			FieldSuite.Html.RemoveClass(velirItem.down('.velirFieldSelected'), 'selected');
		}
		i++;
	}

	//reconcile list
	FieldSuite.Fields.ReconcileFieldValue(fieldId);
	return false;
}

// <summary>
// Moves the selected item in the selected list up
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.MoveItemUp = function (fieldId) {
	var selectedItems = FieldSuite.Fields.HighlightedSelectedItems(fieldId);
	if (selectedItems.length == 0) {
		alert('Please select an item');
		return;
	}

	if (selectedItems.length > 1) {
		alert('Please select a single item');
		return;
	}

	var currentElement = $(selectedItems[0]);

	var previousElement = currentElement.previous();
	if (previousElement == null) {
		return;
	}

	previousElement.up().insertBefore(currentElement, previousElement);

	//reconcile list
	FieldSuite.Fields.ReconcileFieldValue(fieldId);
}

// <summary>
// Moves the selected item in the selected list down
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.MoveItemDown = function (fieldId) {
	var selectedItems = FieldSuite.Fields.HighlightedSelectedItems(fieldId);
	if (selectedItems.length == 0) {
		alert('Please select an item');
		return;
	}

	if (selectedItems.length > 1) {
		alert('Please select a single item');
		return;
	}

	var currentElement = $(selectedItems[0]);

	var nextElement = currentElement.next();
	if (nextElement == null) {
		return;
	}

	var nextNextElement = nextElement.nextSibling;
	nextElement.up().insertBefore(currentElement, nextNextElement);

	//reconcile list
	FieldSuite.Fields.ReconcileFieldValue(fieldId);
}

// <summary>
// Will rebuild the hidden value of the field based on the current items in the selected list
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.ReconcileFieldValue = function (fieldId) {

	var selectedItemsElement = $$('#' + fieldId + ' .selectedItems');
	if (selectedItemsElement == null) {
		console.log('FieldSuite.Fields.ReconcileFieldValue - selectedItemsElement is null');
		return;
	}

	//update field values
	var fieldValue = '';

	//get velir item element
	var selectedItems = $(selectedItemsElement[0]).descendants();

	selectedItems.each(function (selectedItem) {
		if (FieldSuite.Html.HasClass(selectedItem, 'velirItem')) {
			var itemId = $(selectedItem).readAttribute('data_id');
			if (itemId != null && itemId != '') {
				if (fieldValue != '') {
					fieldValue += '|';
				}

				fieldValue += itemId;
			}
		}
	});

	FieldSuite.Fields.UpdateFieldValue(fieldId, fieldValue);
}

// <summary>
// Updates the hidden value of the field
// </summary>
// <param name="fieldId">ID of the Field</param>
// <param name="value">New Field Value</param>
FieldSuite.Fields.UpdateFieldValue = function (fieldId, value) {
	var fieldValueElement = scForm.browser.getControl(fieldId + '_Value');
	if (fieldValueElement == null) {
		console.log('Field Value element is null: ' + fieldId + '_Value');
		return;
	}

	fieldValueElement.value = value;
}

// <summary>
// Will place an item in a selected state
// </summary>
// <param name="sender">Clicked Element</param>
FieldSuite.Fields.SelectItem = function (sender) {
	//check sender
	if (sender == null) {
		console.log('FieldSuite.Fields.SelectItem - Senders is null');
		return;
	}

	//check parent
	var parentElement = sender.parentNode;
	if (parentElement == null) {
		console.log('FieldSuite.Fields.SelectItem - Senders parent is null');
		return;
	}

	//toggle selected class
	if (FieldSuite.Html.HasClass(parentElement, 'selected')) {
		FieldSuite.Html.RemoveClass(parentElement, 'selected')
	}
	else{
		FieldSuite.Html.AddClass(parentElement, 'selected')
	}
}

// <summary>
// Will return a list of selected items from the selected list (right side)
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.HighlightedSelectedItems = function (fieldId) {

	var selectedValues = new Array();

	var i = 0;
	$$('#' + fieldId + ' .selectedItems div.velirFieldSelected.selected').each(function (item) {
		selectedValues[i] = item.parentNode;
		i = i + 1;
	});

	return selectedValues;
}

// <summary>
// Will return a list of selected items from the available list (left side)
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.HighlightedAvailableItems = function (fieldId) {

	var selectedValues = new Array();

	var i = 0;
	$$('#' + fieldId + ' .unSelectedItems div.velirFieldSelected.selected').each(function (item) {
		selectedValues[i] = item.parentNode;
		i = i + 1;
	});

	return selectedValues;
}

// <summary>
// Will return a list of selected items from both the available and selected lists
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.AllHighlightedItems = function (fieldId) {

	var selectedValues = new Array();

	var i = 0;
	$$('#' + fieldId + ' div.velirFieldSelected.selected').each(function (item) {
		selectedValues[i] = item.parentNode;
		i = i + 1;
	});

	return selectedValues;
}