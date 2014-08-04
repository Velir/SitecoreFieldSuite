//using ACC.Library.CustomSitecore.Applications.Content_Editor.Pipeline.RenderContentEditor;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Pipelines;
using Sitecore.Pipelines.GetContentEditorWarnings;
using Sitecore.Resources;
using Sitecore.SecurityModel;
using Sitecore.Shell;
using Sitecore.Shell.Applications.ContentEditor.Galleries;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.GetContentEditorFields;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditor;
using Sitecore.Shell.Applications.ContentManager;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.WebControls;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Sitecore.SharedSource.FieldSuite.Editors
{
	[DebuggerDisplay("{TemplateSection}")]
	public class FieldSuiteEditor
	{
		private Sitecore.Web.UI.Sheer.ClientPage _clientPage;
		private Sitecore.Data.Database _contentDatabase;
		private Sitecore.Data.Database _database;
		private Hashtable _fieldInfo = new Hashtable();
		private string _id;
		private int _isAdministrator = -1;
		private Sitecore.Data.Items.Item _item;
		private Sitecore.Globalization.Language _language;
		private Sitecore.Security.Accounts.User _user;
		private List<string> _sectionsToRender;
		private Dictionary<string, string> _newSectionNames;

		public FieldSuiteEditor()
		{
			this.RenderTabsAndBars = true;
			this.RenderWarnings = true;
			this.RenderHeader = true;
			this.ShowSections = UserOptions.ContentEditor.ShowSections;
			this.ShowDataFieldsOnly = !UserOptions.ContentEditor.ShowSystemFields;
			this.ShowInputBoxes = !UserOptions.ContentEditor.ShowRawValues;
		}

		private static int GetActiveTab(IList<EditorTab> editorTabs)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(editorTabs, "editorTabs");
			string formValue = WebUtil.GetFormValue("scActiveEditorTab");
			if (!string.IsNullOrEmpty(formValue))
			{
				for (int i = 0; i < editorTabs.Count; i++)
				{
					EditorTab tab = editorTabs[i];
					if (tab.Id == formValue)
					{
						return i;
					}
				}
			}
			return 0;
		}

		private static void GetContentTab(Sitecore.Data.Items.Item item,
		                                  Sitecore.Shell.Applications.ContentManager.Editor.Sections sections,
		                                  ICollection<EditorTab> tabs)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(sections, "sections");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(tabs, "tabs");
			EditorTab tab = GetEditorTab("Content", Translate.Text("Content"), "People/16x16/cube_blue.png", "<content>", false);
			HtmlTextWriter output = new HtmlTextWriter(new StringWriter());
			RenderContentControls(output, item, sections);
			tab.Controls = output.InnerWriter.ToString();
			tabs.Add(tab);
		}

		private static void GetCustomEditorTab(Sitecore.Data.Items.Item item, ICollection<EditorTab> tabs)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(tabs, "tabs");
			string customEditor = item.Appearance.CustomEditor;
			if (!string.IsNullOrEmpty(customEditor))
			{
				EditorTab tab = GetEditorTab("CustomEditor", Translate.Text("Custom Editor"), "Applications/16x16/form_blue.png",
				                             customEditor, false);
				tabs.Add(tab);
			}
		}

		private static void GetCustomEditorTabs(Sitecore.Data.Items.Item item, ICollection<EditorTab> tabs)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(tabs, "tabs");
			ListString str = new ListString(item["__Editors"]);
			foreach (string str2 in str)
			{
				Sitecore.Data.Items.Item item2 = Sitecore.Client.CoreDatabase.GetItem(str2);
				if (item2 != null)
				{
					UrlString str3 = new UrlString(item2["Url"]);
					str3["id"] = item.ID.ToString();
					str3["la"] = item.Language.ToString();
					str3["language"] = item.Language.ToString();
					str3["vs"] = item.Version.ToString();
					str3["version"] = item.Version.ToString();
					EditorTab tab = GetEditorTab("T" + Sitecore.Data.ID.NewID.ToShortID(), item2["Header"], item2["Icon"],
					                             str3.ToString(), item2["Refresh On Show"] == "1");
					tabs.Add(tab);
				}
			}
		}

		private static void GetDynamicTabs(Sitecore.Data.Items.Item item, ICollection<EditorTab> tabs)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(tabs, "tabs");
			ListString str = new ListString(WebUtil.GetFormValue("scEditorTabs"));
			foreach (string str2 in str)
			{
				if (!string.IsNullOrEmpty(str2))
				{
					string[] strArray = str2.Split(new char[] {'^'});
					string str3 = strArray[0];
					if (!string.IsNullOrEmpty(str3))
					{
						Command command = CommandManager.GetCommand(str3);
						Sitecore.Diagnostics.Assert.IsNotNull(command, typeof (Command), "Command \"{0}\" not found",
						                                      new object[] {strArray[0]});
						CommandContext context = new CommandContext(item);
						switch (CommandManager.QueryState(command, context))
						{
							case CommandState.Disabled:
							case CommandState.Hidden:
								{
									continue;
								}
						}
					}
					UrlString str4 = new UrlString(strArray[3]);
					str4["id"] = item.ID.ToString();
					str4["la"] = item.Language.ToString();
					str4["language"] = item.Language.ToString();
					str4["vs"] = item.Version.ToString();
					str4["version"] = item.Version.ToString();
					EditorTab tab = GetEditorTab(strArray[5], strArray[1], strArray[2], str4.ToString(), strArray[4] == "1");
					tab.Closeable = true;
					tabs.Add(tab);
				}
			}
		}

		protected virtual EditorFormatter GetEditorFormatter()
		{
			return new EditorFormatter();
		}

		private static EditorTab GetEditorTab(string id, string header, string icon, string url, bool refreshOnShow)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(id, "id");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(header, "header");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(icon, "icon");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(url, "url");
			return new EditorTab {Header = header, Icon = icon, Url = url, RefreshOnShow = refreshOnShow, Id = id};
		}

		private static List<EditorTab> GetEditorTabs(Sitecore.Data.Items.Item item,
		                                             Sitecore.Shell.Applications.ContentManager.Editor.Sections sections)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(sections, "sections");
			List<EditorTab> tabs = new List<EditorTab>();
			GetCustomEditorTab(item, tabs);
			GetCustomEditorTabs(item, tabs);
			GetContentTab(item, sections, tabs);
			GetDynamicTabs(item, tabs);
			return tabs;
		}

		protected virtual bool GetReadOnly(Sitecore.Data.Items.Item item)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			bool flag = !item.State.CanSave();
			if (!item.Access.CanWriteLanguage())
			{
				return true;
			}
			if (item.Appearance.ReadOnly)
			{
				flag = true;
			}
			return flag;
		}

		protected virtual Sitecore.Shell.Applications.ContentManager.Editor.Sections GetSections()
		{
			Sitecore.Shell.Applications.ContentManager.Editor.Sections sections =
				new Sitecore.Shell.Applications.ContentManager.Editor.Sections();
			GetContentEditorFieldsArgs args2 = new GetContentEditorFieldsArgs(this.Item)
			                                   	{
			                                   		Sections = sections,
			                                   		FieldInfo = this.FieldInfo,
			                                   		ShowDataFieldsOnly = this.ShowDataFieldsOnly
			                                   	};
			GetContentEditorFieldsArgs args = args2;
			using (
				new LongRunningOperationWatcher(Settings.Profiling.RenderFieldThreshold, "getContentEditorFields pipeline[id={0}]",
				                                new string[] {this.Item.ID.ToString()}))
			{
				CorePipeline.Run("getContentEditorFields", args);
			}
			return sections;
		}

		protected virtual GetContentEditorWarningsArgs GetWarnings(bool hasSections)
		{
			GetContentEditorWarningsArgs args2 = new GetContentEditorWarningsArgs(this._item)
			                                     	{
			                                     		ShowInputBoxes = this.ShowInputBoxes,
			                                     		HasSections = hasSections
			                                     	};
			GetContentEditorWarningsArgs args = args2;
			using (
				new LongRunningOperationWatcher(Settings.Profiling.RenderFieldThreshold, "GetContentEditorWarningsArgs pipeline",
				                                new string[0]))
			{
				CorePipeline.Run("getContentEditorWarnings", args);
			}
			return args;
		}

		protected void Render(RenderContentEditorArgs args, System.Web.UI.Control parent)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(args, "args");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(parent, "parent");
			args.ShowSections = this.ShowSections;
			args.ShowInputBoxes = this.ShowInputBoxes;

			args.EditorFormatter.Arguments = args;
			args.RenderTabsAndBars = this.RenderTabsAndBars;
			GetContentEditorWarningsArgs warnings = this.GetWarnings(args.Sections.Count > 0);
			if (warnings.HasFullscreenWarnings())
			{
				this.RenderFullscreenWarnings(args, warnings.Warnings);
			}
			else
			{
				args.Parent = RenderEditorTabs(args);
				if (this.ShouldShowHeader())
				{
					this.RenderHeaderPanel(args);
				}
				this.RenderAllWarnings(args, warnings.Warnings);
				if (UserOptions.ContentEditor.ShowQuickInfo && this.ShouldShowHeader())
				{
					RenderQuickInfo(args);
				}
				if (!warnings.HideFields())
				{
					using (
						new LongRunningOperationWatcher(Settings.Profiling.RenderFieldThreshold,
														"renderContentEditor pipeline[id={0}]",
						                                new string[] {(this._item != null) ? this._item.ID.ToString() : string.Empty}))
					{
						CorePipeline.Run("renderContentEditor", args);
					}
				}
			}
		}

		public virtual void Render(Sitecore.Data.Items.Item item, Sitecore.Data.Items.Item root, Hashtable fieldInfo,
		                           System.Web.UI.Control parent, bool showEditor)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(fieldInfo, "fieldInfo");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(parent, "parent");
			this._item = item;
			this._fieldInfo = fieldInfo;
			fieldInfo.Clear();
			Sitecore.Shell.Applications.ContentManager.Editor.Sections sections = this.GetSections();
			bool flag = (this._item != null) ? this.GetReadOnly(this._item) : true;
			RenderContentEditorArgs args2 = new RenderContentEditorArgs
			                                	{
			                                		EditorFormatter = this.GetEditorFormatter(),
			                                		Item = this._item,
			                                		Parent = parent,
			                                		Sections = sections,
			                                		ReadOnly = flag,
			                                		Language = this.Language,
			                                		IsAdministrator = this.IsAdministrator
			                                	};
			RenderContentEditorArgs args = args2;
			this.Render(args, parent);
		}

		private void RenderAllWarnings(RenderContentEditorArgs args,
		                               List<GetContentEditorWarningsArgs.ContentEditorWarning> warnings)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(args, "args");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(warnings, "warnings");
			foreach (GetContentEditorWarningsArgs.ContentEditorWarning warning in warnings)
			{
				if (warning.IsExclusive)
				{
					RenderWarning(args, args.Parent, warning);
					return;
				}
			}
			foreach (GetContentEditorWarningsArgs.ContentEditorWarning warning2 in warnings)
			{
				RenderWarning(args, args.Parent, warning2);
			}
		}

		private static void RenderContentControls(HtmlTextWriter output, Sitecore.Data.Items.Item item,
		                                          Sitecore.Shell.Applications.ContentManager.Editor.Sections sections)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(sections, "sections");
			output.Write("<div class=\"scEditorTabControls\">");
			new ImageBuilder {Src = "Images/Ribbon/tab4.png", Class = "scEditorTabControlsTab4"}.ToString(output);
			output.Write("<span class=\"scEditorTabControlsTab5\">");
			RenderHeaderNavigator(output, sections);
			RenderHeaderLanguage(output, item);
			RenderHeaderVersion(output, item);
			output.Write("</span>");
			new ImageBuilder
				{
					Src = "Images/Ribbon/tab6.png",
					Class = "scEditorTabControlsTab6",
					Width = 6,
					Height = 0x18,
					Margin = "0px 4px 0px 0px"
				}.ToString(output);
			output.Write("</div>");
		}

		private static System.Web.UI.Control RenderContentEditor(System.Web.UI.Control parent, EditorTab tab, bool active,
		                                                         RenderContentEditorArgs args)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(parent, "parent");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(tab, "tab");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(args, "args");
			GridPanel panel2 = new GridPanel
			                   	{
			                   		ID = "F" + tab.Id
			                   	};
			GridPanel child = panel2;
			parent.Controls.Add(child);
			child.Width = new Unit(100.0, UnitType.Percentage);
			child.Height = new Unit(100.0, UnitType.Percentage);
			child.Columns = 2;
			child.Style["table-layout"] = "fixed";
			if (!active)
			{
				child.Style["display"] = "none";
			}
			HtmlGenericControl control = new HtmlGenericControl("div");
			child.Controls.Add(control);
			control.Attributes["id"] = "EditorPanel";
			control.Attributes["class"] = "scEditorPanel";
			if (!args.EditorFormatter.IsFieldEditor)
			{
				AttributeCollection attributes;
				(attributes = control.Attributes)["class"] = attributes["class"] + " scFixSize";
			}
			child.SetExtensibleProperty(control, "class", "scEditorPanelCell");
			child.SetExtensibleProperty(control, "valign", "top");
			return control;
		}

		private static void RenderEditorTab(HtmlTextWriter output, EditorTab tab, int index, int count, int activeTab)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(tab, "tab");
			bool flag = index == activeTab;
			string str = flag ? "scRibbonEditorTabActive" : "scRibbonEditorTabNormal";
			string str2 = flag ? "scEditorTabHeaderActive" : "scEditorTabHeaderNormal";
			output.Write("<a id=\"B" + tab.Id +
			             "\" href=\"#\" onclick=\"javascript:return scContent.onEditorTabClick(this,event,'" + tab.Id +
			             "')\" class=\"" + str + "\">");
			ImageBuilder builder = new ImageBuilder();
			if (index == 0)
			{
				builder.Src = flag ? "Images/Ribbon/tab0_h.png" : "Images/Ribbon/tab0.png";
				builder.Class = "scEditorTabControlsTab0";
				output.Write(builder.ToString());
			}
			output.Write("<span class=\"" + str2 + "\">");
			ImageBuilder builder3 = new ImageBuilder
			                        	{
			                        		Src = StringUtil.GetString(new string[] {tab.Icon, "Applications/16x16/form_blue.png"}),
			                        		Class = "scEditorTabIcon",
			                        		Width = 0x10,
			                        		Height = 0x10
			                        	};
			ImageBuilder builder2 = builder3;
			output.Write(builder2.ToString());
			output.Write(tab.Header);
			if (tab.Closeable)
			{
				builder2.Src = "Images/Close.png";
				builder2.Class = "scEditorTabClose";
				builder2.Width = 0x10;
				builder2.Height = 0x10;
				builder2.RollOver = true;
				builder2.OnClick = "javascript:scContent.closeEditorTab('" + tab.Id + "');";
				output.Write(builder2.ToString());
			}
			output.Write("</span>");
			if (index < (count - 1))
			{
				if (activeTab == (index + 1))
				{
					builder.Src = "Images/Ribbon/tab2_h2.png";
				}
				else
				{
					builder.Src = flag ? "Images/Ribbon/tab2_h1.png" : "Images/Ribbon/tab2.png";
				}
				builder.Class = "scEditorTabControlsTab2";
				output.Write(builder.ToString());
			}
			else
			{
				builder.Src = flag ? "Images/Ribbon/tab3_h.png" : "Images/Ribbon/tab3.png";
				builder.Class = "scEditorTabControlsTab3";
				output.Write(builder.ToString());
			}
			output.Write("</a>");
		}

		private static void RenderEditorTabControls(HtmlTextWriter output, EditorTab tab, int index, int activeTab)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(tab, "tab");
			if (!string.IsNullOrEmpty(tab.Controls))
			{
				string str2 = (activeTab == index) ? string.Empty : " style=\"display:none\"";
				output.Write("<div id=\"EditorTabControls_" + tab.Id + "\" class=\"scEditorTabControlsHolder\"" + str2 + ">");
				output.Write(tab.Controls);
				output.Write("</div>");
			}
		}

		private static System.Web.UI.Control RenderEditorTabs(RenderContentEditorArgs args)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(args, "args");
			if (args.EditorFormatter.IsFieldEditor)
			{
				EditorTab tab2 = new EditorTab
				                 	{
				                 		Id = "EmbeddedEditor"
				                 	};
				EditorTab tab = tab2;
				return RenderContentEditor(args.Parent, tab, true, args);
			}
			if (!args.RenderTabsAndBars)
			{
				return Sitecore.Diagnostics.Assert.ResultNotNull<System.Web.UI.Control>(args.Parent);
			}
			GridPanel child = new GridPanel();
			args.Parent.Controls.Add(child);
			child.Width = new Unit(100.0, UnitType.Percentage);
			child.Height = new Unit(100.0, UnitType.Percentage);
			child.CssClass = "scEditorGrid";
			Border border3 = new Border
			                 	{
			                 		Background = "transparent"
			                 	};
			Border border = border3;
			child.Controls.Add(border);
			child.SetExtensibleProperty(border, "valign", "top");
			Sitecore.Data.Items.Item item = args.Item;
			Sitecore.Diagnostics.Assert.IsNotNull(item, "item");
			Sitecore.Shell.Applications.ContentManager.Editor.Sections sections = args.Sections;
			Sitecore.Diagnostics.Assert.IsNotNull(sections, "sections");
			List<EditorTab> editorTabs = GetEditorTabs(item, sections);
			int activeTab = GetActiveTab(editorTabs);
			HtmlTextWriter output = new HtmlTextWriter(new StringWriter());
			output.Write("<div id=\"EditorTabs\">");
			int count = editorTabs.Count;
			for (int i = 0; i < count; i++)
			{
				RenderEditorTabControls(output, editorTabs[i], i, activeTab);
			}
			for (int j = 0; j < count; j++)
			{
				RenderEditorTab(output, editorTabs[j], j, count, activeTab);
			}
			output.Write("</div>");
			border.Controls.Add(new LiteralControl(output.InnerWriter.ToString()));
			Border border4 = new Border
			                 	{
			                 		ID = "EditorFrames"
			                 	};
			Border border2 = border4;
			child.Controls.Add(border2);
			child.SetExtensibleProperty(border2, "valign", "top");
			child.SetExtensibleProperty(border2, "height", "100%");
			border2.Height = new Unit(100.0, UnitType.Percentage);
			System.Web.UI.Control control = border2;
			output = new HtmlTextWriter(new StringWriter());
			for (int k = 0; k < editorTabs.Count; k++)
			{
				bool active = k == activeTab;
				EditorTab tab3 = editorTabs[k];
				if (tab3.Url == "<content>")
				{
					border2.Controls.Add(new LiteralControl(output.InnerWriter.ToString()));
					control = RenderContentEditor(border2, tab3, active, args);
					output = new HtmlTextWriter(new StringWriter());
				}
				else
				{
					RenderFrame(output, item, tab3, active, args.ReadOnly);
				}
			}
			border2.Controls.Add(new LiteralControl(output.InnerWriter.ToString()));
			return control;
		}

		private static void RenderFrame(HtmlTextWriter output, Sitecore.Data.Items.Item item, EditorTab tab, bool active,
		                                bool readOnly)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(tab, "tab");
			UrlString str = new UrlString(UIUtil.GetUri(tab.Url));
			str.Append("id", item.ID.ToString());
			str.Append("language", item.Language.ToString());
			str.Append("version", item.Version.ToString());
			str.Append("database", item.Database.Name);
			str.Append("readonly", readOnly ? "1" : "0");
			str.Append("la", item.Language.ToString());
			str.Append("vs", item.Version.ToString());
			str.Append("db", item.Database.Name);
			string str2 = active ? string.Empty : "display:none";
			if (!string.IsNullOrEmpty(str2))
			{
				str2 = " style=\"" + str2 + "\"";
			}
			output.Write(
				string.Concat(new object[]
				              	{
				              		"<iframe id=\"F", tab.Id, "\" src=\"", str,
				              		"\" width=\"100%\" height=\"100%\" frameborder=\"no\" marginwidth=\"0\" marginheight=\"0\"", str2,
				              		"></iframe>"
				              	}));
		}

		private void RenderFullscreenWarnings(RenderContentEditorArgs args,
		                                      List<GetContentEditorWarningsArgs.ContentEditorWarning> warnings)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(args, "args");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(warnings, "warnings");
			foreach (GetContentEditorWarningsArgs.ContentEditorWarning warning in warnings)
			{
				if (warning.IsFullscreen)
				{
					Border control = new Border();
					control.Style["width"] = "100%";
					control.Style["height"] = "100%";
					control.Style["background"] = "white";
					Context.ClientPage.AddControl(args.Parent, control);
					RenderWarning(args, control, warning);
					break;
				}
			}
		}

		private static void RenderHeaderLanguage(HtmlTextWriter output, Sitecore.Data.Items.Item item)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			UrlString str = new UrlString();
			str.Append("id", item.ID.ToString());
			str.Append("la", item.Language.ToString());
			str.Append("vs", item.Version.ToString());
			str.Append("db", item.Database.Name);
			str.Append("align", "right");
			string width = "400";
			string height = "250";
			GalleryManager.GetGallerySize("Header_Language_Gallery", ref width, ref height);
			string str4 =
				string.Concat(new object[]
				              	{
				              		"javascript:return scContent.showGallery(this,event,'Header_Language_Gallery','Gallery.Languages','"
				              		, str, "','", width, "','", height, "')"
				              	});
			new Tag("a")
				{
					Href = "#",
					Class = "scEditorHeaderVersionsLanguage scEditorHeaderButton",
					Title = item.Language.CultureInfo.DisplayName,
					Click = str4
				}.Start(output);
			ImageBuilder builder = new ImageBuilder();
			string icon = LanguageService.GetIcon(item.Language, item.Database);
			builder.Src = Images.GetThemedImageSource(icon, ImageDimension.id16x16);
			builder.Class = "scEditorHeaderVersionsLanguageIcon";
			builder.Alt = item.Language.CultureInfo.DisplayName;
			output.Write(builder.ToString());
			output.Write(
				new ImageBuilder {Src = "Images/ribbondropdown.gif", Class = "scEditorHeaderVersionsLanguageGlyph"}.ToString());
			output.Write("</a>");
		}

		private static void RenderHeaderNavigator(HtmlTextWriter output,
		                                          Sitecore.Shell.Applications.ContentManager.Editor.Sections sections)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(sections, "sections");
			new Tag("a")
				{
					ID = "ContentEditorNavigator",
					Class = "scEditorHeaderNavigator scEditorHeaderButton",
					Href = "#",
					Title = Translate.Text("Navigate to sections and fields."),
					Click = "javascript:return scForm.postEvent(this,event,'NavigatorMenu_DropDown()')"
				}.Start(output);
			output.Write(
				new ImageBuilder
					{
						Src = "Applications/16x16/bookmark.png",
						Class = "scEditorHeaderNavigatorIcon",
						Alt = "Navigate to sections and fields."
					}.ToString());
			output.Write(new ImageBuilder {Src = "Images/ribbondropdown.gif", Class = "scEditorHeaderNavigatorGlyph"}.ToString());
			output.Write("</a>");
		}

		private void RenderHeaderPanel(RenderContentEditorArgs args)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(args, "args");
			Sitecore.Data.Items.Item item = args.Item;
			if (item != null)
			{
				HtmlTextWriter writer = new HtmlTextWriter(new StringWriter());
				writer.Write("<div class=\"scEditorHeader\">");
				ImageBuilder builder = new ImageBuilder();
				UrlString str = new UrlString(Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id32x32));
				str["rev"] = item[FieldIDs.Revision];
				str["la"] = item.Language.ToString();
				builder.Src = str.ToString();
				builder.Class = "scEditorHeaderIcon";
				if (item.Appearance.ReadOnly || !item.Access.CanWrite())
				{
					writer.Write("<span class=\"scEditorHeaderIcon\">");
					writer.Write(builder.ToString());
					writer.Write("</span>");
				}
				else
				{
					writer.Write(
						"<a href=\"#\" class=\"scEditorHeaderIcon\" onclick=\"javascript:return scForm.invoke('item:selecticon')\">");
					writer.Write(builder.ToString());
					writer.Write("</a>");
				}
				string displayName = item.DisplayName;
				writer.Write("<div class=\"scEditorHeaderTitlePanel\">");
				if (this.IsAdministrator && (displayName != item.Name))
				{
					displayName = displayName + "<span class=\"scEditorHeaderTitleName\"> - [" + item.Name + "]</span>";
				}
				if (item.Appearance.ReadOnly || !item.Access.CanWrite())
				{
					writer.Write("<div class=\"scEditorHeaderTitle\">" + displayName + "</div>");
				}
				else
				{
					writer.Write(
						"<a href=\"#\" class=\"scEditorHeaderTitle\" onclick=\"javascript:return scForm.invoke('item:rename')\">" +
						displayName + "</a>");
				}
				if (item.Help.ToolTip.Length > 0)
				{
					writer.Write("<div class=\"scEditorHeaderTitleHelp\">" + item.Help.ToolTip + "</div>");
				}
				writer.Write("</div>");
				writer.Write("</div>");
				args.EditorFormatter.AddLiteralControl(args.Parent, writer.InnerWriter.ToString());
			}
		}

		private static void RenderHeaderVersion(HtmlTextWriter output, Sitecore.Data.Items.Item item)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			int count = item.Versions.Count;
			if (count > 0)
			{
				UrlString str = new UrlString();
				str.Append("id", item.ID.ToString());
				str.Append("la", item.Language.ToString());
				str.Append("vs", item.Version.ToString());
				str.Append("db", item.Database.Name);
				str.Append("align", "right");
				string width = "400";
				string height = "250";
				GalleryManager.GetGallerySize("Header_Version_Gallery", ref width, ref height);
				string str4 =
					string.Concat(new object[]
					              	{
					              		"javascript:return scContent.showGallery(this, event, 'Header_Version_Gallery','Gallery.Versions','"
					              		, str, "','", width, "','", height, "')"
					              	});
				string str5 = Translate.Text("Version {0} of {1}.", new object[] {item.Version, count});
				new Tag("a") {Href = "#", Class = "scEditorHeaderVersionsVersion scEditorHeaderButton", Title = str5, Click = str4}.
					Start(output);
				output.Write(item.Version);
				ImageBuilder builder = new ImageBuilder
				                       	{
				                       		Src = "Images/ribbondropdown.gif",
				                       		Class = "scEditorHeaderVersionsVersionGlyph"
				                       	};
				output.Write(builder);
				output.Write("</a>");
			}
		}

		private static void RenderQuickInfo(RenderContentEditorArgs args)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(args, "args");
			Sitecore.Data.Items.Item item = args.Item;
			if (item != null)
			{
				bool isQuickInfoSectionCollapsed = IsQuickInfoSectionCollapsed;
				bool renderCollapsedSections = UserOptions.ContentEditor.RenderCollapsedSections;
				args.EditorFormatter.RenderSectionBegin(args.Parent, "QuickInfo", "QuickInfo", "Quick Info",
				                                        "Applications/16x16/information.png", isQuickInfoSectionCollapsed,
				                                        renderCollapsedSections);
				if (renderCollapsedSections || !isQuickInfoSectionCollapsed)
				{
					HtmlTextWriter output = new HtmlTextWriter(new StringWriter());
					output.Write("<table cellpadding=\"4\" cellspacing=\"0\" border=\"0\">");
					output.Write("<col style=\"white-space:nowrap\" align=\"right\" valign=\"top\" />");
					output.Write("<col style=\"white-space:nowrap\" valign=\"top\" />");
					RenderQuickInfoID(output, item);
					RenderQuickInfoItemKey(output, item);
					RenderQuickInfoPath(output, item);
					RenderQuickInfoTemplate(output, item);
					RenderQuickInfoCreatedFrom(output, item);
					RenderQuickInfoOwner(output, item);
					output.Write("</table>");
					args.EditorFormatter.AddLiteralControl(args.Parent, output.InnerWriter.ToString());
				}
				args.EditorFormatter.RenderSectionEnd(args.Parent, renderCollapsedSections, isQuickInfoSectionCollapsed);
			}
		}

		private static void RenderQuickInfoCreatedFrom(HtmlTextWriter output, Sitecore.Data.Items.Item item)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			output.Write("<tr><td>");
			output.Write(Translate.Text("Created From:"));
			output.Write("</td><td>");
			if (ItemUtil.IsNull(item.BranchId))
			{
				output.Write(Translate.Text("[unknown]"));
			}
			else
			{
				Sitecore.Data.Items.Item item2;
				using (new SecurityDisabler())
				{
					item2 = item.Database.GetItem(item.BranchId);
				}
				if ((item2 != null) && item2.Access.CanRead())
				{
					output.Write("<a href=\"#\" onclick=\"javascript:scForm.postRequest('','','','item:load(id=" + item.BranchId +
					             ")');return false\">");
				}
				if (item2 != null)
				{
					output.Write(item2.DisplayName);
				}
				else
				{
					output.Write(Translate.Text("[branch no longer exists]"));
				}
				if ((item2 != null) && item2.Access.CanRead())
				{
					output.Write("</a>");
				}
				output.Write(" - ");
				output.Write(
					"<input class=\"scEditorHeaderQuickInfoInputID\" readonly=\"readonly\" onclick=\"javascript:this.select();return false\" value=\"" +
					item.BranchId + "\"/>");
			}
			output.Write("</td></tr>");
		}

		private static void RenderQuickInfoID(HtmlTextWriter output, Sitecore.Data.Items.Item item)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			output.Write("<tr><td>");
			output.Write(Translate.Text("Item ID:"));
			output.Write("</td><td>");
			output.Write(
				"<input class=\"scEditorHeaderQuickInfoInput\" readonly=\"readonly\" onclick=\"javascript:this.select();return false\" value=\"" +
				item.ID + "\"/>");
			output.Write("</td></tr>");
		}

		private static void RenderQuickInfoItemKey(HtmlTextWriter output, Sitecore.Data.Items.Item item)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			output.Write("<tr><td>");
			output.Write(Translate.Text("Item Name:"));
			output.Write("</td><td>");
			output.Write(item.Name);
			if (item.DisplayName != item.Name)
			{
				output.Write(" - ");
				output.Write(Translate.Text("Display Name: "));
				output.Write(item.DisplayName);
			}
			output.Write("</td></tr>");
		}

		private static void RenderQuickInfoOwner(HtmlTextWriter output, Sitecore.Data.Items.Item item)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			output.Write("<tr><td>");
			output.Write(Translate.Text("Item Owner:"));
			output.Write("</td><td>");
			string str = item[FieldIDs.Owner];
			if (string.IsNullOrEmpty(str))
			{
				str = Translate.Text("[unknown]");
			}
			output.Write(
				"<input class=\"scEditorHeaderQuickInfoInputID\" readonly=\"readonly\" onclick=\"javascript:this.select();return false\" value=\"" +
				str + "\"/>");
			output.Write("</td></tr>");
		}

		private static void RenderQuickInfoPath(HtmlTextWriter output, Sitecore.Data.Items.Item item)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			output.Write("<tr><td>");
			output.Write(Translate.Text("Item Path:"));
			output.Write("</td><td>");
			output.Write(
				"<input class=\"scEditorHeaderQuickInfoInput\" readonly=\"readonly\" onclick=\"javascript:this.select();return false\" value=\"" +
				item.Paths.Path + "\"/>");
			output.Write("</td></tr>");
		}

		private static void RenderQuickInfoTemplate(HtmlTextWriter output, Sitecore.Data.Items.Item item)
		{
			Sitecore.Data.Items.Item item2;
			Sitecore.Diagnostics.Assert.ArgumentNotNull(output, "output");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
			output.Write("<tr><td>");
			output.Write(Translate.Text("Template:"));
			output.Write("</td><td>");
			using (new SecurityDisabler())
			{
				item2 = item.Database.GetItem(item.TemplateID);
			}
			bool flag = (item2 != null) && (CommandManager.QueryState("shell:edittemplate", item) == CommandState.Enabled);
			if (flag)
			{
				output.Write("<a href=\"#\" onclick=\"javascript:scForm.postRequest('','','','shell:edittemplate');return false\">");
			}
			if (item2 != null)
			{
				output.Write(item2.Paths.Path);
			}
			else
			{
				output.Write(Translate.Text("[template no longer exists]"));
			}
			if (flag)
			{
				output.Write("</a>");
			}
			output.Write(" - ");
			output.Write(
				"<input class=\"scEditorHeaderQuickInfoInputID\" readonly=\"readonly\" onclick=\"javascript:this.select();return false\" value=\"" +
				item.TemplateID + "\"/>");
			output.Write("</td></tr>");
		}

		private static void RenderWarning(RenderContentEditorArgs args, System.Web.UI.Control parent,
		                                  GetContentEditorWarningsArgs.ContentEditorWarning warning)
		{
			Sitecore.Diagnostics.Assert.ArgumentNotNull(args, "args");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(parent, "parent");
			Sitecore.Diagnostics.Assert.ArgumentNotNull(warning, "warning");
			HtmlTextWriter writer = new HtmlTextWriter(new StringWriter());
			writer.Write("<div class=\"scEditorWarningPanel\">");
			writer.Write(
				"<table border=\"0\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" class=\"scEditorWarningPanelTable\">");
			writer.Write("<tr>");
			writer.Write("<td valign=\"top\">");
			writer.Write(new ImageBuilder {Src = warning.Icon, Class = "scEditorSectionCaptionIcon"}.ToString());
			writer.Write("</td>");
			writer.Write("<td width=\"100%\">");
			writer.Write("<div class=\"scEditorWarningTitle\">");
			writer.Write(warning.Title);
			writer.Write("</div>");
			if (!string.IsNullOrEmpty(warning.Text))
			{
				writer.Write("<div class=\"scEditorWarningHelp\">");
				writer.Write(warning.Text);
				writer.Write("</div>");
			}
			if (warning.Options.Count > 0)
			{
				writer.Write("<div class=\"scEditorWarningOptions\">");
				writer.Write("<ul class=\"scEditorWarningOptionsList\">");
				foreach (Pair<string, string> pair in warning.Options)
				{
					writer.Write("<li class=\"scEditorWarningOptionBullet\">");
					string message = pair.Part2;
					if (!message.StartsWith("javascript:"))
					{
						message = Context.ClientPage.GetClientEvent(message);
					}
					writer.Write("<a href=\"#\" class=\"scEditorWarningOption\" onclick=\"");
					writer.Write(message);
					writer.Write("\">");
					writer.Write(Translate.Text(pair.Part1));
					writer.Write("</a>");
					writer.Write("</li>");
				}
				writer.Write("</ul>");
				writer.Write("</div>");
			}
			writer.Write("</td>");
			writer.Write("</tr>");
			writer.Write("</table>");
			writer.Write("</div>");
			args.EditorFormatter.AddLiteralControl(parent, writer.InnerWriter.ToString());
		}

		protected virtual bool ShouldShowHeader()
		{
			return (this.RenderHeader && UserOptions.ContentEditor.ShowHeader);
		}

		public Sitecore.Web.UI.Sheer.ClientPage ClientPage
		{
			get
			{
				if (this._clientPage == null)
				{
					this._clientPage = Context.ClientPage;
				}
				return this._clientPage;
			}
			set
			{
				Sitecore.Diagnostics.Assert.ArgumentNotNull(value, "value");
				this._clientPage = value;
			}
		}

		public Sitecore.Data.Database ContentDatabase
		{
			get
			{
				if (this._contentDatabase == null)
				{
					this._contentDatabase = Context.ContentDatabase;
				}
				return this._contentDatabase;
			}
			set
			{
				Sitecore.Diagnostics.Assert.ArgumentNotNull(value, "value");
				this._contentDatabase = value;
			}
		}

		public Sitecore.Data.Database Database
		{
			get
			{
				if (this._database == null)
				{
					this._database = Context.Database;
				}
				return this._database;
			}
			set
			{
				Sitecore.Diagnostics.Assert.ArgumentNotNull(value, "value");
				this._database = value;
			}
		}

		public Hashtable FieldInfo
		{
			get { return this._fieldInfo; }
			set { this._fieldInfo = value; }
		}

		public string ID
		{
			get { return this._id; }
			set
			{
				Sitecore.Diagnostics.Assert.ArgumentNotNull(value, "value");
				this._id = value;
			}
		}

		public List<string> SectionsToRender
		{
			get { return this._sectionsToRender; }
			set
			{
				Assert.ArgumentNotNull(value, "value");
				this._sectionsToRender = value;
			}
		}

		public Dictionary<string, string> NewSectionNames
		{
			get { return this._newSectionNames; }
			set
			{
				Assert.ArgumentNotNull(value, "value");
				this._newSectionNames = value;
			}
		}

		protected bool IsAdministrator
		{
			get
			{
				if (this._isAdministrator == -1)
				{
					this._isAdministrator = Context.IsAdministrator ? 1 : 0;
				}
				return (this._isAdministrator == 1);
			}
		}

		private static bool IsQuickInfoSectionCollapsed
		{
			get
			{
				UrlString str = new UrlString(Registry.GetString("/Current_User/Content Editor/Sections/Collapsed"));
				string str2 = str["QuickInfo"];
				return (string.IsNullOrEmpty(str2) || (str2 == "1"));
			}
		}

		public Sitecore.Data.Items.Item Item
		{
			get { return this._item; }
		}

		public Sitecore.Globalization.Language Language
		{
			get
			{
				if (this._language == null)
				{
					this._language = Context.Language;
				}
				return this._language;
			}
			set
			{
				Sitecore.Diagnostics.Assert.ArgumentNotNull(value, "value");
				this._language = value;
			}
		}

		public bool RenderHeader { get; set; }

		public bool RenderTabsAndBars { get; set; }

		public bool RenderWarnings { get; set; }

		protected bool ShowDataFieldsOnly { get; set; }

		protected bool ShowInputBoxes { get; set; }

		protected bool ShowSections { get; set; }

		public Sitecore.Security.Accounts.User User
		{
			get
			{
				if (this._user == null)
				{
					this._user = Context.User;
				}
				return this._user;
			}
		}

		private class EditorTab
		{
			private string _controls;
			private string _header;
			private string _icon;
			private string _id;
			private string _url;

			public bool Closeable { get; set; }

			public string Controls
			{
				get { return (this._controls ?? string.Empty); }
				set
				{
					Sitecore.Diagnostics.Assert.ArgumentNotNullOrEmpty(value, "value");
					this._controls = value;
				}
			}

			public string Header
			{
				get { return this._header; }
				set
				{
					Sitecore.Diagnostics.Assert.ArgumentNotNull(value, "value");
					this._header = value;
				}
			}

			public string Icon
			{
				get { return this._icon; }
				set
				{
					Sitecore.Diagnostics.Assert.ArgumentNotNull(value, "value");
					this._icon = value;
				}
			}

			public string Id
			{
				get { return this._id; }
				set
				{
					Sitecore.Diagnostics.Assert.ArgumentNotNull(value, "value");
					this._id = value;
				}
			}

			public bool RefreshOnShow { get; set; }

			public string Url
			{
				get { return this._url; }
				set
				{
					Sitecore.Diagnostics.Assert.ArgumentNotNull(value, "value");
					this._url = value;
				}
			}
		}

		[DebuggerDisplay("Field: {ItemField}")]
		public class Field
		{
			protected string _controlID;
			private Sitecore.Data.Fields.Field _itemField;
			private Sitecore.Data.Templates.TemplateField _templateField;
			private string _value;

			public Field(Sitecore.Data.Fields.Field itemField, Sitecore.Data.Templates.TemplateField templateField)
				: this(itemField, templateField, itemField.Value)
			{
			}

			public Field(Sitecore.Data.Fields.Field itemField, Sitecore.Data.Templates.TemplateField templateField, string value)
			{
				this._itemField = itemField;
				this._templateField = templateField;
				this._controlID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("FIELD");
				this._value = value;
			}

			public Editor.Field Clone(string controlID)
			{
				Editor.Field field = new Editor.Field(this.ItemField, this.TemplateField);
				_controlID = controlID;

				return field;
			}

			public string ControlID
			{
				get { return this._controlID; }
			}

			public Sitecore.Data.Fields.Field ItemField
			{
				get { return this._itemField; }
			}

			public Sitecore.Data.Templates.TemplateField TemplateField
			{
				get { return this._templateField; }
			}

			public string Value
			{
				get { return this._value; }
				set
				{
					Sitecore.Diagnostics.Assert.ArgumentNotNull(value, "value");
					this._value = value;
				}
			}
		}

		public class Fields : List<Editor.Field>
		{
		}
	}
}