using System;
using System.Collections.Generic;
using System.Web.UI;
using log4net.Repository.Hierarchy;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Resources;
using Velir.SitecoreLibrary.Extensions;
using FieldSuite.Controls.ListItem;
using FieldSuite.FieldGutter;
using FieldSuite.ImageMapping;

namespace FieldSuite.Types
{
	public class ImagesField : AFieldSuiteField
	{
		private Database _masterDatabase;

		/// <summary>
		/// Sitecore Database
		/// </summary>
		private Database MasterDatabase
		{
			get
			{
				if (_masterDatabase != null)
				{
					return _masterDatabase;
				}

				if (Sitecore.Context.ContentDatabase == null)
				{
					_masterDatabase = Factory.GetDatabase("master");
				}

				_masterDatabase = Sitecore.Context.ContentDatabase;
				return _masterDatabase;
			}
		}

		/// <summary>
		/// Field Values
		/// </summary>
		public string[] FieldValues
		{
			get
			{
				string fieldValue = this.GetValue();
				if (string.IsNullOrEmpty(fieldValue))
				{
					return new string[0];
				}

				//get rotating images
				string[] fieldValues = fieldValue.Split('|');
				if (fieldValues.Length == 0)
				{
					return new string[0];
				}

				return fieldValues;
			}
		}

		/// <summary>
		///Folder to store the image items in
		/// </summary>
		public virtual string ImageItemLocationId
		{
			get
			{
				string source = base.Source;
				if (string.IsNullOrEmpty(source))
				{
					return string.Empty;
				}

				string[] sourceValues = source.Split('|');
				if (sourceValues.Length < 2)
				{
					return string.Empty;
				}

				string itemId = sourceValues[1];
				if (string.IsNullOrEmpty(itemId))
				{
					return string.Empty;
				}

				return itemId;
			}
		}

		/// <summary>
		///Name of the Field
		/// </summary>
		public virtual string FieldName
		{
			get
			{
				string source = base.Source;
				if (string.IsNullOrEmpty(source))
				{
					return string.Empty;
				}

				string[] sourceValues = source.Split('|');
				if (sourceValues.Length == 0)
				{
					return string.Empty;
				}

				return sourceValues[0];
			}
		}

		/// <summary>
		///Template Id for Items to be created
		/// </summary>
		public virtual string TemplateId
		{
			get
			{
				string source = base.Source;
				if (string.IsNullOrEmpty(source))
				{
					return string.Empty;
				}

				string[] sourceValues = source.Split('|');
				if (sourceValues.Length < 3)
				{
					return string.Empty;
				}

				return sourceValues[2];
			}
		}

		/// <summary>
		/// Current Field Type
		/// </summary>
		public override Item FieldTypeItem
		{
			get
			{
				Database database = Sitecore.Data.Database.GetDatabase("core");
				if (database == null)
				{
					Logger.Error("FieldSuite.ImagesField - Unable to find Field Type Item");
					return null;
				}

				return database.GetItem("{1BFD07E7-DF30-4C40-A6E9-E8C5409D5768}");
			}
		}

		protected override void Render(HtmlTextWriter output)
		{
			Item currentItem = MasterDatabase.GetItem(ItemID);
			if (currentItem.IsNull())
			{
				Logger.Error("FieldSuite.ImagesField - Unable to find current item while rendering: " + ItemID);
				return;
			}

			//iterate over images
			string rotatingImageHtml = string.Empty;
			int i = 0;
			foreach (string itemId in FieldValues)
			{
				if (string.IsNullOrEmpty(itemId))
				{
					continue;
				}

				//image wrapper
				rotatingImageHtml += RenderItem(itemId);
				i++;
			}

			int wrappingWidth = i * 185;

			//output to user that there are none selected)
			if (string.IsNullOrEmpty(rotatingImageHtml))
			{
				rotatingImageHtml = "<div class=\"velirImagesFieldNoValue\">No Rotating Images have been selected.</div>";
			}

			//write back to output for rendering
			string hiddenField = "<input id=\"" + this.ID + "_Value\" type=\"hidden\" value=\"" + StringUtil.EscapeQuote(this.Value) + "\" />";
			output.Write(string.Format("<div id=\"{2}\" class=\"scContentControlHtml rotatingContentControl\" style=\"\">{3}<div class=\"velirImageItems\" style=\"width:{1}px;\">{0}</div></div>", rotatingImageHtml, wrappingWidth, this.ID, hiddenField));
		}

		/// <summary>
		/// Renders an item
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public string RenderItem(string itemId)
		{
			return RenderItem(itemId, true);
		}

		/// <summary>
		/// Renders an item
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public override string RenderItem(string itemId, bool selectedItem)
		{
			if (string.IsNullOrEmpty(itemId))
			{
				return string.Empty;
			}

			FieldSuiteImageListItem listItem = new FieldSuiteImageListItem();
			Item item = Sitecore.Context.ContentDatabase.GetItem(itemId);
			if (item.IsNull())
			{
				return listItem.RenderItemNotFound(itemId, this.ID);
			}

			//get interface item
			IFieldSuiteImage fieldSuiteImage = FieldSuiteImageFactory.GetFieldSuiteImage(item);
			if (fieldSuiteImage == null)
			{
				return listItem.RenderItemConfigured(itemId, this.ID);
			}

			//set default
			string imageSrc = "/sitecore modules/shell/field suite/images/unknown.png";

			//set to image of the item
			if (!string.IsNullOrEmpty(fieldSuiteImage.ImageUrl))
			{
				//setup image and sitecore click event
				imageSrc = fieldSuiteImage.ImageUrl.ToLower();
			}

			//add thumbnail parameter
			string parameters = "w=125&h=125&thn=true&db=" + MasterDatabase.Name.ToLower();
			if (imageSrc.Contains("?"))
			{
				imageSrc += "&" + parameters;
			}
			else
			{
				imageSrc += "?" + parameters;
			}

			string titleText = string.Format("{0}: {1}", item.Name, item.Paths.FullPath);

			//if the form is readonly
			string onclick = "";
			if (!this.ReadOnly)
			{
				onclick = string.Format("javascript:FieldSuite.Fields.ImagesField.ToggleItem(this, '{0}');", this.ID);
			}

			//setup description
			string description = string.Empty;
			if (!string.IsNullOrEmpty(fieldSuiteImage.Title))
			{
				description = fieldSuiteImage.Title;
				if (description.Length > 13)
				{
					description = description.Substring(0, 13) + "...";
				}
			}

			listItem.Text = description;
			listItem.HoverText = titleText;
			listItem.ReadOnly = this.ReadOnly;
			listItem.ItemClick = onclick;
			listItem.Parameters = new List<object>();
			listItem.Parameters.Add(string.Format("<img border=\"0\" src=\"{0}\">", imageSrc));

			//for performance reason limit field gutter
			Int32 renderCount = RenderItemCount;
			if (selectedItem)
			{
				renderCount = RenderSelectedItemCount;
			}

			//for performance reason limit field gutter
			bool useFieldGutter = false;
			IFieldGutterProcessor fieldGutterProcessor = FieldGutterProcessorFactory.GetProcessor();
			if (fieldGutterProcessor != null)
			{
				Int32 maxCount = fieldGutterProcessor.MaxCount;
				if (maxCount != 0 && renderCount <= maxCount)
				{
					useFieldGutter = true;
				}
			}

			//return list item as html
			if (selectedItem)
			{
				RenderSelectedItemCount++;
			}
			else
			{
				RenderItemCount++;
			}

			return listItem.Render(item, item.ID.ToString(), this.ID, useFieldGutter);
		}
	}
}