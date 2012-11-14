using Sitecore.Data;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.FieldSuite.FieldSource
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