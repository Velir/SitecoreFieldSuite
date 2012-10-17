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
	public class AFieldSuiteImage : IFieldSuiteImage
	{
		public AFieldSuiteImage(Item item, string titleField, string imageField)
		{
			if (string.IsNullOrEmpty(imageField))
			{
				return;
			}

			//get title, fall back to display name
			Title = item.DisplayName;
			if (!string.IsNullOrEmpty(titleField))
			{
				Title = item[titleField];
			}

			//image field source
			ImageField imageFieldObject = item.Fields[imageField];
			if (imageFieldObject == null || imageFieldObject.MediaItem == null)
			{
				return;
			}

			MediaUrlOptions options = new MediaUrlOptions();
			options.AbsolutePath = true;
			options.UseItemPath = true;

			ImageUrl = MediaManager.GetMediaUrl(imageFieldObject.MediaItem, options);
			if (string.IsNullOrEmpty(ImageUrl))
			{
				return;
			}
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
