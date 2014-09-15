using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Linq;
using Sitecore.SharedSource.FieldSuite.Controls.GeneralLinks;
using Sitecore.SharedSource.FieldSuite.Controls.ListItem;
using Sitecore.SharedSource.FieldSuite.FieldGutter;
using Sitecore.SharedSource.FieldSuite.Util;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Resources.Media;
using Sitecore.Links;

namespace Sitecore.SharedSource.FieldSuite.Types
{
	public class GeneralLinks : AFieldSuiteField
	{
		private List<GeneralLinkItem> _linkItems;

		private Int32 _renderItemCount;
		public Int32 RenderItemCount
		{
			get { return _renderItemCount; }
			set { _renderItemCount = value; }
		}

		private Int32 _renderSelectedItemCount;
		public Int32 RenderSelectedItemCount
		{
			get { return _renderSelectedItemCount; }
			set { _renderSelectedItemCount = value; }
		}

		public GeneralLinks()
		{
		}

        public GeneralLinks(Item item, string fieldName)
        {
            this.Value = item.Fields[fieldName].ToString();
        }

		public GeneralLinks(Item item, TextField field)
            : this(item, field.InnerField.Name)
		{
			
		}

		public override void HandleMessage(Message message)
		{
			Assert.ArgumentNotNull(message, "message");
			base.HandleMessage(message);
			if (message["id"] == ID)
			{
				NameValueCollection additionalParameters = new NameValueCollection();
				if (message["link"] != null && !string.IsNullOrEmpty(message["link"]))
				{
					additionalParameters.Add("link", message["link"]);
				}

				switch (message.Name)
				{
					case "contentlink:internallink":
						Insert("/sitecore/shell/Applications/Dialogs/Internal link.aspx", additionalParameters);
						return;

					case "contentlink:media":
						{
							additionalParameters.Add("umwn", "1");
							Insert("/sitecore/shell/Applications/Dialogs/Media link.aspx", additionalParameters);
							return;
						}
					case "contentlink:externallink":
						Insert("/sitecore/shell/Applications/Dialogs/External link.aspx", additionalParameters);
						return;

					case "contentlink:anchorlink":
						Insert("/sitecore/shell/Applications/Dialogs/Anchor link.aspx", additionalParameters);
						return;

					case "contentlink:mailto":
						Insert("/sitecore/shell/Applications/Dialogs/Mail link.aspx", additionalParameters);
						return;

					case "contentlink:javascript":
						Insert("/sitecore/shell/Applications/Dialogs/Javascript link.aspx", additionalParameters);
						return;

					case "contentlink:setlinktext":
						SetLinkText(additionalParameters);
						return;
				}
			}
		}

		public virtual List<GeneralLinkItem> LinkItems
		{
			get
			{
				if (_linkItems != null && _linkItems.Count > 0)
				{
					return _linkItems;
				}

				if (Value == null || string.IsNullOrEmpty(Value))
				{
					return new List<GeneralLinkItem>();
				}

				TextReader textReader = new StringReader(Value);

				XmlDocument document = new XmlDocument();
				document.Load(textReader);

				XDocument xDocument = XDocument.Load(document.CreateNavigator().ReadSubtree());

				IEnumerable<XElement> items = xDocument.Descendants("link").ToList();
				if (!items.Any())
				{
					return new List<GeneralLinkItem>();
				}

				_linkItems = new List<GeneralLinkItem>();
				foreach (XElement node in items)
				{
					if (node == null)
					{
						continue;
					}

					//deserialize object
					object obj = XmlUtil.XmlDeserializeFromString(node.ToString(), typeof(GeneralLinkItem));
					if (obj == null)
					{
						continue;
					}

                    //update the url for internal items

                    GeneralLinkItem generalLinkItem = (GeneralLinkItem)obj;

                    if (!string.IsNullOrEmpty(generalLinkItem.Id))
                    {
                        ID linkItemId = new ID(generalLinkItem.Id);
                        Item item = Sitecore.Context.Database.GetItem(linkItemId);

                        if (item != null)
                        {
                            switch (generalLinkItem.LinkType)
                            {
                                case GeneralLinkItem.InternalLinkType:
                                    UrlOptions urlOptions = LinkManager.GetDefaultUrlOptions();
                                    urlOptions.AlwaysIncludeServerUrl = true;
                                    generalLinkItem.Url = LinkManager.GetItemUrl(item, urlOptions);
                                    break;

                                case GeneralLinkItem.MediaLinkType:
                                    generalLinkItem.Url = MediaManager.GetMediaUrl(item);
                                    break;
                            }
                        }
                    }

					_linkItems.Add(generalLinkItem);
				}

				return _linkItems;
			}
			set { _linkItems = value; }
		}

		/// <summary>
		/// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
		/// </summary>
		/// <param name="output">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
		protected override void DoRender(HtmlTextWriter output)
		{
			//iterate over images
			string html = string.Empty;

			html += string.Format("<div id=\"{0}_SelectedItems\" class=\"selectedItems\">", this.ID);
			html += RenderItems(LinkItems);
			html += "</div>";

			//write back to output for rendering
			string hiddenField = "<input id=\"" + this.ID + "_Value\" type=\"hidden\" value=\"" + StringUtil.EscapeQuote(this.Value) + "\" />";
			output.Write(string.Format("<div id=\"{1}\" class=\"scContentControlHtml\" style=\"\">{2}<div class=\"generalLinkItems\">{0}</div></div>", html, this.ID, hiddenField));
		}

