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
	public class FieldSourceFactory
	{
		public static IFieldSource GetFieldSource(string source)
		{
			if (string.IsNullOrEmpty(source))
			{
				return null;
			}

			//verify this is a configurable source
			if(!source.ToLower().Contains("fieldsource"))
			{
				return null;
			}

			string fieldSourceId = source.Substring(source.IndexOf('{'), source.IndexOf('}'));
			if(string.IsNullOrEmpty(fieldSourceId))
			{
				return null;
			}

			if (Sitecore.Context.ContentDatabase == null)
			{
				return null;
			}

			Database contentDatabase = Sitecore.Context.ContentDatabase;
			Item item = contentDatabase.GetItem(fieldSourceId);
			if (item.IsNull())
			{
				return null;
			}

			return new AFieldSource(item);
		}
	}
}