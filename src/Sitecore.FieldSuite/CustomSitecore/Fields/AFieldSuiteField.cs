using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.SharedSource.FieldSuite.Controls.ListItem;
using Sitecore.SharedSource.FieldSuite.FieldGutter;
using Sitecore.SharedSource.FieldSuite.FieldSource;
using log4net;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.FieldSuite.CustomSitecore.Fields
{
	public abstract class AFieldSuiteField : MultilistEx, IFieldSuiteField
	{
		private Item _fieldTypeItem;
		private Item _currentItem;
		private List<Item> _availableItems;
		private List<string> _selectedItems;
		private bool _showSortIcons;
		private string _fieldGutter;
		private IFieldSource _fieldSource;

		public string FieldTypeItemId { get; set; }

		/// <summary>
		/// Current Field Type
		/// </summary>
		public virtual Item FieldTypeItem
		{
			get { return _fieldTypeItem; }
			set { _fieldTypeItem = value; }
		}

		/// <summary>
		/// Current Field Type
		/// </summary>
		public virtual Item CurrentItem
		{
			get
			{
				if (_currentItem.IsNotNull())
				{
					return _currentItem;
				}

				if (string.IsNullOrEmpty(this.ItemID))
				{
					return null;
				}

				_currentItem = Sitecore.Context.ContentDatabase.GetItem(this.ItemID);
				return _currentItem;
			}
			set
			{
				_currentItem = value;
			}
		}

		/// <summary>
		/// List of available items to select from
		/// </summary>
		public virtual List<Item> AvailableItems
		{
			get
			{
				if (_availableItems != null)
				{
					return _availableItems;
				}

				if (CurrentItem.IsNull())
				{
					return null;
				}

				IDictionary dictionary;
				ArrayList list;
				Item[] items = this.GetItems(CurrentItem);

				if (!string.IsNullOrEmpty(Source) && (Source.ToLower() == "/sitecore/content/" || Source.ToLower() == "/sitecore/content/home/"))
				{
					_availableItems = new List<Item>();
					return _availableItems;
				}

				string message = string.Format("FieldSuite.AvailableItems - ItemId: {0}, TemplateId: {1}, Source: {2}", CurrentItem.ID, CurrentItem.TemplateID, Source);
				using (new LongRunningOperationWatcher(Settings.Profiling.RenderFieldThreshold, message, new string[0]))
				{
					//if nothing is returned and the source is empty, default back to home nodes children.
					if ((items == null || items.Length == 0) && string.IsNullOrEmpty(this.Source))
					{
						Item homeItem = Sitecore.Context.ContentDatabase.GetItem("/sitecore/content/home");
						if (homeItem.IsNotNull())
						{
							_availableItems = homeItem.GetChildren().ToList();
							return _availableItems;
						}
					}

					this.GetSelectedItems(items, out list, out dictionary);
					_availableItems = new List<Item>();
					foreach (DictionaryEntry entry in dictionary)
					{
						Item item = entry.Value as Item;
						if (item.IsNotNull())
						{
							_availableItems.Add(item);
						}
					}

					return _availableItems;
				}
			}
		}

		/// <summary>
		/// List of selected items ids
		/// </summary>
		public virtual List<string> SelectedItems
		{
			get
			{
				if (_selectedItems != null)
				{
					return _selectedItems;
				}

				if (CurrentItem.IsNull())
				{
					return new List<string>();
				}

				string message = string.Format("FieldSuite.SelectedItems - ItemId: {0}, TemplateId: {1}, Source: {2}", CurrentItem.ID, CurrentItem.TemplateID, Source);
				using (new LongRunningOperationWatcher(Settings.Profiling.RenderFieldThreshold, message, new string[0]))
				{
					IDictionary dictionary;
					ArrayList list;
					Item[] items = this.GetItems(CurrentItem);
					this.GetSelectedItems(items, out list, out dictionary);
					List<string> fieldValues = new List<string>();

					if (string.IsNullOrEmpty(this.Value))
					{
						return new List<string>();
					}

					fieldValues = this.Value.Split('|').ToList();
					if (fieldValues.Count == 0)
					{
						return new List<string>();
					}

					fieldValues = fieldValues.Where(x => !string.IsNullOrEmpty(x)).ToList();

					_selectedItems = new List<string>();
					_selectedItems.AddRange(fieldValues);
					return fieldValues;
				}
			}
			set
			{
				_selectedItems = value;
			}
		}

		/// <summary>
		/// Returns the Field's Source
		/// </summary>
		public IFieldSource FieldSource
		{
			get
			{
				if (_fieldSource != null)
				{
					return _fieldSource;
				}

				if (CurrentItem.IsNull())
				{
					return null;
				}

				IFieldSource fieldSource = FieldSourceFactory.GetFieldSource(this.Source);
				if (fieldSource == null)
				{
					return null;
				}

				_fieldSource = fieldSource;
				return _fieldSource;
			}
		}

		/// <summary>
		/// Method to Build the Html for an Available Item
		/// </summary>
		/// <returns></returns>
		public virtual string BuildAvailableItems()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<div class=\"unSelectedItems\">");

			foreach (Item item in AvailableItems)
			{
				if (item.IsNull())
				{
					continue;
				}

				sb.Append(RenderItem(item, false));
			}

			sb.Append("</div>");
			return sb.ToString();
		}

		/// <summary>
		/// Method to Build the Html for the Selected Items
		/// </summary>
		/// <returns></returns>
		public virtual string BuildSelectedItems()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(string.Format("<div id=\"{0}_SelectedItems\" class=\"selectedItems\">", this.ID));

			List<string> selectedItems = SelectedItems;
			if (selectedItems != null && selectedItems.Count > 0)
			{
				foreach (string selectedItemId in SelectedItems)
				{
					if (string.IsNullOrEmpty(selectedItemId))
					{
						continue;
					}

					string renderHtml = RenderItem(selectedItemId, true);
					if (string.IsNullOrEmpty(renderHtml))
					{
						continue;
					}

					sb.Append(renderHtml);
				}
			}

			sb.Append("</div>");
			return sb.ToString();
		}

		private Int32 _renderItemCount;
		public Int32 RenderItemCount
		{
			get { return _renderItemCount; }
			set { _renderItemCount = value; }
		}

		private Int32 _renderSelectedItemCount;
		public Int32 RenderSelectedItemCount
		{
			get { return _renderSelectedItemCount; }
			set { _renderSelectedItemCount = value; }
		}

		/// <summary>
		/// Renders an item
		/// </summary>
		/// <param name="item"></param>
		/// <param name="selectedItem"></param>
		/// <returns></returns>
		public virtual string RenderItem(Item item, bool selectedItem)
		{
			FieldSuiteListItem listItem = new FieldSuiteListItem();
			if (item.IsNull())
			{
				//return not found list item template
				listItem.ShowAddRemoveButton = true;
				return listItem.RenderItemNotFound(item.ID.ToString(), this.ID);
			}

			listItem.ShowAddRemoveButton = true;
			listItem.ButtonClick = string.Format("FieldSuite.Fields.ToggleItem('{0}', this);", this.ID);
			listItem.ItemClick = "FieldSuite.Fields.SelectItem(this);";
			listItem.SelectedClass = "velirFieldSelected";
			listItem.ReadOnly = this.ReadOnly;
			listItem.Text = item.DisplayName;
			listItem.HoverText = item.Paths.FullPath;

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

		/// <summary>
		/// Renders an item
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="selectedItem"></param>
		/// <returns></returns>
		public virtual string RenderItem(string itemId, bool selectedItem)
		{
			if (string.IsNullOrEmpty(itemId))
			{
				return string.Empty;
			}

			Item item = Sitecore.Context.ContentDatabase.GetItem(itemId);
			if (item.IsNull())
			{
				//return not found list item template
				FieldSuiteListItem listItem = new FieldSuiteListItem();
				listItem.ShowAddRemoveButton = true;
				return listItem.RenderItemNotFound(itemId, this.ID);
			}

			return RenderItem(item, selectedItem);
		}

		protected static ILog _logger;
		protected static ILog Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = LogManager.GetLogger(typeof(AFieldSuiteField));
				}
				return _logger;
			}
		}
	}
}