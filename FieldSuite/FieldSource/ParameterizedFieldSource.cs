using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Diagnostics;
using Sitecore.Web.UI.HtmlControls.Data;

using Sitecore.Data.Items;
using Velir.SitecoreLibrary.Extensions;
using Sitecore.Data.Query;
using Sitecore.Web;
using Sitecore;

namespace FieldSuite.FieldSource
{
	/// <summary>
	/// A ParameterizedFieldSource contains a querystring of information to setup the string.
	/// you can find more information here: http://www.sitecore.net/Community/Technical-Blogs/John-West-Sitecore-Blog/Posts/2012/09/Apply-Treelist-Inclusion-and-Exclusion-Criteria-Dynamically-with-the-Sitecore-ASPNET-CMS.aspx
	/// </summary>
	public class ParameterizedFieldSource : AbstractFieldSource
	{
		#region Properties

		public override string Source { get; set; }
		
		public string DataSource { get; set; }

		public string DatabaseName { get; set; }

		public bool AllowMultipleSelection { get; set; }

		public string IncludeItemsForDisplaySource;
		public IEnumerable<string> IncludeItemsForDisplay { get; set; }

		public string ExcludeItemsForDisplaySource;
		public IEnumerable<string> ExcludeItemsForDisplay { get; set; }

		public string IncludeTemplatesForSelectionSource;
		public IEnumerable<string> IncludeTemplatesForSelection { get; set; }

		public string ExcludeTemplatesForSelectionSource;
		public IEnumerable<string> ExcludeTemplatesForSelection { get; set; }

		public string IncludeTemplatesForDisplaySource;
		public IEnumerable<string> IncludeTemplatesForDisplay { get; set; }

		public string ExcludeTemplatesForDisplaySource;
		public IEnumerable<string> ExcludeTemplatesForDisplay { get; set; }

		#endregion Properties

		#region Constructors

		public ParameterizedFieldSource(string source) {

			Source = source;
			
			DataSource = GetParam("DataSource");

			DatabaseName = GetParam("DatabaseName").ToLower();

			AllowMultipleSelection = (GetParam("AllowMultipleSelection").Contains("yes")) ? true : false;

			IncludeItemsForDisplaySource = GetParam("IncludeItemsForDisplay");
			IncludeItemsForDisplay = SetupEnums(IncludeItemsForDisplaySource);

			ExcludeItemsForDisplaySource = GetParam("ExcludeItemsForDisplay");
			ExcludeItemsForDisplay = SetupEnums(ExcludeItemsForDisplaySource); 
			
			IncludeTemplatesForSelectionSource = GetParam("IncludeTemplatesForSelection");
			IncludeTemplatesForSelection = SetupEnums(IncludeTemplatesForSelectionSource); 
			
			ExcludeTemplatesForSelectionSource = GetParam("ExcludeTemplatesForSelection");
			ExcludeTemplatesForSelection = SetupEnums(ExcludeTemplatesForSelectionSource); 
			
			IncludeTemplatesForDisplaySource = GetParam("IncludeTemplatesForDisplay");
			IncludeTemplatesForDisplay = SetupEnums(IncludeTemplatesForDisplaySource); 
			
			ExcludeTemplatesForDisplaySource = GetParam("ExcludeTemplatesForDisplay");
			ExcludeTemplatesForDisplay = SetupEnums(ExcludeTemplatesForDisplaySource);
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Manages breaking up the comma delimited list of strings into an IEnumerable<string>
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		private IEnumerable<string> SetupEnums(string source) {
			IEnumerable<string> values = Enumerable.Empty<string>();
			string[] items = source.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
			if (items.Any())
				values = values.Concat(items);
			return values;
		}

		/// <summary>
		/// Manages breaking up the source string by the parameter provided.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private string GetParam(string name) {
			return StringUtil.ExtractParameter(name, this.Source).Trim();
		}

		#endregion Methods
	}
}