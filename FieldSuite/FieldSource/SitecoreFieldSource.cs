using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Diagnostics;
using Sitecore.Web.UI.HtmlControls.Data;

using Sitecore.Data.Items;
using Velir.SitecoreLibrary.Extensions;

namespace FieldSuite.FieldSource
{
	public class SitecoreFieldSource : AFieldSource
	{
		public SitecoreFieldSource(Item CurrentFieldItem) : base(CurrentFieldItem)
		{
		}

		/// <summary>
		/// Returns a list of items for a field
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public override List<Item> GetItems()
		{
			if (CurrentFieldItem.IsNull())
			{
				return new List<Item>();
			}

			TemplateFieldItem templateFieldItem = CurrentFieldItem;

			string source = string.Empty;
			if (!string.IsNullOrEmpty(templateFieldItem.Source))
			{
				source = templateFieldItem.Source;
			}

			return LookupSources.GetItems(CurrentFieldItem, source).ToList();
		}
	}
}