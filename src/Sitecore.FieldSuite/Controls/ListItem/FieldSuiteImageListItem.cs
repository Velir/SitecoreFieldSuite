using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Resources;

using Sitecore.Data.Items;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.SharedSource.FieldSuite.FieldGutter;

namespace Sitecore.SharedSource.FieldSuite.Controls.ListItem
{
	public class FieldSuiteImageListItem : FieldSuiteListItem
	{
		/// <summary>
		/// Renders the list item as Html
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		/// <param name="useFieldGutter"></param>
		/// <returns></returns>
		public override string Render(string itemId, string fieldId, bool useFieldGutter)
		{
			//get item from sitecore
			Item item = Sitecore.Context.ContentDatabase.GetItem(itemId);
			if (item.IsNull())
			{
				return RenderItemNotFound(itemId, fieldId);
			}

			return Render(item, itemId, fieldId, useFieldGutter);
		}

		/// <summary>
		/// Renders the list item as Html
		/// </summary>
		/// <param name="item"></param>
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		/// <param name="useFieldGutter"></param>
		/// <returns></returns>
		public override string Render(Item item, string itemId, string fieldId, bool useFieldGutter)
		{
			//defaults
			string icon = "/sitecore modules/shell/field suite/document_error.png";

			//check item
			if (item.IsNull())
			{
				return RenderItemNotFound(itemId, fieldId);
			}

			//disable items if the form is read only)
			if (ReadOnly)
			{
				ItemClick = string.Empty;
				ButtonClick = string.Empty;
			}

			string templateName = string.Empty;

			//set to items properties
			if (item.IsNotNull())
			{
				icon = item.Appearance.Icon;

				TemplateItem template = item.Template;
				if (!string.IsNullOrEmpty(template.Name))
				{
					templateName = template.Name;
				}
			}
			
			//check and retrieve parameters
			string imgElement = string.Empty;
			if (Parameters != null && Parameters.Count > 0)
			{
				imgElement = Parameters[0] as string;
			}

			string fieldGutterHtml = string.Empty;
			IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
			if (fieldGutterProcessor != null)
			{
				string html = fieldGutterProcessor.Process(new FieldGutterArgs(item, fieldId));
				if (!string.IsNullOrEmpty(html))
				{
					fieldGutterHtml = html;
				}
			}

			return string.Format(HtmlTemplate, 
				item.ID,
				fieldGutterHtml, 
				imgElement, 
				templateName, 
				Images.GetImage(icon, 0x10, 0x10, "absmiddle", "0px 4px 0px 0px", templateName), 
				Text, 
				HoverText, 
				ItemClick,
				SelectedClass);
		}

		/// <summary>
		/// Renders the list item as Html
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		public override string Render(string itemId, string fieldId)
		{
			return Render(itemId, fieldId, true);
		}

		/// <summary>
		/// Renders the item not found list item as Html
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		/// <returns></returns>
		public override string RenderItemNotFound(string itemId, string fieldId)
		{
			string displayText = string.Format("The item could not be retrieved from Sitecore. Id: {0}", itemId);

			//setup description
			string title = displayText;
			if (title.Length > 21)
			{
				title = title.Substring(0, 21) + "...";
			}

			string fieldGutterHtml = string.Empty;
			IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
			if (fieldGutterProcessor != null)
			{
				string html = fieldGutterProcessor.Process(null);
				if (!string.IsNullOrEmpty(html))
				{
					fieldGutterHtml = html;
				}
			}

			return string.Format(HtmlTemplate,
				itemId,
				fieldGutterHtml,
				"<img border=\"0\" src=\"/sitecore modules/shell/field suite/images/unknown.png?w=125&amp;h=125&amp;thn=true&amp;db=master\">",
				displayText,
				Images.GetImage("/sitecore modules/shell/field suite/images/bullet_ball_red.png", 0x10, 0x10, "absmiddle", "0px 4px 0px 0px", displayText),
				title,
				displayText,
				string.Format("FieldSuite.Fields.ImagesField.ToggleItem(this, '{0}');", fieldId),
				SelectedClass);
		}

		/// <summary>
		/// Html Template
		/// </summary>
		protected override string HtmlTemplate
		{
			get
			{
				return "<div data_id=\"{0}\" data_fieldid=\"query:/sitecore/content/Home/Contacts\" class=\"rotatingImageWrapper\">" +
							"<div>" +
								"<a onclick=\"{7}return false\" href=\"#\" title=\"{6}\" class=\"{8}\">{2}</a>" + 
							"</div>" +
							"<div class=\"rotatingImageDescription\">" +
								"{1}" +
								"<span title=\"{3}\">{4}</span>" +
								"{5}" +
							"</div>" +
						"</div>";
			}
		}

		/// <summary>
		/// Renders that this item is not configured for the Velir Images Field
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		/// <returns></returns>
		public string RenderItemConfigured(string itemId, string fieldId)
		{
			const string displayText = "This item's template is not configured for this field.";

			//setup description
			string title = displayText;
			if (title.Length > 21)
			{
				title = title.Substring(0, 21) + "...";
			}

			string fieldGutterHtml = string.Empty;
			IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
			if (fieldGutterProcessor != null)
			{
				string html = fieldGutterProcessor.Process(null);
				if (!string.IsNullOrEmpty(html))
				{
					fieldGutterHtml = html;
				}
			}

			return string.Format(HtmlTemplate,
				itemId,
				fieldGutterHtml,
				"<img border=\"0\" src=\"/sitecore modules/shell/field suite/images/unknown.png?w=125&amp;h=125&amp;thn=true&amp;db=master\">",
				displayText,
				Images.GetImage("/sitecore modules/shell/field suite/images/bullet_ball_red.png", 0x10, 0x10, "absmiddle", "0px 4px 0px 0px", displayText),
				title,
				displayText,
				string.Format("FieldSuite.Fields.ImagesField.ToggleItem(this, '{0}');", fieldId),
				SelectedClass);
		}
	}
}

// 0 - ItemId
// 1 - Field Gutter
// 2 - Main Image
// 3 - Template Name
// 4 - Template Image
// 5 - Item Name
// 6 - Title Text
// 7 - Item Click
// 8 - Selected Class