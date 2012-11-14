using System.Collections.Generic;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.FieldSuite.FieldSource
{
	public class FieldSourceArgs
	{
		public Item CurrentField { get; set; }
		public bool Deep { get; set; }
		public Item RootItem { get; set; }
		public List<string> IncludedTemplates { get; set; }
		public List<string> ExcludedTemplates { get; set; }
		public object Parameters { get; set; }
	}
}