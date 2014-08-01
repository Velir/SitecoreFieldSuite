using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using log4net;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Shell;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditor;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.SharedSource.Commons.Extensions;
using FieldSuite.Placeholders;
using Control = System.Web.UI.Control;
using Memo = Sitecore.Web.UI.HtmlControls.Memo;
using WebControl = System.Web.UI.WebControls.WebControl;
using FieldSuite.FieldSource;
using Sitecore.Web.UI.HtmlControls.Data;

namespace FieldSuite.Editors.ContentEditor
{
	public class FieldSuiteEditorFormatter
	{
		// Fields
		private RenderContentEditorArgs arguments;

		// Methods
		public void AddEditorControl(Control parent, Control editor,
		                             Sitecore.Shell.Applications.ContentManager.Editor.Field field, bool hasRibbon,
		                             bool readOnly, string value)
		{
			Assert.ArgumentNotNull(parent, "parent");
			Assert.ArgumentNotNull(editor, "editor");
			Assert.ArgumentNotNull(field, "field");
			Assert.ArgumentNotNull(value, "value");
			SetProperties(editor, field, readOnly);
			this.SetValue(editor, value);
			EditorFieldContainer container = new EditorFieldContainer(editor)
			                                 	{
			                                 		ID = field.ControlID + "_container"
			                                 	};
			Control control = container;
			Sitecore.Context.ClientPage.AddControl(parent, control);
			SetProperties(editor, field, readOnly);
			SetAttributes(editor, field, hasRibbon);
			SetStyle(editor, field);
			this.SetValue(editor, value);
		}

		public void AddLiteralControl(Control parent, string text)
		{
			Assert.ArgumentNotNull(parent, "parent");
			Assert.ArgumentNotNull(text, "text");
			if (parent.Controls.Count > 0)
			{
				Control control = parent.Controls[parent.Controls.Count - 1];
				LiteralControl control2 = control as LiteralControl;
				if (control2 != null)
				{
					control2.Text = control2.Text + text;
					return;
				}
			}
			Sitecore.Context.ClientPage.AddControl(parent, new LiteralControl(text));
		}

		public Control GetEditor(Item fieldType)
		{
			Assert.ArgumentNotNull(fieldType, "fieldType");
			if (!this.Arguments.ShowInputBoxes)
			{
				switch (fieldType.Name.ToLowerInvariant())
				{
					case "html":
					case "memo":
					case "rich text":
					case "security":
					case "multi-line text":
						return new Memo();
				}
				if (fieldType.Name == "password")
				{
					return new Password();
				}
				return new Text();
			}
			System.Web.UI.Control webControl = Resource.GetWebControl(fieldType["Control"]);
			if (webControl == null)
			{
				string str2 = fieldType["Assembly"];
				string str3 = fieldType["Class"];
				if (!string.IsNullOrEmpty(str2) && !string.IsNullOrEmpty(str3))
				{
					webControl = Sitecore.Reflection.ReflectionUtil.CreateObject(str2, str3, new object[0]) as Control;
				}
			}
			if (webControl == null)
			{
				webControl = new Text();
			}
			return webControl;
		}

		public Item GetFieldType(Field itemField)
		{
			Assert.ArgumentNotNull(itemField, "itemField");
			Item fieldTypeItem = FieldTypeManager.GetFieldTypeItem(StringUtil.GetString(new string[] {itemField.Type, "text"}));
			if (fieldTypeItem == null)
			{
				fieldTypeItem = FieldTypeManager.GetDefaultFieldTypeItem();
			}
			return fieldTypeItem;
		}

		public virtual void RenderField(Control parent, Sitecore.Shell.Applications.ContentManager.Editor.Field field,
		                                bool readOnly)
		{
			    Assert.ArgumentNotNull(parent, "parent");
			Assert.ArgumentNotNull(field, "field");
			Field itemField = field.ItemField;
			Item fieldType = this.GetFieldType(itemField);
			if (fieldType != null)
			{
				if (!itemField.CanWrite)
				{
					readOnly = true;
				}
				this.RenderMarkerBegin(parent, field.ControlID);
				this.RenderMenuButtons(parent, field, fieldType, readOnly);
				this.RenderLabel(parent, field, fieldType, readOnly);
				this.RenderField(parent, field, fieldType, readOnly);
				this.RenderMarkerEnd(parent);
			}
		}

