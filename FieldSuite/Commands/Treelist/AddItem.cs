using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Sitecore;
using Sitecore.Data;
using Sitecore.Resources;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Sitecore.Data.Items;
using Velir.SitecoreLibrary.Extensions;
using FieldSuite.Controls.ListItem;

namespace FieldSuite.Commands.Treelist
{
	public class AddItem : Command
	{
		/// <summary>
		/// Executes the command in the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		public override void Execute(CommandContext context)
		{
			if (context.Items.Count() == 0)
			{
				return;
			}

			Item currentItem = context.Items[0];
			if (currentItem.IsNull())
			{
				return;
			}

			string itemid = context.Parameters["itemid"];
			string fieldid = context.Parameters["fieldid"];
			string excludedTemplates = context.Parameters["excludedTemplates"];
			string includedTemplates = context.Parameters["includedTemplates"];

			if (string.IsNullOrEmpty(fieldid))
			{
				return;
			}

			//is this item available to be added?
			if (!IsAvailableToBeAdded(itemid, excludedTemplates, includedTemplates))
			{
				return;
			}

			NameValueCollection nv = new NameValueCollection();
			nv.Add("fieldid", fieldid);
			nv.Add("currentid", currentItem.ID.ToString());
			nv.Add("additemid", itemid);

			Context.ClientPage.Start(this, "RunAddForm", nv);
		}

		/// <summary>
		/// This method runs the source field builder as a modal
		/// </summary>
		/// <param name="args"></param>
		public void RunAddForm(ClientPipelineArgs args)
		{
			string html = RenderItem(args.Parameters["additemid"], args.Parameters["fieldid"]);
			if (string.IsNullOrEmpty(html))
			{
				return;
			}

			html = HttpUtility.HtmlEncode(html);

			SheerResponse.Eval("FieldSuite.Fields.Treelist.AddItemToContent(\"" + args.Parameters["fieldid"] + "\",\"" + html + "\")");
		}

		private bool IsAvailableToBeAdded(string itemId, string excludedTemplates, string includedTemplates)
		{
			Item item = Context.ContentDatabase.GetItem(itemId);
			if(item.IsNull() || item.Template == null)
			{
				return false;
			}

			//is part of the available list of templates)
			if (!string.IsNullOrEmpty(includedTemplates))
			{
				if (includedTemplates.Contains(item.TemplateName))
				{
					return true;
				}

				//if its not part of the list then deny it access to be added
				return false;
			}

			//is part of the excluded list of templates
			if (!string.IsNullOrEmpty(excludedTemplates) && excludedTemplates.Contains(item.TemplateName))
			{
				return false;
			}

			//default to true
			return true;
		}

		/// <summary>
		/// Renders an item
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		/// <returns></returns>
		public virtual string RenderItem(string itemId, string fieldId)
		{
			if (string.IsNullOrEmpty(itemId) || string.IsNullOrEmpty(fieldId))
			{
				return string.Empty;
			}

			FieldSuiteListItem listItem = new FieldSuiteListItem();
			listItem.ShowAddRemoveButton = true;

			//attempt to get item from sitecore
			Item item = Context.ContentDatabase.GetItem(itemId);
			if (item.IsNull())
			{
				//return not found list item template
				return listItem.RenderItemNotFound(itemId, fieldId);
			}

			listItem.ButtonClick = string.Format("FieldSuite.Fields.Treelist.RemoveItem('{0}', this);", fieldId);
			listItem.ItemClick = "FieldSuite.Fields.SelectItem(this);";
			listItem.SelectedClass = "velirFieldSelected";
			listItem.ReadOnly = false;
			listItem.Text = item.DisplayName;
			listItem.HoverText = item.Paths.FullPath;

			//return list item as html
			return listItem.Render(item, itemId, fieldId, true);
		}
	}
}