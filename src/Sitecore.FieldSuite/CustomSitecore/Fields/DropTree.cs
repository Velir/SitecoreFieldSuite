using System.Web;
using System.Web.UI;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.SharedSource.FieldSuite.FieldGutter;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.FieldSuite.CustomSitecore.Fields
{
	public class DropTree : Tree
	{
		/// <summary>
		/// Called when the data context changed.
		/// </summary>
		/// <param name="dataContext">The data context.</param>
		protected override void OnDataContextChanged(DataContext dataContext)
		{
			Assert.ArgumentNotNull(dataContext, "dataContext");
			if (Sitecore.Context.ClientPage.IsEvent)
			{
				string newValue = string.Empty;
				Item folder = dataContext.GetFolder();
				if (folder != null)
				{
					this.Value = folder.ID.ToString();
					newValue = this.Value;
				}

				//update field gutter
				string fieldGutterHtml = string.Empty;
				string templateIconPath = "/sitecore/images/blank.gif";

				string fieldId = this.ID.Replace("_holder", string.Empty);
				string id = this.Value;
				if (string.IsNullOrEmpty(id))
				{
					SheerResponse.Eval("FieldSuite.Fields.UpdateFieldGutter(\"" + fieldId + "\",\"" + fieldGutterHtml + "\")");
					SheerResponse.Eval("FieldSuite.Fields.UpdateTemplateIcon(\"" + fieldId + "\",\"" + templateIconPath + "\")");
					return;
				}

				if (folder.IsNull())
				{
					SheerResponse.Eval("FieldSuite.Fields.UpdateFieldGutter(\"" + fieldId + "\",\"" + fieldGutterHtml + "\")");
					SheerResponse.Eval("FieldSuite.Fields.UpdateTemplateIcon(\"" + fieldId + "\",\"" + templateIconPath + "\")");
					return;
				}

				fieldGutterHtml = GetFieldGutterHtml(folder, fieldId);

				Item item = null;
				if(!string.IsNullOrEmpty(this.Value))
				{
					item = Sitecore.Context.ContentDatabase.GetItem(this.Value);
					if(item.IsNotNull())
					{
						templateIconPath = Themes.MapTheme(item.Template.Icon);
					}
				}

				SheerResponse.Eval("FieldSuite.Fields.UpdateFieldGutter(\"" + fieldId + "\",\"" + HttpUtility.HtmlEncode(fieldGutterHtml) + "\")");
				SheerResponse.Eval("FieldSuite.Fields.UpdateTemplateIcon(\"" + fieldId + "\",\"" + templateIconPath + "\")");
				SheerResponse.Eval("FieldSuite.Fields.UpdateFieldValue(\"" + fieldId + "\",\"" + newValue + "\")");
			}
		}

		/// <summary>
		/// Renders the control.
		/// </summary>
		/// <param name="output">The output.</param>
		protected override void DoRender(HtmlTextWriter output)
		{
			Assert.ArgumentNotNull(output, "output");
			base.SetWidthAndHeightAttributes();
			string fieldGutterHtml = GetFieldGutterHtml(this.ID);
			string templateIconHtml = GetTemplateIconHtml(this.ID);

			string iD = this.ID;
			this.ID = iD + "_holder";
			output.Write("<input id=\"" + iD + "_Value\" type=\"hidden\" value=\"" + StringUtil.EscapeQuote(this.Value) + "\" />");

			output.Write("<div border=\"0\">");
			output.Write(string.Format("<div style=\"float:left;\">{0}</div>", fieldGutterHtml));
			output.Write(string.Format("<div style=\"float:left;\">{0}</div>", templateIconHtml));
			string str2 = (this.SelectOnly || this.Disabled) ? " readonly" : string.Empty;
			string str3 = this.SelectOnly ? (" onclick=\"" + Sitecore.Context.ClientPage.GetClientEvent(iD + ".Click") + "\"") : string.Empty;

			output.Write("<div style=\"float:left;width:75%;\"><table" + base.ControlAttributes + ">");
			output.Write("<tr>");
			output.Write("<td><input id=\"" + iD + "\" class=\"scComboboxEdit\" type=\"text\"" + str2 + " onmouseout=\"javascript:return false;\" onmouseover=\"javascript:return false;\" value=\"" + StringUtil.EscapeQuote(this.GetDisplayValue()) + "\"" + str3 + "/>");
			ImageBuilder builder = new ImageBuilder
			{
				Src = this.Glyph,
				Class = "scComboboxDropDown",
				Width = 13,
				Height = 0x10,
				Align = "absmiddle"
			};

			builder.Float = "right";

			if (!this.Disabled)
			{
				builder.RollOver = true;
				builder.OnClick = "javascript:return scForm.postEvent(this, event, '" + iD + ".Click')";
			}
			output.Write("</td><td>");
			output.Write(builder);
			output.Write("</td></tr></table></div></div>");
			this.ID = iD;
		}

		private string GetTemplateIconHtml(string fieldId)
		{
			if(string.IsNullOrEmpty(this.Value))
			{
				return GetTemplateIconHtml(null, fieldId);
			}

			Item item = Sitecore.Context.ContentDatabase.GetItem(this.Value);
			if(item.IsNull())
			{
				return GetTemplateIconHtml(null, fieldId);
			}

			return GetTemplateIconHtml(item, fieldId);
		}

		private string GetTemplateIconHtml(Item item, string fieldId)
		{
			string templateName = string.Empty;
			string imagePath = string.Empty;
			if (item.IsNotNull() && item.Template != null && !string.IsNullOrEmpty(item.Template.Icon))
			{
				templateName = item.TemplateName;
				imagePath = Themes.MapTheme(item.Template.Icon);
			}
			else
			{
				imagePath = "/sitecore/images/blank.gif";
			}

			string tempateIconPath = string.Format("<img width=\"16\" height=\"16\" border=\"0\" align=\"middle\" alt=\"{1}\" style=\"margin:0px 4px 0px 0px\" id=\"{2}_templateIconImg\" src=\"{0}\"/>", imagePath, templateName, fieldId);
			return string.Format("<div id=\"{1}_templateIconDiv\" class=\"droplinkTemplateIconItem\">{0}</div>", tempateIconPath, fieldId);
		}

		/// <summary>
		/// Gets the control attributes.
		/// </summary>
		/// <returns/>
		protected override string GetControlAttributes()
		{
			this.BuildFont();
			return this.RenderControlAttributes();
		}

		/// <summary>
		/// Called when the combobox has changed.
		/// </summary>
		protected override void Changed()
		{
			string testing = Sitecore.Context.GetSiteName();
			base.Changed();
		}

		/// <summary>
		/// Handles a click event.
		/// </summary>
		protected override void Click()
		{
			string testing = Sitecore.Context.GetSiteName();
			base.Click();
		}

		protected virtual string GetFieldGutterHtml(string fieldId)
		{
			if (string.IsNullOrEmpty(this.Value) || !Sitecore.Data.ID.IsID(this.Value))
			{
				return string.Format("<div id=\"{1}_fieldGutterDiv\" class=\"droplinkFieldGutterDiv\">{0}</div>", string.Empty, fieldId);
			}

			Item selectedItem = Sitecore.Context.ContentDatabase.GetItem(this.Value);
			if (selectedItem.IsNull())
			{
				return string.Format("<div id=\"{1}_fieldGutterDiv\" class=\"droplinkFieldGutterDiv\">{0}</div>", string.Empty, fieldId);
			}

			return GetFieldGutterHtml(selectedItem, fieldId);
		}

		protected virtual string GetFieldGutterHtml(Item item, string fieldId)
		{
			if (item.IsNull())
			{
				return string.Format("<div id=\"{1}_fieldGutterDiv\" class=\"droplinkFieldGutterDiv\">{0}</div>", string.Empty, fieldId);
			}

			IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
			if (fieldGutterProcessor == null)
			{
				return string.Format("<div id=\"{1}_fieldGutterDiv\" class=\"droplinkFieldGutterDiv\">{0}</div>", string.Empty, fieldId);
			}

			string fieldGutterHtml = fieldGutterProcessor.Process(new FieldGutterArgs(item, fieldId));
			if (string.IsNullOrEmpty(fieldGutterHtml))
			{
				return string.Format("<div id=\"{1}_fieldGutterDiv\" class=\"droplinkFieldGutterDiv\">{0}</div>", string.Empty, fieldId);
			}

			return string.Format("<div id=\"{1}_fieldGutterDiv\" class=\"droplinkFieldGutterDiv\">{0}</div>", fieldGutterHtml, fieldId);
		}
	}
}