		public virtual void RenderField(Control parent, Sitecore.Shell.Applications.ContentManager.Editor.Field field,
		                                Item fieldType, bool readOnly)
		{
			string str;
			Assert.ArgumentNotNull(parent, "parent");
			Assert.ArgumentNotNull(field, "field");
			Assert.ArgumentNotNull(fieldType, "fieldType");
			if (field.ItemField.IsBlobField && !this.Arguments.ShowInputBoxes)
			{
				readOnly = true;
				str = Translate.Text("[Blob Value]");
			}
			else
			{
				str = field.Value;
			}
			this.RenderField(parent, field, fieldType, readOnly, str);
		}

		public virtual void RenderField(Control parent, Sitecore.Shell.Applications.ContentManager.Editor.Field field,
		                                Item fieldType, bool readOnly, string value)
		{
			Assert.ArgumentNotNull(parent, "parent");
			Assert.ArgumentNotNull(field, "field");
			Assert.ArgumentNotNull(fieldType, "fieldType");
			Assert.ArgumentNotNull(value, "value");
			bool hasRibbon = false;
			string str = null;
			int count = 0;
			string text = string.Empty;
			Control editor = this.GetEditor(fieldType);
			if (this.Arguments.ShowInputBoxes)
			{
				ChildList children = fieldType.Children;
				hasRibbon = !UserOptions.ContentEditor.ShowRawValues && (children["Ribbon"] != null);
				switch (field.TemplateField.TypeKey)
				{
					case "rich text":
					case "html":
						hasRibbon = hasRibbon && (UserOptions.HtmlEditor.ContentEditorMode != UserOptions.HtmlEditor.Mode.Preview);
						break;
				}
				if (!UserOptions.ContentEditor.ShowRawValues)
				{
					Item item = children["Menu"];
					if (((item != null) && item.HasChildren) && UserOptions.View.UseSmartTags)
					{
						str = item.Children[0]["Display Name"];
						count = item.Children.Count;
					}
				}
			}
			string str4 = string.Empty;
			string str5 = string.Empty;
			int @int = Registry.GetInt("/Current_User/Content Editor/Field Size/" + field.TemplateField.ID.ToShortID(), -1);
			if (@int != -1)
			{
				str4 = string.Format(" height:{0}px", @int);

				Sitecore.Web.UI.HtmlControls.Control control2 = editor as Sitecore.Web.UI.HtmlControls.Control;
				if (control2 != null)
				{
					control2.Height = new Unit((double) @int, UnitType.Pixel);
				}
				else
				{
					WebControl control3 = editor as WebControl;
					if (control3 != null)
					{
						control3.Height = new Unit((double) @int, UnitType.Pixel);
					}
				}
			}
			else if (editor is Frame)
			{
				string style = field.ItemField.Style;
				if (string.IsNullOrEmpty(style) || !style.ToLowerInvariant().Contains("height"))
				{
					str5 = " class='defaultFieldEditorsFrameContainer'";
				}
			}
			if (!string.IsNullOrEmpty(str))
			{
				string str7 =
					string.Concat(new object[]
					              	{
					              		"<div onactivate='javascript:scContent.smartTag(this, event, \"", field.ControlID, "\", ",
					              		StringUtil.EscapeJavascriptString(str), ",\"", count,
					              		"\")' onmouseover='javascript:scContent.smartTag(this, event, \"", field.ControlID, "\", ",
					              		StringUtil.EscapeJavascriptString(str), ",\"", count, "\")'", str4, ">"
					              	});
				this.AddLiteralControl(parent, str7);
			}
			else if (UserOptions.View.UseSmartTags)
			{
				string str8 =
					"<div onmouseover=\"javascript:scContent.smartTag(this, event)\" onactivate=\"javascript:scContent.smartTag(this, event)\"" +
					str4 + ">";
				this.AddLiteralControl(parent, str8);
			}
			else
			{
				this.AddLiteralControl(parent, "<div style='overflow:hidden; " + str4 + "' " + str5 + ">");
			}
			this.AddLiteralControl(parent, text);
			this.AddEditorControl(parent, editor, field, hasRibbon, readOnly, value);
			this.AddLiteralControl(parent, "</div>");
			this.RenderResizable(parent, field);
		}

