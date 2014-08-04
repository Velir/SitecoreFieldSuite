using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Web.UI.HtmlControls.Data;

using Sitecore.SharedSource.Commons.Extensions;

namespace Sitecore.SharedSource.FieldSuite.FieldSource
{
	/// <summary>
	/// The QueryFieldSource contains a Sitecore Query that is run against the Context Item
	/// </summary>
	public class QueryFieldSource : AbstractFieldSource
	{
		#region Properties

		public override string Source { get; set; }
		
		private Item _contextItem;
		/// <summary>
		/// Context Item where the field value is stored
		/// </summary>
		public Item ContextItem {
			get { return _contextItem; }
		}

		#endregion Properties

		#region Constructors

		public QueryFieldSource(string source, Item contextItem) {
			_contextItem = contextItem;
			this.Source = source;
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Returns a list of items for a field
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public List<Item> GetItems()
		{
			return (ContextItem.IsNull()) ? new List<Item>() : LookupSources.GetItems(ContextItem, this.Source).ToList();
		}

		#endregion Methods
	}
}
