//*****************************************************
// Author: Tim Braga - Velir
// Email: tim.braga@velir.com
// Date: 12/20/2012
// FieldSuite General Links Field
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

FieldSuite.Fields.GeneralLinks = [];

// <summary>
// Removes the item
// </summary>
// <param name="sender">Anchor passed (sender)</param>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.GeneralLinks.RemoveItem = function (sender, fieldId) {

	//check parent
	var parentElement = sender.parentNode;
	if (parentElement == null) {
		console.log('FieldSuite.Fields.GeneralLinks - Senders parent is null');
		return;
	}

	//check parent
	parentElement = parentElement.parentNode;
	if (parentElement == null) {
		console.log('FieldSuite.Fields.GeneralLinks - Senders parent is null');
		return;
	}

	if (FieldSuite.Html.HasClass(parentElement, 'velirItem')) {
		var r = confirm("Are you sure you want to remove this item.");
		if (r == true) {
			//remove html
			var itemId = $(parentElement).readAttribute('data_id');
			var newFieldValue = '';
			var linkItems = FieldSuite.Fields.GeneralLinks.GetLinkNodes(fieldId);

			//iterate over link items
			for (var i = 0; i < linkItems.length; i++) {
				var idAttribute = "linkid=\"" + itemId + "\"";
				if (linkItems[i].indexOf(idAttribute) == -1) {
					newFieldValue += linkItems[i].toString();
				}
			}

			parentElement.remove();
			FieldSuite.Fields.UpdateFieldValue(fieldId, newFieldValue);
		}
	}
}

// <summary>
// Calls the proper sitecore command for inserting and editing links
// </summary>
// <param name="sender">Anchor passed (sender)</param>
// <param name="evt">Click event passed from anchor</param>
// <param name="commandName">Sitecore command to call</param>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.GeneralLinks.InsertEdit = function (sender, evt, commandName, fieldId) {

	var selectedLink = FieldSuite.Fields.GetSelectedLinkNode(fieldId);
	if (selectedLink == null || selectedLink == '') {
		return scForm.postEvent(this, evt, commandName + '(id=' + fieldId + ')')
	}

	return scForm.postEvent(sender, evt, commandName + '(id=' + fieldId + ', link=' + selectedLink + ')')
}

// <summary>
// Sort the selected item up
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.GeneralLinks.SortItemUp = function (fieldId) {
	var selectedItems = FieldSuite.Fields.AllHighlightedItems(fieldId);
	if (selectedItems.length == 0) {
		alert('Please select an item');
		return;
	}

	if (selectedItems.length > 1) {
		alert('Please select a single item');
		return;
	}

	//selected html element
	var currentElement = selectedItems[0];
	var itemId = $(currentElement).readAttribute('data_id');

	var previousElement = currentElement.previous();
	if (previousElement == null) {
		return;
	}

	previousElement.up().insertBefore(currentElement, previousElement);

	//reconcile xml nodes in value field
	var nodes = FieldSuite.Fields.GeneralLinks.GetLinkNodes(fieldId);
	var selectedIndex = 0;
	var moveToIndex = 0;
	var idAttribute = "linkid=\"" + itemId + "\"";

	for (var i = 0; i < nodes.length; i++) {

		var node = nodes[i];

		if (node.indexOf(idAttribute) !== -1) {
			selectedIndex = i;
			moveToIndex = i - 1;
		}
	}

	if (moveToIndex == 0 && selectedIndex == 0) {
		return;
	}

	//move xml nodes
	nodes = nodes.move(selectedIndex, moveToIndex);

	//set to string
	var newFieldValue = nodes.join('');

	//update field value
	FieldSuite.Fields.UpdateFieldValue(fieldId, newFieldValue);
}

// <summary>
// Sort the selected item down
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.GeneralLinks.SortItemDown = function (fieldId) {

	var selectedItems = FieldSuite.Fields.AllHighlightedItems(fieldId);
	if (selectedItems.length == 0) {
		alert('Please select an item');
		return;
	}

	if (selectedItems.length > 1) {
		alert('Please select a single item');
		return;
	}

	//selected html element
	var currentElement = selectedItems[0];
	var itemId = $(currentElement).readAttribute('data_id');

	//selected xml node in value field
	var selectedNode = FieldSuite.Fields.GetSelectedLinkNode(fieldId);
	if (selectedNode == null) {
		return;
	}

	//get next element in preparation for sort move
	var nextElement = currentElement.next();
	if (nextElement == null) {
		return;
	}

	//move html elements
	var nextNextElement = nextElement.nextSibling;
	nextElement.up().insertBefore(currentElement, nextNextElement);

	//reconcile xml nodes in value field
	var nodes = FieldSuite.Fields.GeneralLinks.GetLinkNodes(fieldId);
	var selectedIndex = 0;
	var moveToIndex = 0;
	var idAttribute = "linkid=\"" + itemId + "\"";

	for (var i = 0; i < nodes.length; i++) {

		var node = nodes[i];

		if (node.indexOf(idAttribute) !== -1) {
			selectedIndex = i;
			moveToIndex = i + 1;
		}
	}

	if (moveToIndex == 0 && selectedIndex == 0) {
		return;
	}

	//move xml nodes
	nodes = nodes.move(selectedIndex, moveToIndex);

	//set to string
	var newFieldValue = nodes.join('');
	
	//update field value
	FieldSuite.Fields.UpdateFieldValue(fieldId, newFieldValue);
}

// <summary>
// Return the xmls node of the hidden field value as an array
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.GeneralLinks.GetLinkNodes = function (fieldId) {
	var fieldValue = FieldSuite.Fields.GetFieldValue(fieldId);
	var linkItems = new Array();
	var linkItemsCount = 0;

	for (var i = 0; i < fieldValue.length; i++) {

		var c = fieldValue.charAt(i);

		var currentValue = '';
		if (linkItems[linkItemsCount] != null) {
			currentValue = linkItems[linkItemsCount];
		}

		linkItems[linkItemsCount] = currentValue + c;

		if (c == '>') {
			linkItemsCount++;
		}
	}

	return linkItems;
}

// <summary>
// Return the xml node of the selected item from the hidden field value
// </summary>
// <param name="fieldId">ID of the Field</param>
FieldSuite.Fields.GetSelectedLinkNode = function (fieldId) {

	//get the selected <link> within the values
	var selectedItems = FieldSuite.Fields.AllHighlightedItems(fieldId);
	if (selectedItems.length == 0) {
		return;
	}

	if (selectedItems.length > 1) {
		return;
	}

	var selectedItem = selectedItems[0];
	var itemId = $(selectedItem).readAttribute('data_id')
	var linkItems = FieldSuite.Fields.GeneralLinks.GetLinkNodes(fieldId);

	if (linkItems == null || linkItems.length == 0) {
		return '';
	}

	//iterate over link items
	for (var i = 0; i < linkItems.length; i++) {
		var urlAttribute = "linkid=\"" + itemId + "\"";
		if (linkItems[i].indexOf(urlAttribute) != -1) {
			return linkItems[i];
		}
	}

	return '';
}