		public void RenderLabel(Control parent, Sitecore.Shell.Applications.ContentManager.Editor.Field field, Item fieldType,
		                        bool readOnly)
		{
			Assert.ArgumentNotNull(parent, "parent");
			Assert.ArgumentNotNull(field, "field");
			Assert.ArgumentNotNull(fieldType, "fieldType");
			Field itemField = field.ItemField;
			Language language = this.Arguments.Language;
			Assert.IsNotNull(language, "language");
			if (itemField.Language != language)
			{
				Item item = ItemManager.GetItem(field.ItemField.Item.ID, language, Sitecore.Data.Version.Latest,
				                                field.ItemField.Item.Database);
				if (item != null)
				{
					itemField = item.Fields[itemField.ID];
				}
			}
			string name = itemField.Name;
			if (!string.IsNullOrEmpty(itemField.DisplayName))
			{
				name = itemField.DisplayName;
			}
			name = Translate.Text(name);
			string toolTip = itemField.ToolTip;
			if (!string.IsNullOrEmpty(toolTip))
			{
				toolTip = Translate.Text(toolTip);
				if (toolTip.EndsWith("."))
				{
					toolTip = StringUtil.Left(toolTip, toolTip.Length - 1);
				}
				name = name + " - " + toolTip;
			}
			name = HttpUtility.HtmlEncode(name);
			bool separator = false;
			StringBuilder attributes = new StringBuilder(200);
			if (this.Arguments.IsAdministrator && (itemField.Unversioned || itemField.Shared))
			{
				attributes.Append("<span class=\"scEditorFieldLabelAdministrator\"> [");
				if (itemField.Unversioned)
				{
					attributes.Append(Translate.Text("unversioned"));
					separator = true;
				}
				if (itemField.Shared)
				{
					if (separator)
					{
						attributes.Append(", ");
					}
					attributes.Append(Translate.Text("shared"));
					separator = true;
				}
			}
			Field field3 = field.ItemField;
			Action<string> action = delegate(string text)
			                        	{
			                        		attributes.Append(separator ? ", " : "<span class=\"scEditorFieldLabelAdministrator\"> [");
			                        		attributes.Append(Translate.Text(text));
			                        	};
			if (field3.InheritsValueFromOtherItem)
			{
				action("original value");
			}
			else if (field3.ContainsStandardValue)
			{
				action("standard value");
			}
			if (attributes.Length > 0)
			{
				attributes.Append("]</span>");
			}
			name = name + attributes.ToString() + ":";
			if (readOnly)
			{
				name = "<span class=\"scEditorFieldLabelDisabled\">" + name + "</span>";
			}
			string helpLink = itemField.HelpLink;
			if (helpLink.Length > 0)
			{
				name = "<a class=\"scEditorFieldLabelLink\" href=\"" + helpLink + "\" target=\"__help\">" + name + "</a>";
			}
			string str4 = string.Empty;
			if (itemField.Description.Length > 0)
			{
				str4 = " title=\"" + itemField.Description + "\"";
			}
			string str5 = "scEditorFieldLabel";
			if ((UserOptions.View.UseSmartTags && !readOnly) && !UserOptions.ContentEditor.ShowRawValues)
			{
				Item item2 = fieldType.Children["Menu"];
				if (item2 != null)
				{
					ChildList children = item2.Children;
					int count = children.Count;
					if (count > 0)
					{
						string str6 = children[0]["Display Name"];
						name =
							(string.Concat(new object[]
							               	{
							               		"<span id=\"SmartTag_", field.ControlID,
							               		"\" onmouseover='javascript:scContent.smartTag(this, event, \"", field.ControlID, "\", ",
							               		StringUtil.EscapeJavascriptString(str6), ",\"", count, "\")'>"
							               	}) +
							 Images.GetImage("Images/SmartTag.png", 11, 11, "middle", "0px 4px 0px 0px")) + "</span>" + name;
					}
				}
			}
			name = "<div class=\"" + str5 + "\"" + str4 + ">" + name + "</div>";
			this.AddLiteralControl(parent, name);
		}

		public void RenderMarkerBegin(Control parent, string fieldControlID)
		{
			Assert.ArgumentNotNull(parent, "parent");
			Assert.ArgumentNotNullOrEmpty(fieldControlID, "fieldControlID");
			string text =
				"<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" class=\"scEditorFieldMarker\"><tr><td id=\"FieldMarker" +
				fieldControlID +
				"\" class=\"scEditorFieldMarkerBarCell\"><img src=\"/sitecore/images/blank.gif\" width=\"4px\" height=\"1px\" /></td><td class=\"scEditorFieldMarkerInputCell\">";
			this.AddLiteralControl(parent, text);
		}

