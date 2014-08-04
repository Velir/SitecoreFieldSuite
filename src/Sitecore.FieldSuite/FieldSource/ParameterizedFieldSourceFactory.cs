using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Items;
using Sitecore.Data;

using Sitecore.SharedSource.Commons.Extensions;

namespace Sitecore.SharedSource.FieldSuite.FieldSource
{
	public class ParameterizedFieldSourceFactory
	{
		public static ParameterizedFieldSource GetFieldSource(string source) {
			
			return new ParameterizedFieldSource(source);
		}
	}
}
