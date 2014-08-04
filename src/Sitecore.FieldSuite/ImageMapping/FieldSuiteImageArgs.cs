using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.FieldSuite.ImageMapping
{
	public class FieldSuiteImageArgs
	{
		public Item InnerItem { get; set; }
		public XmlNode Node { get; set; }
	}
}