		public void RenderMarkerEnd(Control parent)
		{
			Assert.ArgumentNotNull(parent, "parent");
			this.AddLiteralControl(parent, "</td></tr></table>");
		}

		protected virtual Item CurrentItem { get; set; }

		private string RenderMenuButtons(Sitecore.Shell.Applications.ContentManager.Editor.Field field, Item menu, bool readOnly)
		{
			Assert.ArgumentNotNull(field, "field");
			Assert.ArgumentNotNull(menu, "menu");
			HtmlTextWriter writer = new HtmlTextWriter(new StringWriter());
			writer.Write("<div class=\"scContentButtons\">");
			bool flag = true;
			foreach (Item item in menu.Children)
			{
				if (!this.IsFieldEditor || MainUtil.GetBool(item["Show In Field Editor"], false))
				{
					if (!flag)
					{
						writer.Write("&#183;");
					}
					flag = false;

					string clickEvent = string.Empty;
					if (!string.IsNullOrEmpty(item["Message"]))
					{
						clickEvent = item["Message"];
					}

					FieldPlaceholderArgs fieldPlaceholderArgs = new FieldPlaceholderArgs();
					fieldPlaceholderArgs.FieldId = field.ControlID;
					if (this.CurrentItem != null)
					{
						fieldPlaceholderArgs.ItemId = this.CurrentItem.ID.ToString();
						fieldPlaceholderArgs.InnerItem = CurrentItem;
						fieldPlaceholderArgs.TemplateItem = CurrentItem.Template;
					}
					fieldPlaceholderArgs.Source = GetFieldSource(field);
					fieldPlaceholderArgs.ClickEvent = item["Message"];
					fieldPlaceholderArgs.FieldItem = menu;

					IFieldPlaceholderProcessor fieldPlaceholderProcessor = FieldPlaceholderProcessorFactory.GetProcessor();
					if (fieldPlaceholderProcessor != null)
					{
						clickEvent = fieldPlaceholderProcessor.Process(fieldPlaceholderArgs);
						if (string.IsNullOrEmpty(clickEvent))
						{
							clickEvent = string.Empty;
						}
					}

					//to send a message to the messaging system (handleMessage)
					if (item["Client Event"] == null || string.IsNullOrEmpty(item["Client Event"]) || item["Client Event"] == "0")
					{
						clickEvent = Sitecore.Context.ClientPage.GetClientEvent(clickEvent);
					}

					if (readOnly)
					{
						writer.Write("<span class=\"scContentButtonDisabled\">");
						writer.Write(item["Display Name"]);
						writer.Write("</span>");
					}
					else
					{
						string cssClass = "scContentButton";
						if (!string.IsNullOrEmpty(item["CssClass"]))
						{
							cssClass = item["CssClass"];
						}

						string innerHtml = item["Inner Html"];
						if (string.IsNullOrEmpty(innerHtml))
						{
							innerHtml = item["Display Name"];
						}

						writer.Write("<a href=\"#\" class=\"" + cssClass + "\" onclick=\"" + clickEvent + "\">");
						writer.Write(innerHtml);
						writer.Write("</a>");
					}
				}
			}
			writer.Write("</div>");
			return writer.InnerWriter.ToString();
		}

		public virtual void RenderMenuButtons(Control parent, Sitecore.Shell.Applications.ContentManager.Editor.Field field,
		                                      Item fieldType, bool readOnly)
		{
			Assert.ArgumentNotNull(parent, "parent");
			Assert.ArgumentNotNull(field, "field");
			Assert.ArgumentNotNull(fieldType, "fieldType");
			if (this.Arguments.ShowInputBoxes && !UserOptions.ContentEditor.ShowRawValues)
			{
				Item menu = fieldType.Children["Menu"];
				if ((menu != null) && menu.HasChildren)
				{
					this.AddLiteralControl(parent, this.RenderMenuButtons(field, menu, readOnly));
				}
			}
		}

