//*****************************************************
// Author: Tim Braga - Velir
// Email: tim.braga@velir.com
// Date: 09/25/2012
// FieldSuite DropTree Field
//*****************************************************

if (FieldSuite == null) {
	FieldSuite = [];
}

if (FieldSuite.Fields == null) {
	FieldSuite.Fields = [];
}

if (FieldSuite.Fields.DropTree == null) {
	FieldSuite.Fields.DropTree = [];
}

if (FieldSuite.Html == null) {
	FieldSuite.Html = [];
}

FieldSuite.Fields.DropTree.UpdateValue = function (fieldId, itemId) {
	var selectedItem = FieldSuite.Fields.DropTree.SelectedItem(fieldId);
	if (selectedItem == null) {
		alert('Please select an item');
		return;
	}

	var itemPath = $(selectedItem).readAttribute('value');
	if (itemPath == null || itemPath == '') {
		alert('Please select an item');
		return;
	}

	//launch edit form
	scForm.invoke('fieldsuite:edititem(id=' + itemPath + ',fieldId=' + fieldId + ')');
}

FieldSuite.Fields.DropTree.EditItem = function (fieldId) {
	var selectedItem = FieldSuite.Fields.DropTree.SelectedItem(fieldId);
	if (selectedItem == null) {
		alert('Please select an item');
		return;
	}

	var itemId = $(selectedItem).readAttribute('value');
	if (itemId == null || itemId == '') {
		alert('Please select an item');
		return;
	}

	//launch edit form
	scForm.invoke('fieldsuite:edititem(id=' + itemId + ',fieldId=' + fieldId + ')');
}

FieldSuite.Fields.DropTree.GotoItem = function (fieldId) {
	var selectedItem = FieldSuite.Fields.DropTree.SelectedItem(fieldId);
	if (selectedItem == null) {
		alert('Please select an item');
		return;
	}

	var itemId = $(selectedItem).readAttribute('value');
	if (itemId == null || itemId == '') {
		alert('Please select an item');
		return;
	}

	//launch edit form
	scForm.invoke('item:load(id=' + itemId + ')');
}

FieldSuite.Fields.DropTree.SelectedItem = function (fieldId) {
	var fieldValueElement = scForm.browser.getControl(fieldId + '_Value');
	if (fieldValueElement == null) {
		console.log('Field Value element is null: ' + fieldId + '_Value');
		return;
	}

	return fieldValueElement;
}