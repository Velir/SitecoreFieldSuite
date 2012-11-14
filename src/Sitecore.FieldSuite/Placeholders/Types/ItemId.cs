
namespace Sitecore.SharedSource.FieldSuite.Placeholders.Types
{
	public class ItemId : IFieldPlaceholder
	{
		/// <summary>
		/// Returns Field Placeholder Value
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public string Execute(FieldPlaceholderArgs args)
		{
			if (args == null || string.IsNullOrEmpty(args.ClickEvent) || string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(args.ItemId))
			{
				return string.Empty;
			}

			string clickEvent = args.ClickEvent;
			clickEvent = clickEvent.Replace(Key, args.ItemId);

			return clickEvent;
		}

		/// <summary>
		/// Key
		/// </summary>
		public string Key
		{
			get;set;
		}
	}
}