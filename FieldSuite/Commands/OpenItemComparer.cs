using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Velir.SitecoreLibrary.Extensions;
using FieldSuite.FieldGutter;

namespace FieldSuite.Commands
{
	public class OpenItemComparer : Command
	{
		/// <summary>
		/// Overriding the Execute method that Sitecore calls.
		/// </summary>
		/// <param name = "context"></param>
		public override void Execute(CommandContext context)
		{
			if (context.Parameters["id"] == null || string.IsNullOrEmpty(context.Parameters["id"]))
			{
				return;
			}

			//only use on authoring environment
			Item currentItem = Sitecore.Context.ContentDatabase.GetItem(context.Parameters["id"]);
			if (currentItem == null)
			{
				return;
			}

			NameValueCollection nv = new NameValueCollection();
			nv.Add("id", context.Parameters["id"]);

			Item item = Sitecore.Context.ContentDatabase.GetItem(context.Parameters["id"]);
			if (item.IsNull())
			{
				return;
			}

			if (context.Parameters["fieldid"] != null)
			{
				nv.Add("fieldid", context.Parameters["fieldid"]);
			}

			nv.Add("la", item.Language.ToString());
			nv.Add("vs", item.Version.ToString());

			Sitecore.Context.ClientPage.Start(this, "ItemComparerForm", nv);
		}

		/// <summary>
		/// This method runs the rotating image modal
		/// </summary>
		/// <param name="args"></param>
		public void ItemComparerForm(ClientPipelineArgs args)
		{
			if (!args.IsPostBack)
			{
				//Build the url for the control
				string controlUrl = UIUtil.GetUri("control:ItemComparerViewer");
				string id = args.Parameters["id"];
				string la = args.Parameters["la"];
				string vs = args.Parameters["vs"];
				string[] parameters = { controlUrl, id, la, vs };
				string url = string.Format("{0}&id={1}&la={2}&vs={3}", parameters);

				//Open the dialog
				SheerResponse.ShowModalDialog(new UrlString(url).ToString(), "500", "1000");
			}
		}
	}
}