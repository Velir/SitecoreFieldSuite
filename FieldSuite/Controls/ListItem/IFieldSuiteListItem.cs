using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FieldSuite.Controls.ListItem
{
	public interface IFieldSuiteListItem
	{
				/// <summary>
		/// Text of the List Item
		/// </summary>
		string Text { get; set; }

		/// <summary>
		/// Text to be shown during hover
		/// </summary>
		string HoverText { get; set; }

		/// <summary>
		/// Css class to use when the item is selected
		/// </summary>
		string SelectedClass { get; set; }

		/// <summary>
		/// Click event for when the Add/Remove button is clicked
		/// </summary>
		string ButtonClick { get; set; }

		/// <summary>
		/// Click event for when the item is clicked
		/// </summary>
		string ItemClick { get; set; }

		/// <summary>
		/// Optional parameters that can be used
		/// </summary>
		List<object> Parameters { get; set; }

		/// <summary>
		/// Is this list item read only
		/// </summary>
		bool ReadOnly { get; set; }

		/// <summary>
		/// Renders the list item as Html
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		/// <param name="useFieldGutter"></param>
		string Render(string itemId, string fieldId, bool useFieldGutter);

		/// <summary>
		/// Renders the list item as Html
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		string Render(string itemId, string fieldId);

		/// <summary>
		/// Renders the item not found list item as Html
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		string RenderItemNotFound(string itemId, string fieldId);
	}
}