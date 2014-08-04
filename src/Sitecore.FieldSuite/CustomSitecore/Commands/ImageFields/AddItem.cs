using System.Collections.Specialized;
using System.Linq;
using Sitecore;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.SharedSource.FieldSuite.Commands.ImageFields
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

			NameValueCollection nv = new NameValueCollection();
			nv.Add("templateid", context.Parameters["templateid"]);
			nv.Add("fieldname", context.Parameters["fieldname"]);
			nv.Add("primaryid", context.Parameters["currentitemid"]);
			nv.Add("parentFolder", context.Parameters["parentfolderid"]);

			Sitecore.Context.ClientPage.Start(this, "RunRotatingImageModal", nv);
		}

		/// <summary>
		/// This method runs the rotating image modal
		/// </summary>
		/// <param name="args"></param>
		public void RunRotatingImageModal(ClientPipelineArgs args)
		{
			if (!args.IsPostBack)
			{
				//get url for field type selector modal
				UrlString ustr = new UrlString(UIUtil.GetUri("control:FieldSuiteImagesEditForm"));
				ustr.Parameters.Add(args.Parameters);

				//open field type selector
				Sitecore.Context.ClientPage.ClientResponse.ShowModalDialog(ustr.ToString(), "712", "485", "", true);

				//wait for response
				args.WaitForPostBack();
			}
			else
			{
				//reload the item
				Sitecore.Context.ClientPage.SendMessage(this, string.Format("item:refresh(id={0})", args.Parameters["primaryid"]));
				Sitecore.Context.ClientPage.SendMessage(this, string.Format("item:load(id={0})", args.Parameters["primaryid"]));
			}
		}
	}
}
