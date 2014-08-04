using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sitecore.Data.Items;

namespace Sitecore.SharedSource.FieldSuite.FieldSource
{
	public abstract class AbstractFieldSource
	{
		public abstract string Source { get; set; }
	}
}