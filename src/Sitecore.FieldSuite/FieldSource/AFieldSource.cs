using System.Collections.Generic;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.FieldSuite.FieldSource
{
	public class AFieldSource : IFieldSource
	{
		private Item _currentFieldItem;

		public AFieldSource(Item currentFieldItem)
		{
			_currentFieldItem = currentFieldItem;

		}

		/// <summary>
		/// Current Field
		/// </summary>
		public Item CurrentFieldItem
		{
			get { return _currentFieldItem; }
		}

		public bool Deep { get; set; }
		public Item RootItem { get; set; }
		public List<string> IncludedTemplates { get; set; }
		public List<string> ExcludedTemplates { get; set; }
		public object Parameters { get; set; }

		/// <summary>
		/// Returns a list of items for a field
		/// </summary>
		/// <returns></returns>
		public virtual List<Item> GetItems()
		{
			return new List<Item>();
		}
	}
}
