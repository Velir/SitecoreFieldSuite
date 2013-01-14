using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using Sitecore;
using Sitecore.Data;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Velir.SitecoreLibrary.Extensions;
using Sitecore.Data.Items;
using FieldSuite.Controls.ListItem;
using FieldSuite.FieldGutter;

namespace FieldSuite.Commands
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

			Item item = (string.IsNullOrEmpty(id)) ? null : Sitecore.Context.ContentDatabase.GetItem(id);
			if(item.IsNotNull())
				fieldGutterHtml = HttpUtility.HtmlEncode(GetFieldGutterHtml(new FieldGutterArgs(item, fieldId)));
			
			SheerResponse.Eval("FieldSuite.Fields.UpdateFieldGutter(\"" + fieldId + "\",\"" + fieldGutterHtml + "\")");
		}

		protected virtual string GetFieldGutterHtml(FieldGutterArgs args)
		{
			IFieldGutterProcessor fieldGutterProcessor = (args == null || args.InnerItem.IsNull()) ? null : FieldGutterProcessorFactory.GetProcessor();
			if (fieldGutterProcessor == null)
				return string.Empty;
			
			string fieldGutterHtml = fieldGutterProcessor.Process(args);
			return (string.IsNullOrEmpty(fieldGutterHtml)) ? string.Empty : fieldGutterHtml;
		}
	}
}
