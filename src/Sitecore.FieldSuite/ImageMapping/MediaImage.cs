using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Resources.Media;

using Sitecore.Data.Items;
using Sitecore.SharedSource.Commons.Extensions;

namespace Sitecore.SharedSource.FieldSuite.ImageMapping
{
	public class MediaImage : AFieldSuiteImage
	{
		public MediaImage(FieldSuiteImageArgs args) : base(args)
		{
			if (args == null || args.InnerItem.IsNull())
			{
				return;
			}

			if(!args.InnerItem.Paths.IsMediaItem)
			{
				return;
			}

			MediaItem mediaItem = args.InnerItem;

			MediaUrlOptions options = new MediaUrlOptions();
			options.AbsolutePath = true;
			options.UseItemPath = true;

			Title = mediaItem.DisplayName;
			ImageUrl = MediaManager.GetMediaUrl(mediaItem, options);
		}
	}
}