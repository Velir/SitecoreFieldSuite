using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sitecore.Data.Items;

namespace FieldSuite.FieldSource
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