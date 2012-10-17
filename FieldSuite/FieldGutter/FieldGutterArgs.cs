using System;
using Sitecore.Data.Items;

namespace FieldSuite.FieldGutter
{
	public class FieldGutterArgs
	{
		private Item _item;
		private string _fieldId;

		public FieldGutterArgs(Item item, string fieldId)
		{
			_item = item;
			_fieldId = fieldId;
		}

		/// <summary>
		/// Item
		/// </summary>
		public Item InnerItem
		{
			get { return _item; }
			set { _item = value; }
		}

		/// <summary>
		/// ID of the Field
		/// </summary>
		public string FieldId
		{
			get { return _fieldId; }
			set { _fieldId = value; }
		}
	}
}