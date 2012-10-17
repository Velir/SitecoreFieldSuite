using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Resources.Media;

using Sitecore.Data.Items;

namespace FieldSuite.ImageMapping
{
	public class MediaImage : IFieldSuiteImage
	{
		public MediaImage(Item item, string titleField, string imageField)
		{
			if (item == null)
			{
				return;
			}

			MediaItem mediaItem = item;

			MediaUrlOptions options = new MediaUrlOptions();
			options.AbsolutePath = true;
			options.UseItemPath = true;

			Title = mediaItem.DisplayName;
			ImageUrl = MediaManager.GetMediaUrl(mediaItem, options);
		}

		/// <summary>
		/// Title Field
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Image Url
		/// </summary>
		public string ImageUrl{ get; set; }
	}
}