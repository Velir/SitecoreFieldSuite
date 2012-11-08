using System.Web;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Velir.SitecoreLibrary.Extensions;
using Sitecore.Data.Items;
using FieldSuite.FieldGutter;

namespace FieldSuite.CustomSitecore.Commands
{
	public class FieldGutter : Command
	{
		/// <summary>
		/// Executes the command in the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		public override void Execute(CommandContext context)
		{
			string fieldGutterHtml = string.Empty;
			string fieldId = context.Parameters["fieldid"];
			string id = context.Parameters["id"];
			if (string.IsNullOrEmpty(id))
			{
				SheerResponse.Eval("FieldSuite.Fields.UpdateFieldGutter(\"" + fieldId + "\",\"" + fieldGutterHtml + "\")");
				return;
			}

			Item item = Sitecore.Context.ContentDatabase.GetItem(id);
			if (item.IsNull())
			{
				SheerResponse.Eval("FieldSuite.Fields.UpdateFieldGutter(\"" + fieldId + "\",\"" + fieldGutterHtml + "\")");
				return;
			}

			fieldGutterHtml = GetFieldGutterHtml(new FieldGutterArgs(item, fieldId));
			SheerResponse.Eval("FieldSuite.Fields.UpdateFieldGutter(\"" + fieldId + "\",\"" + HttpUtility.HtmlEncode(fieldGutterHtml) + "\")");
		}

		protected virtual string GetFieldGutterHtml(FieldGutterArgs args)
		{
			if (args == null || args.InnerItem.IsNull())
			{
				return string.Empty;
			}

			IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
			if (fieldGutterProcessor == null)
			{
				return string.Empty;
			}

			string fieldGutterHtml = fieldGutterProcessor.Process(args);
			if (string.IsNullOrEmpty(fieldGutterHtml))
			{
				return string.Empty;
			}

			return fieldGutterHtml;
		}
	}
}
