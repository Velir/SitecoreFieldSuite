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