		private void RenderResizable(Control parent, Sitecore.Shell.Applications.ContentManager.Editor.Field field)
		{
			string str = field.TemplateField.Type;
			if (!string.IsNullOrEmpty(str))
			{
				FieldType fieldType = FieldTypeManager.GetFieldType(str);
				if ((fieldType != null) && fieldType.Resizable)
				{
					string text =
						string.Concat(new object[]
						              	{
						              		"<div style=\"cursor:row-resize\" onmousedown=\"scContent.fieldResizeDown(this, event)\" onmousemove=\"scContent.fieldResizeMove(this, event)\" onmouseup=\"scContent.fieldResizeUp(this, event, '"
						              		, field.TemplateField.ID.ToShortID(), "')\">", Images.GetSpacer(1, 4), "</div>"
						              	});
					this.AddLiteralControl(parent, text);
				}
			}
		}

		public void RenderSection(Sitecore.Shell.Applications.ContentManager.Editor.Section section, Control parent,
		                          bool readOnly)
		{
			Assert.ArgumentNotNull(section, "section");
			Assert.ArgumentNotNull(parent, "parent");
			bool isSectionCollapsed = section.IsSectionCollapsed;
			bool renderFields = !isSectionCollapsed || UserOptions.ContentEditor.RenderCollapsedSections;
			this.RenderSectionBegin(parent, section.ControlID, section.Name, section.DisplayName, section.Icon,
			                        isSectionCollapsed, renderFields);
			if (renderFields)
			{
				for (int i = 0; i < section.Fields.Count; i++)
				{
					this.RenderField(parent, section.Fields[i], readOnly);
				}
			}
			this.RenderSectionEnd(parent, renderFields, isSectionCollapsed);
		}

		public void RenderSectionBegin(Control parent, string controlId, string sectionName, string displayName, string icon,
		                               bool isCollapsed, bool renderFields)
		{
			Assert.ArgumentNotNull(parent, "parent");
			Assert.ArgumentNotNullOrEmpty(controlId, "controlId");
			Assert.ArgumentNotNullOrEmpty(sectionName, "sectionName");
			Assert.ArgumentNotNullOrEmpty(displayName, "displayName");
			Assert.ArgumentNotNull(icon, "icon");
			HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(new StringBuilder(0x400)));
			if (this.Arguments.ShowSections)
			{
				string str2;
				string str = isCollapsed ? "scEditorSectionCaptionCollapsed" : "scEditorSectionCaptionExpanded";
				if (UserOptions.ContentEditor.RenderCollapsedSections)
				{
					str2 = "javascript:scContent.toggleSection('" + controlId + "','" + sectionName + "')";
				}
				else
				{
					str2 = "javascript:return scForm.postRequest('','','','" +
					       StringUtil.EscapeQuote("ToggleSection(\"" + sectionName + "\",\"" + (isCollapsed ? "1" : "0") + "\")") +
					       "')";
				}
				writer.Write("<div id=\"{0}\" class=\"{1}\" ondblclick=\"" + str2 + "\">", controlId, str);
				ImageBuilder builder2 = new ImageBuilder
				                        	{
				                        		ID = controlId + "_Glyph",
				                        		Src = isCollapsed ? "Images/expand15x15.gif" : "Images/collapse15x15.gif",
				                        		Class = "scEditorSectionCaptionGlyph",
				                        		OnClick = str2
				                        	};
				ImageBuilder builder = builder2;
				writer.Write(builder.ToString());
				writer.Write(
					new ImageBuilder
						{Src = Images.GetThemedImageSource(icon, ImageDimension.id16x16), Class = "scEditorSectionCaptionIcon"}.ToString());
				writer.Write(Translate.Text(displayName));
				writer.Write("</div>");
			}
			if (renderFields || !isCollapsed)
			{
				string str3 = (isCollapsed && this.Arguments.ShowSections) ? " style=\"display:none\"" : string.Empty;
				string str4 = this.Arguments.ShowSections ? "scEditorSectionPanelCell" : "scEditorSectionPanelCell_NoSections";
				writer.Write("<table width=\"100%\" class=\"scEditorSectionPanel\"{0}><tr><td class=\"{1}\">", str3, str4);
			}
			this.AddLiteralControl(parent, writer.InnerWriter.ToString());
		}

		public void RenderSectionEnd(Control parent, bool renderFields)
		{
			Assert.ArgumentNotNull(parent, "parent");
			this.RenderSectionEnd(parent, renderFields, false);
		}

