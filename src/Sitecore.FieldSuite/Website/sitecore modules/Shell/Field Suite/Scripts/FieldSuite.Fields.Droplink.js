//*****************************************************
// Author: Tim Braga - Velir
// Email: tim.braga@velir.com
// Date: 03/22/2012
// FieldSuite Droplink Field
//*****************************************************

if (FieldSuite == null) {
	FieldSuite = [];
}

if (FieldSuite.Fields == null) {
	FieldSuite.Fields = [];
}

if (FieldSuite.Fields.Droplink == null) {
	FieldSuite.Fields.Droplink = [];
}

if (FieldSuite.Html == null) {
	FieldSuite.Html = [];
}

FieldSuite.Fields.Droplink.Select = function (fieldId) {
	var selectedItem = FieldSuite.Fields.Droplink.SelectedItem(fieldId);
	if (selectedItem == null) {
		return;
	}

	var itemId = $(selectedItem).readAttribute('data_id');
	if (itemId == null || itemId == '') {
		itemId = '';
	}

	//update field value
	FieldSuite.Fields.UpdateFieldValue(fieldId, itemId);

	//launch template icon request
	scForm.invoke('fieldsuite:templateIconUpdate(fieldid=' + fieldId + ',id=' + itemId + ')');

	//launch field gutter request
	scForm.invoke('fieldsuite:fieldGutterUpdate(fieldid=' + fieldId + ',id=' + itemId + ')');
}

FieldSuite.Fields.Droplink.EditItem = function (fieldId) {
	var selectedItem = FieldSuite.Fields.Droplink.SelectedItem(fieldId);
	if (selectedItem == null) {
		alert('Please select an item');
		return;
	}

	var itemId = $(selectedItem).readAttribute('data_id');
	if (itemId == null || itemId == '') {
		alert('Please select an item');
		return;
	}

	//launch edit form
	scForm.invoke('fieldsuite:edititem(id=' + itemId + ',fieldId=' + fieldId + ')');
}

FieldSuite.Fields.Droplink.GotoItem = function (fieldId) {
	var selectedItem = FieldSuite.Fields.Droplink.SelectedItem(fieldId);
	if (selectedItem == null) {
		alert('Please select an item');
		return;
	}

	var itemId = $(selectedItem).readAttribute('data_id');
	if (itemId == null || itemId == '') {
		alert('Please select an item');
		return;
	}

	//launch edit form
	scForm.invoke('item:load(id=' + itemId + ')');
}

FieldSuite.Fields.Droplink.SelectedItem = function (fieldId) {

	var elements = $$('#' + fieldId);
	var ddl = elements[0];

	var selected = ddl.options[ddl.selectedIndex];
	return selected;
}