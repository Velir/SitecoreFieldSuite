using System.Web.UI;
using System.Web.UI.HtmlControls;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditor;

namespace FieldSuite.Editors.ContentEditor
{
	public class RenderFieldSuiteContentEditor
	{
		// Methods
		public void Process(RenderContentEditorArgs args)
		{
			Assert.ArgumentNotNull(args, "args");

			FieldSuiteEditorFormatter advisoryEditorFormatter = new FieldSuiteEditorFormatter();
			advisoryEditorFormatter.IsFieldEditor = args.EditorFormatter.IsFieldEditor;
			advisoryEditorFormatter.RenderSections(args);

			AddCustomScripts();
			AddCustomStyles();
		}

		private static void AddCustomStyles()
		{
			var ss1 = MakeStyleTag("/sitecore modules/shell/field suite/styles/fieldsuite.fields.css");
			var ss2 = MakeStyleTag("/sitecore modules/shell/field suite/styles/fieldsuite.fields.images.css");
			var ss3 = MakeStyleTag("/sitecore modules/shell/field suite/styles/fieldsuite.fields.droplink.css");
			var ss4 = MakeStyleTag("/sitecore modules/shell/field suite/styles/fieldsuite.fields.treelist.css");
			var ss5 = MakeStyleTag("/sitecore modules/shell/field suite/styles/fieldsuite.fields.generallinks.css");
			AddControlToHeader(ss1);
			AddControlToHeader(ss2);
			AddControlToHeader(ss3);
			AddControlToHeader(ss4);
			AddControlToHeader(ss5);
		}
		private static void AddCustomScripts()
		{
			
			var script1 = MakeScriptTag("/sitecore modules/shell/field suite/scripts/fieldsuite.fields.js");
			var script2 = MakeScriptTag("/sitecore modules/shell/field suite/scripts/fieldsuite.html.js");
			var script3 = MakeScriptTag("/sitecore modules/shell/field suite/scripts/fieldsuite.fields.images.js");
			var script4 = MakeScriptTag("/sitecore modules/shell/field suite/scripts/fieldsuite.fields.droplink.js");
			var script5 = MakeScriptTag("/sitecore modules/shell/field suite/scripts/fieldsuite.fields.droptree.js");
			var script6 = MakeScriptTag("/sitecore modules/shell/field suite/scripts/fieldsuite.fields.treelist.js");
			var script7 = MakeScriptTag("/sitecore modules/shell/field suite/scripts/fieldsuite.fields.generallinks.js");

			AddControlToHeader(script1);
			AddControlToHeader(script2);
			AddControlToHeader(script3);
			AddControlToHeader(script4);
			AddControlToHeader(script5);
			AddControlToHeader(script6);
			AddControlToHeader(script7);
		}

		private static void AddControlToHeader(Control ctrl)
		{
			Sitecore.Context.Page.Page.Header.Controls.Add(ctrl);
		}

		private static HtmlLink MakeStyleTag(string path)
		{

			HtmlLink ctrl = new HtmlLink {Href = path};
			ctrl.Attributes.Add("rel", "stylesheet");
			ctrl.Attributes.Add("type", "text/css");
			return ctrl;

		}

		private static HtmlGenericControl MakeScriptTag(string path)
		{
			HtmlGenericControl ctrl = new HtmlGenericControl("script");
			ctrl.Attributes.Add("type", "text/JavaScript");
			ctrl.Attributes.Add("language", "javascript");
			ctrl.Attributes.Add("src", path);
			return ctrl;
		}
	}
}