using System;

namespace Sitecore.SharedSource.FieldSuite.FieldGutter
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
