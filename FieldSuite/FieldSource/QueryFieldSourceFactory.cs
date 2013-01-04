using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Sitecore.Data;
using Sitecore.Text;
using Velir.SitecoreLibrary.CustomItems.Common.FieldSuite.FieldSource;

using Sitecore.Data.Items;
using Velir.SitecoreLibrary.Extensions;

namespace FieldSuite.FieldSource
{
	public class QueryFieldSourceFactory
	{
		public static QueryFieldSource GetFieldSource(string source, Item contextItem)
		{
			return new QueryFieldSource(source, contextItem);
		}
	}
}