		public void RenderSectionEnd(Control parent, bool renderFields, bool isCollapsed)
		{
			Assert.ArgumentNotNull(parent, "parent");
			if (renderFields || !isCollapsed)
			{
				this.AddLiteralControl(parent, "</td></tr></table>");
			}
		}

		public void RenderSections(RenderContentEditorArgs args)
		{
			CurrentItem = args.Item;


			Assert.ArgumentNotNull(args, "args");
			this.Arguments = args;
			RenderSections(args.Parent, args.Sections, args.ReadOnly);
		}

		public void RenderSections(Control parent, Sitecore.Shell.Applications.ContentManager.Editor.Sections sections,
		                           bool readOnly)
		{
			Assert.ArgumentNotNull(parent, "parent");
			Assert.ArgumentNotNull(sections, "sections");
			Sitecore.Context.ClientPage.ClientResponse.DisableOutput();
			this.AddLiteralControl(parent, "<div class=\"scEditorSections\">");
			for (int i = 0; i < sections.Count; i++)
			{
				this.RenderSection(sections[i], parent, readOnly);
			}
			this.AddLiteralControl(parent, "</div>");
			Sitecore.Context.ClientPage.ClientResponse.EnableOutput();
		}

		public static void SetAttributes(Control editor, Sitecore.Shell.Applications.ContentManager.Editor.Field field,
		                                 bool hasRibbon)
		{
			Assert.ArgumentNotNull(editor, "editor");
			Assert.ArgumentNotNull(field, "field");
			AttributeCollection property = Sitecore.Reflection.ReflectionUtil.GetProperty(editor, "Attributes") as AttributeCollection;
			if (property != null)
			{
				string str =
					string.Concat(new object[] {field.ItemField.Item.Uri, "&fld=", field.ItemField.ID, "&ctl=", field.ControlID});
				if (hasRibbon)
				{
					str = str + "&rib=1";
				}
				property["onactivate"] = "javascript:return scContent.activate(this,event,'" + str + "')";
				property["ondeactivate"] = "javascript:return scContent.deactivate(this,event,'" + str + "')";
				if (!UIUtil.IsIE())
				{
					property["onfocus"] = "javascript:return scContent.activate(this,event,'" + str + "')";
					property["onblur"] = "javascript:return scContent.deactivate(this,event,'" + str + "')";
				}
			}
		}

		public virtual void SetProperties(Control editor, Sitecore.Shell.Applications.ContentManager.Editor.Field field, bool readOnly)
		{
			Assert.ArgumentNotNull(editor, "editor");
			Assert.ArgumentNotNull(field, "field");
			Sitecore.Reflection.ReflectionUtil.SetProperty(editor, "ID", field.ControlID);
			Sitecore.Reflection.ReflectionUtil.SetProperty(editor, "ItemID", field.ItemField.Item.ID.ToString());
			Sitecore.Reflection.ReflectionUtil.SetProperty(editor, "ItemVersion", field.ItemField.Item.Version.ToString());
			Sitecore.Reflection.ReflectionUtil.SetProperty(editor, "ItemLanguage", field.ItemField.Item.Language.ToString());
			Sitecore.Reflection.ReflectionUtil.SetProperty(editor, "FieldID", field.ItemField.ID.ToString());
			Sitecore.Reflection.ReflectionUtil.SetProperty(editor, "Source", GetFieldSource(field));
			Sitecore.Reflection.ReflectionUtil.SetProperty(editor, "ReadOnly", readOnly);
			Sitecore.Reflection.ReflectionUtil.SetProperty(editor, "Disabled", readOnly);

			//if the control has the field type item id property
			string fieldTypeId = string.Empty;
			if (!string.IsNullOrEmpty(field.TemplateField.Type))
			{
				//get field type by name
				Item fieldType = GetFieldType(field.TemplateField.Type);
				if (fieldType.IsNotNull())
				{
					fieldTypeId = fieldType.ID.ToString();
				}
			}
			Sitecore.Reflection.ReflectionUtil.SetProperty(editor, "FieldTypeItemId", fieldTypeId);
		}

		public static bool HasMethod(object objectToCheck, string methodName)
		{
			var type = objectToCheck.GetType();
			return type.GetMethod(methodName) != null;
		} 