		protected string RenderItems(List<GeneralLinkItem> linkItems)
		{
			StringBuilder sb = new StringBuilder();
			foreach (GeneralLinkItem linkItem in LinkItems)
			{
				if (linkItem == null)
				{
					continue;
				}

				sb.Append(this.RenderItem(linkItem));
			}

			return sb.ToString();
		}

		protected string RenderItem(GeneralLinkItem linkItem)
		{
			GeneralLinksListItem listItem = new GeneralLinksListItem();
			bool useFieldGutter = false;

			listItem.ShowAddRemoveButton = true;
			listItem.ButtonClick = string.Format("FieldSuite.Fields.GeneralLinks.RemoveItem(this, '{0}');", this.ID);
			listItem.ItemClick = "FieldSuite.Fields.SelectItem(this);";
			listItem.SelectedClass = "velirFieldSelected";
			listItem.ReadOnly = this.ReadOnly;
			listItem.Text = linkItem.LinkText;

			listItem.HoverText = linkItem.Url;
			if (!string.IsNullOrEmpty(linkItem.Id) && Sitecore.Data.ID.IsID(linkItem.Id))
			{
				Database db = Sitecore.Data.Database.GetDatabase("master");
				if (db != null)
				{
					Item item = db.GetItem(linkItem.Id);
					if (item.IsNotNull())
					{
						listItem.HoverText = item.Paths.FullPath;
						useFieldGutter = true;

						//for performance reason limit field gutter
						IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
						if (fieldGutterProcessor != null)
						{
							Int32 maxCount = fieldGutterProcessor.MaxCount;
							if (maxCount != 0 && RenderItemCount <= maxCount)
							{
								RenderItemCount++;
							}
						}
					}
				}
			}

			return listItem.RenderGeneralLink(linkItem, this.ID, useFieldGutter);
		}

		/// <summary>
		/// Sets up the pipeline for the link text
		/// </summary>
		/// <param name="additionalParameters"></param>
		private void SetLinkText(NameValueCollection additionalParameters)
		{
			if (additionalParameters["link"] == null || string.IsNullOrEmpty(additionalParameters["link"]))
			{
				Sitecore.Context.ClientPage.ClientResponse.Alert("Please select an item");
				return;
			}

			Sitecore.Context.ClientPage.Start(this, "SetLinkTextPipeline", additionalParameters);
		}

		/// <summary>
		/// Pipeline for setting the link text
		/// </summary>
		/// <param name="args"></param>
		protected void SetLinkTextPipeline(ClientPipelineArgs args)
		{
			if (!args.IsPostBack)
			{
				Sitecore.Context.ClientPage.ClientResponse.Input("Enter a name: ", string.Empty);
				args.WaitForPostBack();
			}
			else
			{
				if (!args.HasResult || args.Result == null)
				{
					return;
				}

				GeneralLinkItem selectedLink = XmlUtil.XmlDeserializeFromString<GeneralLinkItem>(args.Parameters["link"]);
				if (selectedLink == null)
				{
					return;
				}

				int i = 0;
				while (i <= LinkItems.Count)
				{
					GeneralLinkItem linkItem = LinkItems[i];
					if (linkItem.LinkId == selectedLink.LinkId)
					{
						linkItem.LinkText = args.Result;
						LinkItems[i] = linkItem;

						//update new field value
						Value = string.Format("<links>{0}</links>", LinkItems.Aggregate(string.Empty, (current, link) => current + XmlUtil.XmlSerializeToString(link)));
						SetModified();
						break;
					}
					i++;
				}

				//set new value
				Sitecore.Context.ClientPage.ClientResponse.SetAttribute(ID + "_Value", "value", Value);

				//set new display
				Sitecore.Context.ClientPage.ClientResponse.SetInnerHtml(ID + "_SelectedItems", RenderItems(LinkItems));

				SheerResponse.Eval("scContent.startValidators()");
			}
		}

		protected void Insert(string url)
		{
			Assert.ArgumentNotNull(url, "url");
			Insert(url, new NameValueCollection());
		}

		protected void Insert(string url, NameValueCollection additionalParameters)
		{
			Assert.ArgumentNotNull(url, "url");
			Assert.ArgumentNotNull(additionalParameters, "additionalParameters");
			var values2 = new NameValueCollection();
			values2.Add("url", url);
			values2.Add(additionalParameters);
			NameValueCollection parameters = values2;
			Sitecore.Context.ClientPage.Start(this, "InsertLink", parameters);
		}

