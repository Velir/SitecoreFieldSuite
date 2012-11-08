using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using Sitecore;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Data.Items;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Velir.SitecoreLibrary.Extensions;
using FieldSuite.Controls;
using FieldSuite.Controls.ListItem;
using FieldSuite.FieldGutter;

namespace FieldSuite.CustomSitecore.Fields
{
	public class Treelist : AFieldSuiteField
	{
		private Database _database;

		/// <summary>
		/// Current Field Type
		/// </summary>
		public override Item FieldTypeItem
		{
			get
			{
				string id = "{1A670679-36D2-4805-8392-917B077D20F9}";
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

		public Database RenderingDatabase
		{
			get
			{
				if(_database != null)
				{
					return _database;
				}

				_database = Sitecore.Context.ContentDatabase;

				string databaseName = StringUtil.ExtractParameter("databasename", this.Source).Trim().ToLower();
				if(!string.IsNullOrEmpty(databaseName))
				{
					databaseName = databaseName.Trim().ToLower();
					_database = Sitecore.Data.Database.GetDatabase(databaseName);
					return _database;
				}

				return _database;
			}
		}

		protected override void OnLoad(EventArgs args)
		{
			Assert.ArgumentNotNull(args, "args");
			if (!Sitecore.Context.ClientPage.IsEvent)
			{
				this.SetProperties();
				this.GetControlAttributes();
				base.SetViewStateString("ID", this.ID);

				FieldSuiteTreeviewEx ex = new FieldSuiteTreeviewEx
				{
					ID = this.ID + "_all"
				};

				this.Controls.Add(ex);
				ex.ParentId = this.ID;
				ex.DblClick = this.ID + ".Add";
				ex.AllowDragging = false;
				ex.Enabled = !ReadOnly;

				DataContext context = new DataContext();
				this.Controls.Add(context);
				context.ID = GetUniqueID("D");
				context.Filter = this.FormTemplateFilterForDisplay();
				ex.DataContext = context.ID;
				context.DataViewName = "Master";
				if (!string.IsNullOrEmpty(this.DatabaseName))
				{
					context.Parameters = "databasename=" + this.DatabaseName;
				}
				context.Root = this.DataSource;
				ex.ShowRoot = true;

				if (!string.IsNullOrEmpty(ExcludeTemplatesForSelection))
				{
					List<string> templates = new List<string>();
					foreach (string templateName in ExcludeTemplatesForSelection.Split(','))
					{
						if (string.IsNullOrEmpty(templateName))
						{
							continue;
						}

						templates.Add(templateName);
					}

					ex.ExcludeTemplatesForSelection = templates;
				}

				if (!string.IsNullOrEmpty(IncludeTemplatesForSelection))
				{
					List<string> templates = new List<string>();
					foreach (string templateName in IncludeTemplatesForSelection.Split(','))
					{
						if (string.IsNullOrEmpty(templateName))
						{
							continue;
						}

						templates.Add(templateName);
					}

					ex.IncludeTemplatesForSelection = templates;
				}
			}

			base.OnLoad(args);
		}

		/// <summary>
		/// 	Render the Field
		/// </summary>
		/// <param name = "output"></param>
		protected override void Render(HtmlTextWriter output)
		{
			Assert.ArgumentNotNull(output, "output");
			base.ServerProperties["ID"] = this.ID;
			string str = string.Empty;
			if (this.ReadOnly)
			{
				str = " disabled=\"disabled\"";
			}
			output.Write(string.Format("<div id=\"{0}\">", this.ID));

			output.Write("<input id=\"" + this.ID + "_Value\" type=\"hidden\" value=\"" + StringUtil.EscapeQuote(this.Value) +
						 "\" />");

			//build the menu items
			this.Attributes["class"] = "scContentControl scContentControlTreelist";
			output.Write("<table" + this.GetControlAttributes() + ">");
			output.Write("<tr>");

			//build available list
			output.Write("<td valign=\"top\" width=\"50%\" height=\"100%\">");
			output.Write("<div class=\"velirTreelist\">");
			base.RenderContents(output);
			output.Write("</div>");
			output.Write("</td>");

			//build selected list
			output.Write("<td valign=\"top\" width=\"50%\" height=\"100%\">");
			output.Write(BuildSelectedItems());
			output.Write("</td>");

			//build sort icons
			output.Write("<td valign=\"top\" width=\"20\">");
			this.RenderButton(output, "Core/16x16/arrow_blue_up.png", "FieldSuite.Fields.MoveItemUp('" + this.ID + "')");
			output.Write("<br />");
			this.RenderButton(output, "Core/16x16/arrow_blue_down.png", "FieldSuite.Fields.MoveItemDown('" + this.ID + "')");
			output.Write("</td>");

			output.Write("</tr>");
			output.Write("</table>");
			output.Write("</div>");
		}

		/// <summary>
		/// Renders an item
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public string RenderItem(string itemId)
		{
			return RenderItem(itemId, false);
		}

		/// <summary>
		/// Renders an item
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="selectedItem"></param>
		/// <returns></returns>
		public override string RenderItem(string itemId, bool selectedItem)
		{
			if (string.IsNullOrEmpty(itemId))
			{
				return string.Empty;
			}

			FieldSuiteListItem listItem = new FieldSuiteListItem();
			listItem.ShowAddRemoveButton = true;

			Item item = RenderingDatabase.GetItem(itemId);
			if (item.IsNull())
			{
				return listItem.RenderItemNotFound(itemId, this.ID);
			}

			listItem.ButtonClick = string.Format("FieldSuite.Fields.Treelist.RemoveItem('{0}', this);", this.ID);
			listItem.ItemClick = "FieldSuite.Fields.SelectItem(this);";
			listItem.SelectedClass = "velirFieldSelected";
			listItem.ReadOnly = this.ReadOnly;
			listItem.Text = item.DisplayName;
			listItem.HoverText = item.Paths.FullPath;

			Int32 renderCount = RenderItemCount;
			if (selectedItem)
			{
				renderCount = RenderSelectedItemCount;
			}

			//for performance reason limit field gutter
			bool useFieldGutter = false;
			IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
			if (fieldGutterProcessor != null)
			{
				Int32 maxCount = fieldGutterProcessor.MaxCount;
				if (maxCount != 0 && renderCount <= maxCount)
				{
					useFieldGutter = true;
				}
			}

			//return list item as html
			if (selectedItem)
			{
				RenderSelectedItemCount++;
			}
			else
			{
				RenderItemCount++;
			}

			//return list item as html
			return listItem.Render(item, item.ID.ToString(), this.ID, useFieldGutter);
		}

		/// <summary>
		/// Rendering the Sort Icons
		/// </summary>
		/// <param name="output"></param>
		/// <param name="icon"></param>
		/// <param name="click"></param>
		private void RenderButton(HtmlTextWriter output, string icon, string click)
		{
			Assert.ArgumentNotNull(output, "output");
			Assert.ArgumentNotNull(icon, "icon");
			Assert.ArgumentNotNull(click, "click");
			ImageBuilder builder = new ImageBuilder
			{
				Src = icon,
				Width = 0x10,
				Height = 0x10,
				Margin = "2px"
			};
			if (!this.ReadOnly)
			{
				builder.OnClick = click;
			}
			output.Write(builder.ToString());
		}

		protected void Add()
		{
			if (this.Disabled)
			{
				return;
			}

			string viewStateString = base.GetViewStateString("ID");
			FieldSuiteTreeviewEx ex = this.FindControl(viewStateString + "_all") as FieldSuiteTreeviewEx;
			Listbox listbox = this.FindControl(viewStateString + "_selected") as Listbox;
			Item selectionItem = ex.GetSelectionItem();
			if (selectionItem == null)
			{
				SheerResponse.Alert("Select an item in the Content Tree.", new string[0]);
				return;
			}

			if (this.HasExcludeTemplateForSelection(selectionItem))
			{
				return;
			}

			if (this.IsDeniedMultipleSelection(selectionItem, listbox))
			{
				SheerResponse.Alert("You cannot select the same item twice.", new string[0]);
			}
			else if (this.HasIncludeTemplateForSelection(selectionItem))
			{
				SheerResponse.Eval("scForm.browser.getControl('" + viewStateString + "_selected').selectedIndex=-1");
				ListItem control = new ListItem
				{
					ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("L")
				};
				Sitecore.Context.ClientPage.AddControl(listbox, control);
				control.Header = selectionItem.DisplayName;
				control.Value = control.ID + "|" + selectionItem.ID;
				SheerResponse.Refresh(listbox);
				SetModified();
			}

		}

		private bool HasExcludeTemplateForSelection(Item item)
		{
			return ((item == null) || HasItemTemplate(item, this.ExcludeTemplatesForSelection));
		}

		private bool HasIncludeTemplateForSelection(Item item)
		{
			Assert.ArgumentNotNull(item, "item");
			return ((this.IncludeTemplatesForSelection.Length == 0) || HasItemTemplate(item, this.IncludeTemplatesForSelection));
		}

		private static bool HasItemTemplate(Item item, string templateList)
		{
			Assert.ArgumentNotNull(templateList, "templateList");
			if (item == null)
			{
				return false;
			}
			if (templateList.Length == 0)
			{
				return false;
			}
			string[] strArray = templateList.Split(new char[] { ',' });
			ArrayList list = new ArrayList(strArray.Length);
			for (int i = 0; i < strArray.Length; i++)
			{
				list.Add(strArray[i].Trim().ToLower());
			}
			return list.Contains(item.TemplateName.Trim().ToLower());
		}

		private bool IsDeniedMultipleSelection(Item item, Listbox listbox)
		{
			Assert.ArgumentNotNull(listbox, "listbox");
			if (item == null)
			{
				return true;
			}
			if (!this.AllowMultipleSelection)
			{
				foreach (ListItem item2 in listbox.Controls)
				{
					string[] strArray = item2.Value.Split(new char[] { '|' });
					if ((strArray.Length >= 2) && (strArray[1] == item.ID.ToString()))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Properties
		[Category("Data"), Description("If set to Yes, allows the same item to be selected more than once")]
		public bool AllowMultipleSelection
		{
			get
			{
				return base.GetViewStateBool("AllowMultipleSelection");
			}
			set
			{
				base.SetViewStateBool("AllowMultipleSelection", value);
			}
		}

		public string DatabaseName
		{
			get
			{
				return base.GetViewStateString("DatabaseName");
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				base.SetViewStateString("DatabaseName", value);
			}
		}

		[Category("Data"), Description("Comma separated list of item names/ids.")]
		public string ExcludeItemsForDisplay
		{
			get
			{
				return base.GetViewStateString("ExcludeItemsForDisplay");
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				base.SetViewStateString("ExcludeItemsForDisplay", value);
			}
		}

		[Description("Comma separated list of template names. If this value is set, items based on these template will not be displayed in the tree."), Category("Data")]
		public string ExcludeTemplatesForDisplay
		{
			get
			{
				return base.GetViewStateString("ExcludeTemplatesForDisplay");
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				base.SetViewStateString("ExcludeTemplatesForDisplay", value);
			}
		}

		[Category("Data"), Description("Comma separated list of template names. If this value is set, items based on these template will not be included in the menu.")]
		public string ExcludeTemplatesForSelection
		{
			get
			{
				return base.GetViewStateString("ExcludeTemplatesForSelection");
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				base.SetViewStateString("ExcludeTemplatesForSelection", value);
			}
		}

		[Description("Comma separated list of items names/ids."), Category("Data")]
		public string IncludeItemsForDisplay
		{
			get
			{
				return base.GetViewStateString("IncludeItemsForDisplay");
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				base.SetViewStateString("IncludeItemsForDisplay", value);
			}
		}

		[Category("Data"), Description("Comma separated list of template names. If this value is set, only items based on these template can be displayed in the menu.")]
		public string IncludeTemplatesForDisplay
		{
			get
			{
				return base.GetViewStateString("IncludeTemplatesForDisplay");
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				base.SetViewStateString("IncludeTemplatesForDisplay", value);
			}
		}

		[Category("Data"), Description("Comma separated list of template names. If this value is set, only items based on these template can be included in the menu.")]
		public string IncludeTemplatesForSelection
		{
			get
			{
				return base.GetViewStateString("IncludeTemplatesForSelection");
			}
			set
			{
				Assert.ArgumentNotNull(value, "value");
				base.SetViewStateString("IncludeTemplatesForSelection", value);
			}
		}

		protected void SetProperties()
		{
			string @string = StringUtil.GetString(new string[1]
      {
        this.Source
      });
			if (Sitecore.Data.ID.IsID(@string))
				this.DataSource = this.Source;
			else if (this.Source != null && !@string.Trim().StartsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				this.ExcludeTemplatesForSelection = StringUtil.ExtractParameter("ExcludeTemplatesForSelection", this.Source).Trim();
				this.IncludeTemplatesForSelection = StringUtil.ExtractParameter("IncludeTemplatesForSelection", this.Source).Trim();
				this.IncludeTemplatesForDisplay = StringUtil.ExtractParameter("IncludeTemplatesForDisplay", this.Source).Trim();
				this.ExcludeTemplatesForDisplay = StringUtil.ExtractParameter("ExcludeTemplatesForDisplay", this.Source).Trim();
				this.ExcludeItemsForDisplay = StringUtil.ExtractParameter("ExcludeItemsForDisplay", this.Source).Trim();
				this.IncludeItemsForDisplay = StringUtil.ExtractParameter("IncludeItemsForDisplay", this.Source).Trim();
				this.AllowMultipleSelection = string.Compare(StringUtil.ExtractParameter("AllowMultipleSelection", this.Source).Trim().ToLower(), "yes", StringComparison.OrdinalIgnoreCase) == 0;
				this.DataSource = StringUtil.ExtractParameter("DataSource", this.Source).Trim().ToLower();
				this.DatabaseName = StringUtil.ExtractParameter("databasename", this.Source).Trim().ToLower();
			}
			else
				this.DataSource = this.Source;
		}

		protected string FormTemplateFilterForDisplay()
		{
			if (string.IsNullOrEmpty(this.IncludeTemplatesForDisplay) && string.IsNullOrEmpty(this.ExcludeTemplatesForDisplay) && (string.IsNullOrEmpty(this.IncludeItemsForDisplay) && string.IsNullOrEmpty(this.ExcludeItemsForDisplay)))
				return string.Empty;
			string str1 = string.Empty;
			string str2 = ("," + this.IncludeTemplatesForDisplay + ",").ToLower();
			string str3 = ("," + this.ExcludeTemplatesForDisplay + ",").ToLower();
			string str4 = "," + this.IncludeItemsForDisplay + ",";
			string str5 = "," + this.ExcludeItemsForDisplay + ",";
			if (!string.IsNullOrEmpty(this.IncludeTemplatesForDisplay))
			{
				if (!string.IsNullOrEmpty(str1))
					str1 = str1 + " and ";
				str1 = str1 + string.Format("(contains('{0}', ',' + @@templateid + ',') or contains('{0}', ',' + @@templatekey + ','))", (object)str2);
			}
			if (!string.IsNullOrEmpty(this.ExcludeTemplatesForDisplay))
			{
				if (!string.IsNullOrEmpty(str1))
					str1 = str1 + " and ";
				str1 = str1 + string.Format("not (contains('{0}', ',' + @@templateid + ',') or contains('{0}', ',' + @@templatekey + ','))", (object)str3);
			}
			if (!string.IsNullOrEmpty(this.IncludeItemsForDisplay))
			{
				if (!string.IsNullOrEmpty(str1))
					str1 = str1 + " and ";
				str1 = str1 + string.Format("(contains('{0}', ',' + @@id + ',') or contains('{0}', ',' + @@key + ','))", (object)str4);
			}
			if (!string.IsNullOrEmpty(this.ExcludeItemsForDisplay))
			{
				if (!string.IsNullOrEmpty(str1))
					str1 = str1 + " and ";
				str1 = str1 + string.Format("not (contains('{0}', ',' + @@id + ',') or contains('{0}', ',' + @@key + ','))", (object)str5);
			}
			return str1;
		}

	}
}