using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Fields;
using Sitecore.Resources.Media;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.Data.Items;

namespace FieldSuite.ImageMapping
{
	public class FieldSuiteImage : AFieldSuiteImage
	{
		public FieldSuiteImage(FieldSuiteImageArgs args) : base(args)
		{
			if (args.InnerItem.IsNull() || args.Node == null || args.Node.Attributes["imageField"] == null)
			{
				return;
			}

			string titleField = null;
			string imageField = args.Node.Attributes["imageField"].Value;

			if (args.Node.Attributes["titleField"] != null)
			{
				titleField = args.Node.Attributes["titleField"].Value;
			}
			
			//get title, fall back to display name
			Title = args.InnerItem.DisplayName;
			if (!string.IsNullOrEmpty(titleField))
			{
				Title = args.InnerItem[titleField];
			}

			//image field source
			ImageField imageFieldObject = args.InnerItem.Fields[imageField];
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
	}
}