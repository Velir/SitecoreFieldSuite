using Sitecore.Data.Items;

namespace Sitecore.SharedSource.FieldSuite.Placeholders
{
	public class FieldPlaceholderArgs
	{
		/// <summary>
		/// The Click Event that is defined in the Field Type in the Core Database
		/// </summary>
		public string ClickEvent { get; set; }

		/// <summary>
		/// Current Item
		/// </summary>
		public Item InnerItem { get; set; }

		/// <summary>
		/// Field Item from the Core Database
		/// </summary>
		public Item FieldItem { get; set; }

		/// <summary>
		/// Current Item's Template
		/// </summary>
		public TemplateItem TemplateItem { get; set; }

		/// <summary>
		/// Source of the Current Field
		/// </summary>
		public string Source { get; set; }

		/// <summary>
		/// The ID of the current field we are on, this is not the ID from the Field Type in the core database.
		/// </summary>
		public string FieldId { get; set; }

		/// <summary>
		/// The ID of the current item
		/// </summary>
		public string ItemId { get; set; }
	}
}