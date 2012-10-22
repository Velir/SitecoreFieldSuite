using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Fields;
using Sitecore.Resources.Media;
using Velir.SitecoreLibrary.Extensions;
using Sitecore.Data.Items;

namespace FieldSuite.ImageMapping
{
	public abstract class AFieldSuiteImage : IFieldSuiteImage
	{
		protected AFieldSuiteImage(FieldSuiteImageArgs args)
		{
		}

		/// <summary>
		/// Title Field
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Image Url
		/// </summary>
		public string ImageUrl { get; set; }
	}
}
