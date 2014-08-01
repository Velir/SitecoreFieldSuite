using System;

namespace FieldSuite.FieldGutter
{
	public interface IFieldGutter
	{
		/// <summary>
		/// Returns Field Gutter Html
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		string Execute(FieldGutterArgs args);

		/// <summary>
		/// Max Count, for performance reasons
		/// </summary>
		Int32 MaxCount { get; set; }
	}
}