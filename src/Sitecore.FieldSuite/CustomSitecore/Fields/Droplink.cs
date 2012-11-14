using System.Web.UI;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Data.Items;
using Sitecore.Resources;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.SharedSource.FieldSuite.FieldGutter;

namespace Sitecore.SharedSource.FieldSuite.CustomSitecore.Fields
{
	public class Droplink : AFieldSuiteField
	{
		/// <summary>
		/// Current Field Type
		/// </summary>
		public override Item FieldTypeItem
		{
			get
			{
				string id = "{EBB32C7A-5CF1-4977-B084-384B5C5F77F2}";
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

		protected override void Render(HtmlTextWriter output)
		{
			Assert.ArgumentNotNull(output, "output");

			string disabled = string.Empty;
			if (this.ReadOnly)
			{
				disabled = " disabled=\"disabled\"";
			}
			output.Write("<input id=\"" + this.ID + "_Value\" type=\"hidden\" value=\"" + StringUtil.EscapeQuote(this.Value) + "\" />");

			Item[] items = this.GetItems(CurrentItem);

			output.Write("<div class=\"velirDroplink\">");
			output.Write(RenderItems(items, disabled));
			output.Write("</div>");
		}

		protected virtual string GetFieldGutterHtml()
		{
			if (string.IsNullOrEmpty(this.Value) || !Sitecore.Data.ID.IsID(this.Value))
			{
				return string.Format("<div id=\"{1}_fieldGutterDiv\" class=\"droplinkFieldGutterDiv\">{0}</div>", string.Empty, this.ID);
			}

			Item selectedItem = Sitecore.Context.ContentDatabase.GetItem(this.Value);
			if(selectedItem.IsNull())
			{
				return string.Format("<div id=\"{1}_fieldGutterDiv\" class=\"droplinkFieldGutterDiv\">{0}</div>", string.Empty, this.ID);
			}

			IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
			if (fieldGutterProcessor == null)
			{
				return string.Format("<div id=\"{1}_fieldGutterDiv\" class=\"droplinkFieldGutterDiv\">{0}</div>", string.Empty, this.ID);
			}

			string fieldGutterHtml = fieldGutterProcessor.Process(new FieldGutterArgs(selectedItem, this.ID));
			if(string.IsNullOrEmpty(fieldGutterHtml))
			{
				return string.Format("<div id=\"{1}_fieldGutterDiv\" class=\"droplinkFieldGutterDiv\">{0}</div>", string.Empty, this.ID);
			}

			return string.Format("<div id=\"{1}_fieldGutterDiv\" class=\"droplinkFieldGutterDiv\">{0}</div>", fieldGutterHtml, this.ID);
		}

		protected virtual string GetTemplateIconHtml()
		{
			string templateName = string.Empty;
			string imagePath = string.Empty;
			if (!string.IsNullOrEmpty(this.Value))
			{
				Item item = Sitecore.Context.ContentDatabase.GetItem(this.Value);
				if (item.IsNotNull() && item.Template != null && !string.IsNullOrEmpty(item.Template.Icon))
				{
					templateName = item.TemplateName;
					imagePath = Themes.MapTheme(item.Template.Icon);
				}
			}

			if(string.IsNullOrEmpty(imagePath))
			{
				imagePath = "/sitecore/images/blank.gif";
			}

			string tempateIconPath = string.Format("<img width=\"16\" height=\"16\" border=\"0\" align=\"middle\" alt=\"{1}\" style=\"margin:0px 4px 0px 0px\" id=\"{2}_templateIconImg\" src=\"{0}\"/>", imagePath, templateName, this.ID);
			return string.Format("<div id=\"{1}_templateIconDiv\" class=\"droplinkTemplateIconItem\">{0}</div>", tempateIconPath, this.ID);
		}

		protected virtual string RenderItems(Item[] items, string disabled)
		{
			string fieldGutterHtml = GetFieldGutterHtml();
			string templateIconHtml = GetTemplateIconHtml();

			//create dropdown
			string html = string.Format("<div class=\"droplinkWrapper\">{3}{4}<div class=\"droplinkSelectDiv\"><select id=\"{0}\" onchange=\"javascript:FieldSuite.Fields.Droplink.Select('{0}');return false\" {1} {2}>", this.ID, string.Empty, disabled, fieldGutterHtml, templateIconHtml);
			
			//add empty value
			html += "<option data_templateIcon=\"\" value=\"\"></option>";

			//add items to dropdown
			foreach (Item sourceItem in items)
			{
				if (sourceItem.IsNull())
				{
					continue;
				}

				html += RenderItem(sourceItem);
			}

			html +="</select></div></div>";
			return html;
		}

		/// <summary>
		/// Renders an item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		protected virtual string RenderItem(Item item)
		{
			string templateIcon = string.Empty;
			if(item.IsNotNull() && item.Template != null && !string.IsNullOrEmpty(item.Template.Icon))
			{
				templateIcon = Themes.MapTheme(item.Template.Icon);
			}

			bool isSelected = (this.Value == item.ID.ToString());
			return "<option data_templateIcon=\"" + templateIcon + "\" data_id=\"" + item.ID + "\" title=\"" + item.Paths.FullPath + "\" value=\"" + item.ID + "\"" + (isSelected ? " selected=\"selected\"" : string.Empty) + ">" + item.DisplayName + "</option>";
		}
	}
}