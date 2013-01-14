using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FieldSuite.Controls.GeneralLinks;
using FieldSuite.Controls.ListItem;
using FieldSuite.FieldGutter;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Resources;
using Velir.SitecoreLibrary.Extensions;

namespace FieldSuite.Controls.ListItem
{
	public class GeneralLinksListItem : FieldSuiteListItem
	{
		/// <summary>
		/// Renders the list item as Html
		/// </summary>
		/// <param name="linkItem"></param>
		/// <param name="fieldId"></param>
		/// <param name="useFieldGutter"></param>
		/// <returns></returns>
		public virtual string RenderGeneralLink(GeneralLinkItem linkItem, string fieldId, bool useFieldGutter)
		{
			//disable items if the form is read only)
			if (ReadOnly)
			{
				ItemClick = string.Empty;
				ButtonClick = string.Empty;
			}

			string templateName = string.Empty;
			string icon = string.Empty;
			Item item = null;
			if (!string.IsNullOrEmpty(linkItem.Id) && ID.IsID(linkItem.Id))
			{
				Database db = Database.GetDatabase("master");
				if (db != null)
				{
					item = db.GetItem(linkItem.Id);
					if (item.IsNotNull())
					{
						icon = item.Appearance.Icon;

						TemplateItem template = item.Template;
						if (!string.IsNullOrEmpty(template.Name))
						{
							templateName = template.Name;
						}
					}
				}
			}

			//external link
			if (linkItem.LinkType == GeneralLinkItem.ExternalLinkType)
			{
				templateName = GeneralLinkItem.ExternalLinkType;
				icon = GeneralLinkItem.ExternalLinkIcon;
			}

			//javascript link
			if (linkItem.LinkType == GeneralLinkItem.JavascriptLinkType)
			{
				templateName = GeneralLinkItem.JavascriptLinkType;
				icon = GeneralLinkItem.JavascriptLinkIcon;
			}

			//mail link
			if (linkItem.LinkType == GeneralLinkItem.MailLinkType)
			{
				templateName = GeneralLinkItem.MailLinkType;
				icon = GeneralLinkItem.MailLinkIcon;
			}

			//anchor link
			if (linkItem.LinkType == GeneralLinkItem.AnchorLinkType)
			{
				templateName = GeneralLinkItem.AnchorLinkType;
				icon = GeneralLinkItem.AnchorLinkIcon;
			}

			string fieldGutterHtml = string.Format("<div class=\"fieldGutter\">{0}</div>", Images.GetSpacer(16, 16));
			if (useFieldGutter)
			{
				if (item.IsNotNull())
				{
					IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
					if (fieldGutterProcessor != null)
					{
						string html = fieldGutterProcessor.Process(new FieldGutterArgs(item, fieldId));
						if (!string.IsNullOrEmpty(html))
						{
							fieldGutterHtml = html;
						}
					}
				}
			}

			return string.Format(HtmlTemplate,
				linkItem.LinkId,
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

		private string AddRemoveHtml(string clickEvent, string innerAnchorHtml)
		{
			if (!ShowAddRemoveButton)
			{
				return string.Empty;
			}

			return string.Format("<div class=\"addRemoveButton\"><a onclick=\"{0}return false;\" href=\"#\">{1}</a></div>", clickEvent, innerAnchorHtml);
		}

		/// <summary>
		/// Html Template
		/// </summary>
		protected override string HtmlTemplate
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
