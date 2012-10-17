//*****************************************************
// Author: Tim Braga - Velir
// Email: tim.braga@velir.com
// Date: 05/29/2012
// FieldSuite Droplist Field
//*****************************************************

if (FieldSuite == null) {
	FieldSuite = [];
}

if (FieldSuite.Fields == null) {
	FieldSuite.Fields = [];
}

if (FieldSuite.Fields.Treelist == null) {
	FieldSuite.Fields.Treelist = [];
}

if (FieldSuite.Html == null) {
	FieldSuite.Html = [];
}

FieldSuite.Fields.Treelist.SelectedItem = function (fieldId) {
	var selectedItems = $$('#' + fieldId + ' .scContentTreeNodeActive');
	if (selectedItems == null || selectedItems.length == 0) {
		return null;
	}

	return selectedItems[0];
}

FieldSuite.Fields.Treelist.AddItemToContent = function (fieldId, html) {

	var selectedItemsElement = $$('#' + fieldId + ' .selectedItems');
	if (selectedItemsElement == null) {
		console.log('FieldSuite.Fields.Treelist.AddItemToContent - selectedItemsElement is null');
		return;
	}

	//move html to destination
	selectedItemsElement[0].innerHTML += FieldSuite.Html.HtmlDecode(html);

	//reconcile list
	FieldSuite.Fields.ReconcileFieldValue(fieldId);
	return false;
}

FieldSuite.Fields.Treelist.AddItem = function (fieldId) {

	//get selected item
	var selectedItem = FieldSuite.Fields.Treelist.SelectedItem(fieldId);
	if (selectedItem == null) {
		alert('Please select an item');
		return;
	}

	var itemId = $(selectedItem).readAttribute('data_id');
	if (itemId == null || itemId == '') {
		//this is a node that was fetched from a webservice
		var id = $(selectedItem).up('.scContentTreeNode').readAttribute('id');

		var n = id.indexOf("_all_");
		if (n == -1) {
			return;
		}

		//format correctly
		itemId = id.substring(n + 5)
		itemId = itemId.substring(0, 8) + '-' + itemId.substring(8, 12) + '-' + itemId.substring(12, 16) + '-' + itemId.substring(16, 20) + '-' + itemId.substring(20);
		itemId = '{' + itemId + '}';
	}

	var excludedTemplates = '';
	var excludedElement = $(fieldId + '_all_ExcludedTemplatesForSelection');
	if (excludedElement != null && excludedElement.value != null) {
		excludedTemplates = excludedElement.value;
	}

	var includedTemplates = '';
	var includedElement = $(fieldId + '_all_IncludedTemplatesForSelection');
	if (includedElement != null && includedElement.value != null) {
		includedTemplates = includedElement.value;
	}

	//launch serverside command
	scForm.invoke('fieldsuite:treelist.additem(fieldid=' + fieldId + ', itemid=' + itemId + ', excludedTemplates=' + excludedTemplates + ', includedTemplates=' + includedTemplates + ')');
	return false;
}

FieldSuite.Fields.Treelist.RemoveItem = function (fieldId, sender) {
	//get wrapping list item div from sender
	var selectedItem = $(sender).up('.velirItem');
	if (selectedItem == null) {
		console.log('Selected Item is null');
		return;
	}

	//remove html
	selectedItem.remove();

	//reconcile list
	FieldSuite.Fields.ReconcileFieldValue(fieldId);
	return false;
}