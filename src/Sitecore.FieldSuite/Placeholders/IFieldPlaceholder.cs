using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitecore.SharedSource.FieldSuite.Placeholders
{
	public interface IFieldPlaceholder
	{
		/// <summary>
		/// Returns Field Placeholder Value
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		string Execute(FieldPlaceholderArgs args);

		/// <summary>
		/// Key
		/// </summary>
		string Key { get; set; }
	}
}
