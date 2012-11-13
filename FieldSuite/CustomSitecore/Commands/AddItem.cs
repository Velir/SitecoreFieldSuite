using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Sitecore;
using Sitecore.Data;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Sitecore.Data.Items;
using FieldSuite.Controls.ListItem;
using FieldSuite.ImageMapping;

namespace FieldSuite.CustomSitecore.Commands
{
	public class AddItem : Command
	{
		/// <summary>
		/// Executes the command in the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		public override void Execute(CommandContext context)
		{
			if (!context.Items.Any())
			{
				return;
			}

			Item currentItem = context.Items[0];
			if (currentItem.IsNull())
			{
				return;
			}

			string source = context.Parameters["source"];
			string fieldid = context.Parameters["fieldid"];
			if (string.IsNullOrEmpty(fieldid))
			{
				return;
			}

			NameValueCollection nv = new NameValueCollection();
			nv.Add("source", source);
			nv.Add("fieldid", fieldid);
			nv.Add("currentid", currentItem.ID.ToString());

			Sitecore.Context.ClientPage.Start(this, "RunAddForm", nv);
		}

		/// <summary>
		/// This method runs the source field builder as a modal
		/// </summary>
		/// <param name="args"></param>
		public void RunAddForm(ClientPipelineArgs args)
		{
			if (!args.IsPostBack)
			{
				//get url for field type selector modal
				UrlString ustr = new UrlString(UIUtil.GetUri("control:FieldSuiteAddForm"));
				ustr.Parameters.Add(args.Parameters);

				//open field type selector
				Context.ClientPage.ClientResponse.ShowModalDialog(ustr.ToString(), "500", "300", "", true);

				//wait for response
				args.WaitForPostBack();
			}
			else
			{
				//verify result
				if (!args.HasResult || string.IsNullOrEmpty(args.Result))
				{
					return;
				}
				
				string linkedItemId = args.Result;
				if (!ID.IsID(linkedItemId))
				{
					return;
				}

				IFieldSuiteListItem listItem = GetListItem(linkedItemId, args.Parameters["fieldid"]);
				if (listItem == null)
				{
					listItem = new FieldSuiteImageListItem();
					SheerResponse.Eval("FieldSuite.Fields.ImagesField.AddItemToContent(\"" + args.Parameters["fieldid"] + "\",\"" + HttpUtility.HtmlEncode(listItem.RenderItemNotFound(linkedItemId, args.Parameters["fieldid"])) + "\",\"" + linkedItemId + "\")");
					return;
				}

				SheerResponse.Eval("FieldSuite.Fields.ImagesField.AddItemToContent(\"" + args.Parameters["fieldid"] + "\",\"" + HttpUtility.HtmlEncode(listItem.Render(linkedItemId, args.Parameters["fieldid"], true)) + "\",\"" + linkedItemId + "\")");
			}
		}

		private IFieldSuiteListItem GetListItem(string itemId, string fieldId)
		{
			Item item = Sitecore.Context.ContentDatabase.GetItem(itemId);
			if (item == null)
			{
				return null;
			}

			IFieldSuiteImage fieldSuiteImage = FieldSuiteImageFactory.GetFieldSuiteImage(item);
			if (fieldSuiteImage == null)
			{
				return null;
			}

			FieldSuiteImageListItem listItem = new FieldSuiteImageListItem();

			//set default
			string imageSrc = "/sitecore modules/shell/field suite/images/unknown.png";

			//set to image of the item
			if (!string.IsNullOrEmpty(fieldSuiteImage.ImageUrl))
			{
				//setup image and sitecore click event
				imageSrc = fieldSuiteImage.ImageUrl.ToLower();
			}

			//add thumbnail parameter
			string parameters = "w=125&h=125&thn=true&db=master";
			if (imageSrc.Contains("?"))
			{
				imageSrc += "&" + parameters;
			}
			else
			{
				imageSrc += "?" + parameters;
			}

			string titleText = string.Format("{0}: {1}", item.Name, item.Paths.FullPath);
			string onclick = string.Format("javascript:FieldSuite.Fields.ImagesField.ToggleItem(this, '{0}');", fieldId);
			string description = fieldSuiteImage.Title;
			if (description.Length > 21)
			{
				description = description.Substring(0, 21) + "...";
			}

			listItem.Text = description;
			listItem.HoverText = titleText;
			listItem.ReadOnly = false;
			listItem.ItemClick = onclick;
			listItem.Parameters = new List<object>();
			listItem.Parameters.Add(string.Format("<img border=\"0\" src=\"{0}\">", imageSrc));

			return listItem;
		}
	}
}