		protected void InsertLink(ClientPipelineArgs args)
		{
			Assert.ArgumentNotNull(args, "args");
			if (args.IsPostBack)
			{
				if (!string.IsNullOrEmpty(args.Result) && (args.Result != "undefined"))
				{
					GeneralLinkItem selectedLink = null;
					if (args.Parameters["link"] != null)
					{
						selectedLink = XmlUtil.XmlDeserializeFromString<GeneralLinkItem>(args.Parameters["link"]);
					}

					GeneralLinkItem returnedLink = XmlUtil.XmlDeserializeFromString<GeneralLinkItem>(args.Result);
					string rawValue = LinkItems.Aggregate(string.Empty, (current, link) => current + XmlUtil.XmlSerializeToString(link));

					if (selectedLink == null)
					{
						//item doesn't exist, add to values
						if (string.IsNullOrEmpty(returnedLink.LinkId))
						{
							returnedLink.LinkId = Guid.NewGuid().ToString();
						}
						rawValue += XmlUtil.XmlSerializeToString(returnedLink);
						LinkItems.Add(returnedLink);
					}
					else
					{
						//item exists, set in values
						int i = 0;
						while (i <= LinkItems.Count)
						{
							GeneralLinkItem linkItem = LinkItems[i];
							if (linkItem.LinkId == selectedLink.LinkId)
							{
								returnedLink.LinkId = selectedLink.LinkId;
								LinkItems[i] = returnedLink;
								rawValue = LinkItems.Aggregate(string.Empty, (current, link) => current + XmlUtil.XmlSerializeToString(link));
								break;
							}
							i++;
						}
					}

					Value = string.Format("<links>{0}</links>", rawValue);

					SetModified();

					//set new value
					Sitecore.Context.ClientPage.ClientResponse.SetAttribute(ID + "_Value", "value", Value);

					//set new display
					Sitecore.Context.ClientPage.ClientResponse.SetInnerHtml(ID + "_SelectedItems", RenderItems(LinkItems));

					SheerResponse.Eval("scContent.startValidators()");
				}
			}
			else
			{
				var str = new UrlString(args.Parameters["url"]);
				UrlHandle urlHandle = new UrlHandle();
				
				
				
				if (args.Parameters["link"] != null && !string.IsNullOrEmpty(args.Parameters["link"]))
				{
					str.Append("va", args.Parameters["link"]);
					urlHandle["va"] = args.Parameters["link"];
				}
				urlHandle.Add(str);

				str.Append("ro", Source);
				if ((UIUtil.IsIE() && (GetIEEngineBasedVersion() == 9)) && (args.Parameters["umwn"] == "1"))
				{
					ShowIEModelessDialog(str.ToString(), 500, 600);
				}
				else
				{
					Sitecore.Context.ClientPage.ClientResponse.ShowModalDialog(str.ToString(), true);
				}
				args.WaitForPostBack();
			}
		}

		// Explicit predicate delegate. 
		private static bool FindLinkPredicate(GeneralLinkItem item, string id)
		{
			if (item.Id == id)
			{
				return true;
			}
			return false;
		}

		protected static void ShowIEModelessDialog(string url, int width, int height)
		{
			Assert.ArgumentNotNull(url, "url");
			if (!UIUtil.IsIE())
			{
				throw new InvalidOperationException("Operation can be performed only for IE browser");
			}
			string str = url;
			try
			{
				UrlString str2 = new UrlString(url);
				str2["mdls"] = "1";
				str = str2.ToString();
			}
			catch
			{
				str = url;
			}
			SheerResponse.Eval("scForm.browser.closePopups('ShowModelessWindowCommand');");
			string str3 = "{scCalleeForm:scForm, scCalleePipeline:request.pipeline}";
			string str4 = string.Format("dialogWidth:{0}px;dialogHeight:{1}px;help:no;scroll:auto;resizable:yes;center:yes;status:no", width, height);
			SheerResponse.Eval(string.Format("window.showModelessDialog(\"{0}\", {1},\"{2}\");", str, str3, str4));
		}

		protected static int GetIEEngineBasedVersion()
		{
			if (!UIUtil.IsIE())
			{
				throw new InvalidOperationException("Operation can be performed only for IE browser");
			}
			HttpContext current = HttpContext.Current;
			if (current == null)
			{
				return -1;
			}
			string userAgent = current.Request.UserAgent;
			if (string.IsNullOrEmpty(userAgent))
			{
				return -1;
			}
			return GetIEEngineBasedVersion(userAgent);
		}

		protected static int GetIEEngineBasedVersion(string userAgent)
		{
			int num;
			Assert.ArgumentNotNull(userAgent, "userAgent");
			bool flag = true;
			Regex regex = new Regex(@"Trident/(?<version>\d+)", RegexOptions.IgnoreCase);
			MatchCollection matchs = regex.Matches(userAgent);
			if (matchs.Count == 0)
			{
				flag = false;
				matchs = new Regex(@"MSIE (?<version>\d+)").Matches(userAgent);
			}
			if (matchs[0].Groups["version"] == null)
			{
				return -1;
			}
			if (!int.TryParse(matchs[0].Groups["version"].Value, out num))
			{
				return -1;
			}
			int num2 = num;
			if (flag)
			{
				int num3 = 8;
				int num4 = 4;
				num2 += num3 - num4;
			}
			return num2;
		}
	}
}