		/// <summary>
		/// Get field type Id of the current field item
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private static Item GetFieldType(string name)
		{
			Database coreDatabase = Database.GetDatabase("core");
			if (coreDatabase == null)
			{
				return null;
			}

			List<Item> fieldTypeItems = new List<Item>();
			if (HttpContext.Current.Cache["FieldSuite.FieldTypes"] != null)
			{
				//retrieve from cache
				fieldTypeItems = (List<Item>) HttpContext.Current.Cache["FieldSuite.FieldTypes"];
			}
			else
			{
				Item fieldTypeFolder = coreDatabase.GetItem("/sitecore/system/Field types");
				if (fieldTypeFolder.IsNull())
				{
					return null;
				}

				//retrieve from sitecore and add to cache
				fieldTypeItems = fieldTypeFolder.Axes.GetDescendants().ToList();
				HttpContext.Current.Cache["FieldSuite.FieldTypes"] = fieldTypeItems;
			}

			foreach (Item fieldTypeItem in fieldTypeItems)
			{
				if (fieldTypeItem.IsNull() || !fieldTypeItem.IsOfTemplate("{F8A17D6A-118E-4CD7-B5F5-88FF37A4F237}"))
				{
					continue;
				}

				if (fieldTypeItem.Name == name)
				{
					return fieldTypeItem;
				}
			}

			return null;
		}

		/// <summary>
		/// This method will return the field source but will also check if the source is parameterized and if the datasource has a query. if so it will convert the query into an item path.
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		public virtual string GetFieldSource(Sitecore.Shell.Applications.ContentManager.Editor.Field field)
		{
			if (field == null || field.ItemField == null || field.ItemField.Item == null || string.IsNullOrEmpty(field.ItemField.Source))
			{
				return string.Empty;
			}
			
			string rawSource = field.ItemField.Source;
			return (!string.IsNullOrEmpty(rawSource) && rawSource.IndexOf("datasource=query:", StringComparison.OrdinalIgnoreCase) >= 0)
				? ReplaceQueriesInDataSource(rawSource, field.ItemField.Item)
				: rawSource;
		}

		/// <summary>
		/// This method is used to change the query value in the datasource value into an item path
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		private static string ReplaceQueriesInDataSource(string source, Item contextItem) {

			string ds = StringUtil.ExtractParameter("DataSource", source).Trim();
			Item itemQueried = GetQueriedItem(contextItem, ds);
			return (itemQueried != null)
				? source.Replace(ds, itemQueried.Paths.FullPath)
				: source.Replace(ds, string.Empty);
		}

		/// <summary>
		/// This runs the query on the axes for the item passed in making it relative to that item
		/// </summary>
		/// <param name="currentItem"></param>
		/// <param name="query"></param>
		/// <returns></returns>
		private static Item GetQueriedItem(Item currentItem, string query) {
			if (currentItem == null || string.IsNullOrEmpty(query))
				return null;

			Item[] returnItems = LookupSources.GetItems(currentItem, query);

			return (returnItems != null && returnItems.Any()) ? returnItems.First() : null;
		}

		public static void SetStyle(Control editor, Sitecore.Shell.Applications.ContentManager.Editor.Field field)
		{
			Assert.ArgumentNotNull(editor, "editor");
			Assert.ArgumentNotNull(field, "field");
			if (!string.IsNullOrEmpty(field.ItemField.Style))
			{
				CssStyleCollection property = Sitecore.Reflection.ReflectionUtil.GetProperty(editor, "Style") as CssStyleCollection;
				if (property != null)
				{
					UIUtil.ParseStyle(property, field.ItemField.Style);
				}
			}
		}

		public void SetValue(Control editor, string value)
		{
			Assert.ArgumentNotNull(editor, "editor");
			Assert.ArgumentNotNull(value, "value");
			if (!(editor is IStreamedContentField))
			{
				IContentField field = editor as IContentField;
				if (field != null)
				{
					field.SetValue(value);
				}
				else
				{
					Sitecore.Reflection.ReflectionUtil.SetProperty(editor, "Value", value);
				}
			}
		}

		// Properties
		public RenderContentEditorArgs Arguments
		{
			get { return this.arguments; }
			set
			{
				Assert.ArgumentNotNull(value, "value");
				this.arguments = value;
			}
		}

		public bool IsFieldEditor { get; set; }

		private static ILog _logger;
		private static ILog Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = LogManager.GetLogger(typeof(FieldSuiteEditorFormatter));
				}
				return _logger;
			}
		}
	}
}