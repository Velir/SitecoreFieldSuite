using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sitecore.Data.Items;

namespace FieldSuite.ImageMapping
{
	public interface IFieldSuiteImage
	{
		/// <summary>
		/// Title Field
		/// </summary>
		string Title { get; set; }

		/// <summary>
		/// Image Url
		/// </summary>
		string ImageUrl { get; set; }
	}
}