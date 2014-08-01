//*****************************************************
// Author: Tim Braga - Velir
// Email: tim.braga@velir.com
// Date: 03/22/2012
// FieldSuite Images Field
//*****************************************************

if (FieldSuite == null) {
	FieldSuite = [];
}

if (FieldSuite.Fields == null) {
	FieldSuite.Fields = [];
}

if (FieldSuite.Html == null) {
	FieldSuite.Html = [];
}

FieldSuite.Fields.ImagesField = [];

var x = 0;
var _currentItemId;

FieldSuite.Fields.ImagesField.AddItemToContent = function (fieldId, html, itemId) {
	var imageItemsWrapper = $$('#' + fieldId + ' .velirImageItems');
	if (imageItemsWrapper == null) {
		console.log('Image Items Wrapper is null');
		return;
	}

	//hide no value text
	var noValueDiv = $$('#' + fieldId + ' .velirImagesFieldNoValue');
	if (noValueDiv != null && noValueDiv.length > 0) {
		noValueDiv[0].style.display = 'none';
	}

	//widen space
	var currentWidth = imageItemsWrapper[0].style.width.replace('px', '');
	var newWidth = (parseInt(currentWidth) + 185) + 'px';
	imageItemsWrapper[0].style.width = newWidth;

	//add html to field content
	imageItemsWrapper[0].innerHTML += FieldSuite.Html.HtmlDecode(html);

	//update hidden field with new item id
	var fieldValueElement = scForm.browser.getControl(fieldId + '_Value');
	if (fieldValueElement == null) {
		console.log('Field Value element is null: ' + fieldId + '_Value');
		return;
	}

	//check for braces
	if (itemId.indexOf("{") == -1) {
		itemId = '{' + itemId + '}';
	}

	var value = fieldValueElement.value;
	if (value == null || value == '') {
		fieldValueElement.value = itemId;
	}
	else {
		fieldValueElement.value += '|' + itemId;
	}
	return false;
}

FieldSuite.Fields.ImagesField.RemoveItem = function (fieldId) {
	var selectedValues = FieldSuite.Fields.ImagesField.GetSelectedItems(fieldId);
	if (selectedValues == null || selectedValues.length == 0) {
		alert('Please select an item');
		return;
	}

	var r=confirm("Are you sure you want to remove this item.");
	if (r==true)
	{
		//remove html
		selectedValues[0].remove();

		//reconcile list
		FieldSuite.Fields.ImagesField.ReconcileFieldValue(fieldId);
	}
}

FieldSuite.Fields.ImagesField.EditItem = function (fieldId) {
	var selectedValues = FieldSuite.Fields.ImagesField.GetSelectedItems(fieldId);
	if (selectedValues == null || selectedValues.length == 0) {
		alert('Select an item');
		return;
	}

	var selectedItemId = selectedValues[0].readAttribute('data_id');

	//launch edit form
	scForm.invoke('fieldsuite:edititem(id=' + selectedItemId + ',fieldId=' + fieldId + ')');
}

FieldSuite.Fields.ImagesField.GotoItem = function (fieldId) {
	var selectedItems = FieldSuite.Fields.ImagesField.GetSelectedItems(fieldId);
	if (selectedItems.length == 0) {
		alert('Please select an item');
		return;
	}

	if (selectedItems.length > 1) {
		alert('Please select a single item');
		return;
	}

	var selectedItem = selectedItems[0];
	var itemId = $(selectedItem).readAttribute('data_id');
	if (itemId == null || itemId == '') {
		console.log('FieldSuite.Fields.ImagesField.EditItem - No Item Id Found');
		return;
	}

	if (!itemId.indexOf('{') == 0) {
		itemId = '{' + itemId + '}';
	}

	//launch edit form
	scForm.invoke('item:load(id=' + itemId + ')');
}

//Adds a new item
FieldSuite.Fields.ImagesField.AddItem = function (fieldId, source) {

	//launch add form
	scForm.invoke('fieldsuite:imagesfield.additem(fieldid=' + fieldId + ', source=' + source + ')');
}

//Sort the selected item to the left
FieldSuite.Fields.ImagesField.SortItemLeft = function (fieldId) {
	var selectedItems = FieldSuite.Fields.ImagesField.GetSelectedItems(fieldId);
	if (selectedItems.length == 0) {
		alert('Please select an item');
		return;
	}

	if (selectedItems.length > 1) {
		alert('Please select a single item');
		return;
	}

	var currentElement = selectedItems[0];

	var previousElement = currentElement.previous();
	if (previousElement == null) {
		return;
	}

	previousElement.up().insertBefore(currentElement, previousElement);

	//reconcile list
	FieldSuite.Fields.ImagesField.ReconcileFieldValue(fieldId);
}

//Sort the selected item to the right
FieldSuite.Fields.ImagesField.SortItemRight = function (fieldId) {

	var selectedItems = FieldSuite.Fields.ImagesField.GetSelectedItems(fieldId);
	if (selectedItems.length == 0) {
		alert('Please select an item');
		return;
	}

	if (selectedItems.length > 1) {
		alert('Please select a single item');
		return;
	}

	var currentElement = selectedItems[0];

	var nextElement = currentElement.next();
	if (nextElement == null) {
		return;
	}

	var nextNextElement = nextElement.nextSibling;
	nextElement.up().insertBefore(currentElement, nextNextElement);

	//reconcile list
	FieldSuite.Fields.ImagesField.ReconcileFieldValue(fieldId);
}

FieldSuite.Fields.ImagesField.ReconcileFieldValue = function (fieldId) {

	//update field values
	var fieldValue = '';

	$$('#' + fieldId + ' .rotatingImageWrapper').each(function (item) {
		var itemId = $(item).readAttribute('data_id');

		if (fieldValue != '') {
			fieldValue += '|';
		}

		fieldValue += itemId;
	});

	FieldSuite.Fields.UpdateFieldValue(fieldId, fieldValue);
}

//Select item
FieldSuite.Fields.ImagesField.ToggleItem = function (sender, fieldId) {

	var item = $(sender).up(1);
	if (item == null) {
		return;
	}

	var hasClass = FieldSuite.Html.HasClass(item, "velirImageSelected");

	//remove from all selected items
	FieldSuite.Fields.ImagesField.GetSelectedItems(fieldId).each(function (item) {
		if (item != null) {
			//perform remove first
			FieldSuite.Html.RemoveClass(item, "velirImageSelected");
		}
	});

	//if the item did not have the class, add it
	if (!hasClass) {
		FieldSuite.Html.AddClass(item, "velirImageSelected");
	}
}

//Retrieves the selected items from the form
FieldSuite.Fields.ImagesField.GetSelectedItems = function (fieldId) {

	var selectedValues = new Array();
	var i = 0;
	$$('#' + fieldId + ' .velirImageSelected').each(function (item) {
		selectedValues[i] = item;
		i = i + 1;
	});

	return selectedValues;
}