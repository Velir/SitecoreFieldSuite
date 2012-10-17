using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using log4net;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI;
using Sitecore.Web.UI.Sheer;
using Sitecore.Data.Items;
using Velir.SitecoreLibrary.Extensions;
using FieldSuite.Controls.ListItem;
using FieldSuite.FieldGutter;

namespace FieldSuite.Types
{
	public class TreelistEx : WebControl, IContentField, IMessageHandler
	{
		public string FieldTypeItemId { get; set; }

		private Int32 _renderSelectedItemCount;
		public Int32 RenderSelectedItemCount
		{
			get { return _renderSelectedItemCount; }
			set { _renderSelectedItemCount = value; }
		}

		// Methods
		protected void Edit(ClientPipelineArgs args)
		{
			Assert.ArgumentNotNull(args, "args");
			if (this.Enabled)
			{
				if (args.IsPostBack)
				{
					if ((args.Result != null) && (args.Result != "undefined"))
					{
						string result = args.Result;
						if (result == "-")
						{
							result = string.Empty;
						}
						if (this.Value != result)
						{
							Sitecore.Context.ClientPage.Modified = true;
						}
						this.Value = result;
						HtmlTextWriter output = new HtmlTextWriter(new StringWriter());
						this.RenderItems(output);
						SheerResponse.SetInnerHtml(this.ID, output.InnerWriter.ToString());
					}
				}
				else
				{
					UrlString urlString = new UrlString(UIUtil.GetUri("control:TreeListExEditor"));
					UrlHandle handle = new UrlHandle();
					string str3 = this.Value;
					if (str3 == "__#!$No value$!#__")
					{
						str3 = string.Empty;
					}
					handle["value"] = str3;
					handle["source"] = this.Source;
					handle["language"] = this.ItemLanguage;
					handle.Add(urlString);
					SheerResponse.ShowModalDialog(urlString.ToString(), "800px", "500px", string.Empty, true);
					args.WaitForPostBack();
				}
			}
		}

		protected override void DoRender(HtmlTextWriter output)
		{
			Assert.ArgumentNotNull(output, "output");

			//selected item wrapper
			output.Write("<div id=\"" + this.ID + "\" class=\"scContentControl\" style=\"height:80px;overflow:auto;padding:4px\" ondblclick=\"javascript:return scForm.postEvent(this,event,'treelist:edit(id=" + this.ID + ")')\" onactivate=\"javascript:return scForm.activate(this,event)\" ondeactivate=\"javascript:return scForm.activate(this,event)\">");

			//build selected items
			output.Write(string.Format("<div id=\"{0}_SelectedItems\" class=\"selectedItems\" style=\"overflow-y:none;padding:0;border:0;\">", this.ID));
			this.RenderItems(output);
			output.Write("</div>");
			output.Write("</div>");
		}

		protected override void Render(HtmlTextWriter output)
		{
			Assert.ArgumentNotNull(output, "output");

			//selected item wrapper
			output.Write("<div id=\"" + this.ID + "\" class=\"scContentControl\" style=\"height:80px;overflow:auto;padding:4px\" ondblclick=\"javascript:return scForm.postEvent(this,event,'treelist:edit(id=" + this.ID + ")')\" onactivate=\"javascript:return scForm.activate(this,event)\" ondeactivate=\"javascript:return scForm.activate(this,event)\">");

			//build selected items
			//output.Write(string.Format("<div id=\"{0}_SelectedItems\" class=\"selectedItems\" style=\"overflow-y:none;padding:0;border:0;\">", this.ID));
			this.RenderItems(output);
			//output.Write("</div>");
			output.Write("</div>");
		}

		private void RenderItems(HtmlTextWriter output)
		{
			Assert.ArgumentNotNull(output, "output");
			foreach (string itemId in this.Value.Split(new char[] { '|' }))
			{
				if (string.IsNullOrEmpty(itemId))
				{
					continue;
				}

				FieldSuiteListItem listItem = new FieldSuiteListItem();
				listItem.ShowAddRemoveButton = false;

				Item item = this.Database.GetItem(itemId);
				if (item == null)
				{
					output.Write(listItem.RenderItemNotFound(itemId, this.ID));
				}
				else
				{
					//for performance reason limit field gutter
					bool useFieldGutter = false;
					IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
					if (fieldGutterProcessor != null)
					{
						Int32 maxCount = fieldGutterProcessor.MaxCount;
						if (maxCount != 0 && RenderSelectedItemCount <= maxCount)
						{
							useFieldGutter = true;
						}
					}

					RenderSelectedItemCount++;

					listItem.ButtonClick = string.Format("FieldSuite.Fields.ToggleItem('{0}', this);", this.ID);
					listItem.ItemClick = "FieldSuite.Fields.SelectItem(this);";
					listItem.SelectedClass = "velirFieldSelected";
					listItem.ReadOnly = false;
					listItem.Text = item.DisplayName;
					listItem.HoverText = item.Paths.FullPath;
					
					//return list item as html
					output.Write(listItem.Render(item, itemId, ID, useFieldGutter));
				}
			}
		}

		/// <summary>
		/// Current Field Type
		/// </summary>
		public Item FieldTypeItem
		{
			get
			{
				string id = "{49068BF0-DFBE-40A9-8E30-FC3F76C99C50}";
				if (!string.IsNullOrEmpty(FieldTypeItemId))
				{
					id = FieldTypeItemId;
				}

				Database database = Sitecore.Data.Database.GetDatabase("core");
				if (database == null)
				{
					return null;
				}

				return database.GetItem(id);
			}
		}

		string IContentField.GetValue()
		{
			return this.Value;
		}

		void IContentField.SetValue(string value)
		{
			Assert.ArgumentNotNull(value, "value");
			this.Value = value;
		}

		void IMessageHandler.HandleMessage(Message message)
		{
			string str;
			Assert.ArgumentNotNull(message, "message");
			if (((message["id"] == this.ID) && ((str = message.Name) != null)) && (str == "treelist:edit"))
			{
				Sitecore.Context.ClientPage.Start(this, "Edit");
			}
		}

		// Properties
		public Database Database
		{
			get
			{
				UrlString str = new UrlString(this.Source);
				if (!string.IsNullOrEmpty(str["databasename"]))
				{
					return Factory.GetDatabase(str["databasename"]);
				}
				return Sitecore.Context.ContentDatabase;
			}
		}

		public string ItemLanguage
		{
			get
			{
				return StringUtil.GetString(this.ViewState["ItemLanguage"]);
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				this.ViewState["ItemLanguage"] = value;
			}
		}

		public string Source
		{
			get
			{
				return StringUtil.GetString(this.ViewState["Source"]);
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				this.ViewState["Source"] = value;
			}
		}

		public string Value
		{
			get
			{
				return StringUtil.GetString(this.ViewState["Value"]);
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				this.ViewState["Value"] = value;
			}
		}

		protected static ILog _logger;
		protected static ILog Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = LogManager.GetLogger(typeof(TreelistEx));
				}
				return _logger;
			}
		}
	}
}