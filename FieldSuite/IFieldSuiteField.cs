using System.Collections.Generic;
using Sitecore.Data.Items;
using FieldSuite.FieldSource;

namespace FieldSuite
{
	public interface IFieldSuiteField
	{
		/// <summary>
		/// Current Field Type
		/// </summary>
		Item FieldTypeItem { get; }

		/// <summary>
		/// Current Content Item
		/// </summary>
		Item CurrentItem { get; }

		/// <summary>
		/// List of available items to select from
		/// </summary>
		List<Item> AvailableItems { get; }

		/// <summary>
		/// List of selected items ids
		/// </summary>
		List<string> SelectedItems { get; }

		/// <summary>
		/// Method to Build the Html for the Available Items
		/// </summary>
		/// <returns></returns>
		string BuildAvailableItems();

		/// <summary>
		/// Method to Build the Html for the Selected Items
		/// </summary>
		/// <returns></returns>
		string BuildSelectedItems();

		/// <summary>
		/// Method to Build the Html for an Item
		/// </summary>
		/// <returns></returns>
		string RenderItem(string itemId, bool selectedItem);
	}
}