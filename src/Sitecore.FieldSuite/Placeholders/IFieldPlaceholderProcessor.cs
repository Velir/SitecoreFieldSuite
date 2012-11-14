namespace Sitecore.SharedSource.FieldSuite.Placeholders
{
	public interface IFieldPlaceholderProcessor
	{
		/// <summary>
		/// Processes the Field Item
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		string Process(FieldPlaceholderArgs args);
	}
}
