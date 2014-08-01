using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Data.Items;

namespace FieldSuite.FieldGutter
{
	public interface IFieldGutterProcessor
	{
		/// <summary>
		/// Processes the Field Item
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		string Process(FieldGutterArgs args);

		/// <summary>
		/// Max Number of Field Items for the Processor to run against
		/// </summary>
		/// <returns></returns>
		Int32 MaxCount { get; set;}
	}
}
