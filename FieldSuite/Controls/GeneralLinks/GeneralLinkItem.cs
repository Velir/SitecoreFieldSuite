using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace FieldSuite.Controls.GeneralLinks
{
	[Serializable]
	[XmlRoot(ElementName = "link")]
	public class GeneralLinkItem
	{
        public const string DefaultLinkText = "Set Link Text";
		public const string ExternalLinkType = "external";
        public const string ExternalLinkIcon = "/sitecore modules/shell/field suite/images/externalLink.png";
        public const string JavascriptLinkType = "javascript";
        public const string JavascriptLinkIcon = "/sitecore modules/shell/field suite/images/code_javascript.png";
        public const string MailLinkType = "mailto";
        public const string MailLinkIcon = "/sitecore modules/shell/field suite/images/mail.png";
        public const string AnchorLinkType = "anchor";
        public const string AnchorLinkIcon = "/sitecore modules/shell/field suite/images/application.png";
        public const string MediaLinkType = "media";
        public const string InternalLinkType = "internal";
		
		[XmlIgnore()]
		[NonSerialized]
		private string _linkText = DefaultLinkText;

		[XmlAttribute("id")]
		public string Id { get; set; }

		[XmlAttribute("linkid")]
		public string LinkId { get; set; }

		[XmlAttribute("text")]
		public string Text { get; set; }

		[XmlAttribute("linktype")]
		public string LinkType { get; set; }
		
		[XmlAttribute("url")]
		public string Url { get; set; }

		[XmlAttribute("target")]
		public string Target { get; set; }

		[XmlAttribute("anchor")]
		public string Anchor { get; set; }

		[XmlAttribute("cssclass")]
		public string CssClass { get; set; }

		[XmlAttribute("title")]
		public string Title { get; set; }

		[XmlAttribute("querystring")]
		public string QueryString { get; set; }

		[XmlAttribute("linktext")]
		public string LinkText { get { return _linkText; } set { _linkText = value; } }

	}
}
