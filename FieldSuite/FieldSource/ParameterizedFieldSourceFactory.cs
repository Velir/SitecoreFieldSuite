using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data;

using Velir.SitecoreLibrary.Extensions;

namespace FieldSuite.FieldSource
{
	public class ParameterizedFieldSourceFactory
	{
		public static ParameterizedFieldSource GetFieldSource(string source) {
			
			return new ParameterizedFieldSource(source);
		}
	}
}
