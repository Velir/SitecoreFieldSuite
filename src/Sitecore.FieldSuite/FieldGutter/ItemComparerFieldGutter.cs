using System;
using System.Collections.Generic;
using Sitecore.SharedSource.PublishedItemComparer.CustomItems.Common.ItemComparer;
using Sitecore.SharedSource.PublishedItemComparer.Domain;
using Sitecore.SharedSource.PublishedItemComparer.Utils;
using Sitecore.SharedSource.PublishedItemComparer.Validations;
using log4net;
using Sitecore.Data;

namespace Sitecore.SharedSource.FieldSuite.FieldGutter
{
	public class ItemComparerFieldGutter : IFieldGutter
	{
		/// <summary>
		/// Uses the Published Item Comparer
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public string Execute(FieldGutterArgs args)
		{
			if (args.InnerItem == null)
			{
				return string.Format("<span title=\"The item could not be retrieved from Sitecore.\"><img class=\"fieldGutterItem\" src=\"/sitecore modules/shell/field suite/images/bullet_ball_red.png\"/></span>");
			}

			if (args.InnerItem.Database != null && args.InnerItem.Database.Name.ToLower() == "core")
			{
				return string.Empty;
			}

			//verify settings item exists
			ItemComparerSettingsItem settingsItem = ItemComparerSettingsItem.GetSettingsItem();
			if (settingsItem == null)
			{
				Logger.Error("Published Item Comparer: The Settings Item Could not be retrieved.");
				return "<span title=\"The settings item could not be retrieved from Sitecore.\"><img class=\"fieldGutterItem\" src=\"/sitecore modules/shell/field suite/images/bullet_ball_red.png\"/></span>";
			}

			//verify target database
			Database targetDatabase = ItemComparerUtil.GetTargetDatabase();
			if (targetDatabase == null)
			{
				Logger.Error("Published Item Comparer: The Target Database Could not be retrieved.");
				return "<span title=\"The target database could not be retrieved from Sitecore.\"><img class=\"fieldGutterItem\" src=\"/sitecore modules/shell/field suite/images/bullet_ball_red.png\"/></span>";
			}

			try
			{
				ItemComparerContext context = new ItemComparerContext();
				context.Item = args.InnerItem;
				context.ItemComparerSettingsItem = settingsItem;
				context.TargetDatabase = targetDatabase;

				ItemValidator itemValidator = new ItemValidator();
				List<string> validations = itemValidator.Validate(context);
				if (validations != null && validations.Count > 0)
				{
					return string.Format("<span title=\"The item did not pass validation.\"><a href=\"#\" style=\"border:0;padding:0;\" class=\"itemComparerGutterLink\" onclick=\"FieldSuite.Fields.OpenItemComparer('{0}','{1}')\"><img class=\"fieldGutterItem\" src=\"/sitecore modules/shell/field suite/images/bullet_ball_red.png\"/></a></span>", args.InnerItem.ID, args.FieldId);
				}

				return "<span title=\"The item passed validation.\"><img class=\"fieldGutterItem\" src=\"/sitecore modules/shell/field suite/images/bullet_ball_green.png\"/></span>";
			}
			catch (Exception e)
			{
				Logger.Error("Field Gutter - Published Item Comparer: Error trying to validate");
				Logger.Error(e.InnerException);
				Logger.Error(e.Message);
			}

			return string.Empty;
		}

		/// <summary>
		/// Max Count, for performance reasons
		/// </summary>
		public int MaxCount { get; set; }

		private static ILog _logger;
		private static ILog Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = LogManager.GetLogger(typeof(ItemComparerFieldGutter));
				}
				return _logger;
			}
		}
	}
}