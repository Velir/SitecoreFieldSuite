using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Sitecore.Data;
using Sitecore.Text;

using Sitecore.Data.Items;
using Sitecore.SharedSource.Commons.Extensions;

namespace Sitecore.SharedSource.FieldSuite.FieldSource
{
	public class QueryFieldSourceFactory
	{
		public static QueryFieldSource GetFieldSource(string source, Item contextItem)
		{
			return new QueryFieldSource(source, contextItem);
		}
	}
}