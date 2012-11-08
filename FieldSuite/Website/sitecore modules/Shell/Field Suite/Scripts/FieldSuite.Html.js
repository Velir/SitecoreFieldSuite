//*****************************************************
// Author: Tim Braga - Velir
// Email: tim.braga@velir.com
// Date: 03/22/2012
// Html Utility Methods
//*****************************************************

if (FieldSuite == null) {
	FieldSuite = [];
}

FieldSuite.Html = [];

// <summary>
// Checks an element for a specified class
// </summary>
// <param name="element">Element</param>
// <param name="className">The class name to be checked</param>
FieldSuite.Html.HasClass = function (element, className) {
	if (element == null) {
		return false;
	}

	var elementClassName = element.className;
	return (elementClassName.length > 0 && (elementClassName == className || new RegExp("(^|\\s)" + className + "(\\s|$)").test(elementClassName)));
}

// <summary>
// Adds a class from an element
// </summary>
// <param name="element">Element</param>
// <param name="className">The class name to be added</param>
FieldSuite.Html.AddClass = function (element, className) {
	if (element == null) {
		return false;
	}

	if (!FieldSuite.Html.HasClass(element, className)) {
		element.className += (element.className ? ' ' : '') + className;
	}
}

// <summary>
// Removes a class from an element
// </summary>
// <param name="element">Element</param>
// <param name="className">The class name to be removed</param>
FieldSuite.Html.RemoveClass = function (element, className) {
	if (element == null) {
		return false;
	}
	element.className = element.className.replace(new RegExp("(^|\\s+)" + className + "(\\s+|$)"), ' ').strip();
}

// <summary>
// Copy element to another element
// </summary>
// <param name="e">Element to be copied</param>
// <param name="target">This is the element that it will be copied to</param>
FieldSuite.Html.Copy = function (e, target) {
	var eId = $(e);
	var copyE = eId.cloneNode(true);
	var cLength = copyE.childNodes.length - 1;
	copyE.id = e + '-copy';

	for (var i = 0; cLength >= i; i++) {
		if (copyE.childNodes[i].id) {
			var cNode = copyE.childNodes[i];
			var firstId = cNode.id;
			cNode.id = firstId + '-copy';
		}
	}
	$(target).appendChild(copyE);
}

// <summary>
// Moves element to another element
// </summary>
// <param name="e">Element to be moved</param>
// <param name="target">This is the element that it will be moved to</param>
FieldSuite.Html.Move = function (e, target) {
	var eId = $(e);
	$(target).appendChild(eId);
}

FieldSuite.Html.HtmlDecode = function (input) {
	var e = document.createElement('div');
	e.innerHTML = input;
	return e.childNodes.length === 0 ? "" : e.childNodes[0].nodeValue;
}