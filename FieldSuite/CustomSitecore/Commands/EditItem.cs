using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Velir.SitecoreLibrary.Extensions;
using FieldSuite.FieldGutter;

namespace FieldSuite.CustomSitecore.Commands
{
	public class EditItem : Command
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
			nv.Add("id", context.Parameters["id"]);

			if (context.Parameters["fieldid"] != null)
			{
				nv.Add("fieldid", context.Parameters["fieldid"]);
			}

			//attempt to lock item
			LockItem(context.Items[0]);

			Sitecore.Context.ClientPage.Start(this, "RunEditForm", nv);
		}

		/// <summary>
		/// This method runs the rotating image modal
		/// </summary>
		/// <param name="args"></param>
		public void RunEditForm(ClientPipelineArgs args)
		{
			if (!args.IsPostBack)
			{
				//get url for field type selector modal
				UrlString ustr = new UrlString(UIUtil.GetUri("control:FieldSuiteEditForm"));
				ustr.Parameters.Add(args.Parameters);

				//open field type selector
				Sitecore.Context.ClientPage.ClientResponse.ShowModalDialog(ustr.ToString(), "712", "485", "", true);

				//wait for response
				args.WaitForPostBack();
			}
			else
			{
				string itemId = args.Parameters["id"];
				if(!string.IsNullOrEmpty(itemId) && Sitecore.Context.ContentDatabase != null)
				{
					Item item = Sitecore.Context.ContentDatabase.GetItem(itemId);
					if(item.IsNotNull())
					{
						UnlockItem(item);

						//send command to update list update's field gutter
						if (args.Parameters["fieldid"] != null && !string.IsNullOrEmpty(args.Parameters["fieldid"]))
						{
							string fieldGutterHtml = GetFieldGutterHtml(new FieldGutterArgs(item, args.Parameters["fieldid"]));
							SheerResponse.Eval("FieldSuite.Fields.UpdateItemFieldGutter(\"" + args.Parameters["fieldid"] + "\",\"" + HttpUtility.HtmlEncode(fieldGutterHtml) + "\",\"" + item.ID + "\")");
						}
					}
				}
			}
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

		/// <summary>
		/// If the item isn't already locked, acquire the lock for the item.
		/// </summary>
		/// <param name="item"></param>
		private void LockItem(Item item)
		{
			if (item != null)
			{
				bool currentUserLock = item.Locking.HasLock();
				if (!currentUserLock)
				{
					if (item.Locking.IsLocked())
					{
						item.Locking.Unlock();
					}
					item.Locking.Lock();
				}
			}
		}

		/// <summary>
		/// Unlocks the item for the current user.
		/// </summary>
		/// <param name="item"></param>
		private void UnlockItem(Item item)
		{
			if (item != null)
			{
				if (item.Locking.HasLock() && item.Locking.CanUnlock())
				{
					item.Locking.Unlock();
				}
			}
		}
	}
}