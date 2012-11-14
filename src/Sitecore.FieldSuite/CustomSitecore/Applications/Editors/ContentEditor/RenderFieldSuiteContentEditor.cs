using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditor;

namespace Sitecore.SharedSource.FieldSuite.CustomSitecore.Applications.Editors.ContentEditor
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
		}
	}
}