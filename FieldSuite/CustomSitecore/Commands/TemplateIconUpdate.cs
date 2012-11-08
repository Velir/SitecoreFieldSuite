using Sitecore.Data.Items;
using Sitecore.Resources;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Velir.SitecoreLibrary.Extensions;

namespace FieldSuite.CustomSitecore.Commands
{
	public class TemplateIconUpdate : Command
	{
		public override void Execute(CommandContext context)
		{
			string templateIconPath = string.Empty;
			string fieldId = context.Parameters["fieldid"];
			string id = context.Parameters["id"];
			if (string.IsNullOrEmpty(id))
			{
				SheerResponse.Eval("FieldSuite.Fields.UpdateTemplateIcon(\"" + fieldId + "\",\"" + templateIconPath + "\")");
				return;
			}

			Item item = Sitecore.Context.ContentDatabase.GetItem(id);
			if (item.IsNull())
			{
				SheerResponse.Eval("FieldSuite.Fields.UpdateTemplateIcon(\"" + fieldId + "\",\"" + templateIconPath + "\")");
				return;
			}

			SheerResponse.Eval("FieldSuite.Fields.UpdateTemplateIcon(\"" + fieldId + "\",\"" + Themes.MapTheme(item.Template.Icon) + "\")");
		}
	}
}
