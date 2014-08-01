using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Resources;

using Sitecore.Data.Items;
using Sitecore.SharedSource.Commons.Extensions;
using FieldSuite.FieldGutter;

namespace FieldSuite.Controls.ListItem
{
	public class FieldSuiteListItem : IFieldSuiteListItem
	{
		/// <summary>
		/// Text of the List Item
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Text to be shown during hover
		/// </summary>
		public string HoverText { get; set; }

		/// <summary>
		/// Css class to use when the item is selected
		/// </summary>
		public string SelectedClass { get; set; }

		/// <summary>
		/// Click event for when the Add/Remove button is clicked
		/// </summary>
		public string ButtonClick { get; set; }

		/// <summary>
		/// Click event for when the item is clicked
		/// </summary>
		public string ItemClick { get; set; }

		/// <summary>
		/// Optional parameters that can be used
		/// </summary>
		public List<object> Parameters { get;set; }

		/// <summary>
		/// Is this list item read only
		/// </summary>
		public bool ReadOnly { get; set; }

		/// <summary>
		/// Is this list item read only
		/// </summary>
		public bool ShowAddRemoveButton { get; set; }

		/// <summary>
		/// Renders the list item as Html
		/// </summary>
		/// <param name="item"></param>
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		/// <param name="useFieldGutter"></param>
		/// <returns></returns>
		public virtual string Render(Item item, string itemId, string fieldId, bool useFieldGutter)
		{
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

			//defaults
			string icon = "/sitecore modules/shell/field suite/document_error.png";

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

			string fieldGutterHtml = string.Empty;
			if (useFieldGutter)
			{
				IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
				if (fieldGutterProcessor != null)
				{
					string html = fieldGutterProcessor.Process(new FieldGutterArgs(item, fieldId));
					if(!string.IsNullOrEmpty(html))
					{
						fieldGutterHtml = html;
					}
				}
			}

			return string.Format(HtmlTemplate,
				item.ID,
				fieldGutterHtml,
				fieldId,
				AddRemoveHtml(ButtonClick, Images.GetSpacer(16, 16)),
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
		/// <param name="useFieldGutter"></param>
		/// <returns></returns>
		public virtual string Render(string itemId, string fieldId, bool useFieldGutter)
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
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		public virtual string Render(string itemId, string fieldId)
		{
			return Render(itemId, fieldId, true);
		}

		/// <summary>
		/// Renders the item not found list item as Html
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="fieldId"></param>
		/// <returns></returns>
		public virtual string RenderItemNotFound(string itemId, string fieldId)
		{
			string displayText = string.Format("The item could not be retrieved from Sitecore. Id: {0}", itemId);

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
				fieldId,
				AddRemoveHtml(string.Format("FieldSuite.Fields.RemoveItem('{0}', '{1}');", fieldId, itemId), Images.GetSpacer(16, 16)),
				displayText,
				Images.GetImage("/sitecore modules/shell/field suite/images/bullet_ball_red.png", 0x10, 0x10, "absmiddle", "0px 4px 0px 0px", displayText),
				displayText,
				displayText,
				ItemClick,
				SelectedClass);
		}

		private string AddRemoveHtml(string clickEvent, string innerAnchorHtml)
		{
			if (!ShowAddRemoveButton)
			{
				return string.Empty;
			}

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("<div class=\"addRemoveButton\"><a onclick=\"");
			stringBuilder.Append(clickEvent);
			stringBuilder.Append("return false;\" href=\"#\">");
			stringBuilder.Append(innerAnchorHtml);
			stringBuilder.Append("</a></div>");
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Html Template
		/// </summary>
		protected virtual string HtmlTemplate
		{
			get
			{
				return "<div data_id=\"{0}\" class=\"velirItem\">" + 
							"{1}" + 
							"<div class=\"{9}\">" + 
								"<a data_fieldid=\"{2}\" title=\"{7}\" href=\"#\" onclick=\"{8}return false;\">" +
									"<span title=\"{4}\">{5}</span>" +
									"<span>{6}</span>" +
								"</a>" +
							"</div>" +
							"{3}" +
						"</div>";
			}
		}
	}
}

// 0 - ItemId
// 1 - Field Gutter
// 2 - Field Id
// 3 - Button Click
// 4 - Blank Image
// 5 - Template Name
// 6 - Template Image
// 7 - Item Name
// 8 - Title Text
// 9 - Item Click
// 10 - Selected Class