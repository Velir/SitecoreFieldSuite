using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Validators;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Save;
using Sitecore.Reflection;
using Sitecore.SecurityModel;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.ContentManager;
using Sitecore.Shell.Framework;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Xml;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.SharedSource.FieldSuite.Editors;
using Sitecore.SharedSource.FieldSuite.ImageMapping;

namespace Sitecore.SharedSource.FieldSuite.Xaml
{
	public class FieldSuiteImagesEditForm : BaseForm
	{
		private readonly Database _database = Database.GetDatabase("master");
		protected Border ValidatorPanel;
		protected Literal HelpLiteral;
		protected Panel ContentPanel;
		protected Scrollbox ContentViewScrollbox;
		private static List<FieldSuiteEditor> CurrentEditors;
		protected Border ContentEditor;
		private Item _currentItem;
		private Item _primaryItem;
		private Item _parentItem;
		private TemplateItem _templateItem;
		protected GridPanel examGridPanel;
		protected DataContext TreeviewDataContext;
		protected DataTreeview dataTreeView;
		protected Toolbutton addButton;
		protected Toolbutton removeButton;
		protected Toolbutton deleteButton;
		protected Toolbutton saveButton;

		/// <summary>
		/// The current item
		/// </summary>
		public Item CurrentItem
		{
			get
			{
				// case where we have already retrieved the exam item
				if (_currentItem != null) return _currentItem;

				// get the item from querystring
				if(WebUtil.GetQueryString("id") != null)
				{
					_currentItem = _database.GetItem(WebUtil.GetQueryString("id"));
				}

				return _currentItem;
			}
			set { _currentItem = value; }
		}

		/// <summary>
		/// This is the item from which the modal was launched from
		/// </summary>
		public Item PrimaryItem
		{
			get
			{
				// case where we have already retrieved the exam item
				if (_primaryItem != null) return _primaryItem;

				// get the item
				_primaryItem = _database.GetItem(WebUtil.GetQueryString("primaryid"));
				return _primaryItem;
			}
		}

		/// <summary>
		/// Field Name from which the item was launched from
		/// </summary>
		public string FieldName
		{
			get
			{
				return WebUtil.GetQueryString("fieldname");
			}
		}

		/// <summary>
		/// Add From Existing Item
		/// </summary>
		public bool AddFromExistingItem
		{
			get
			{
				if (WebUtil.GetQueryString("addexistingitem") == null)
				{
					return false;
				}

				return (WebUtil.GetQueryString("addexistingitem") == "1");
			}
		}

		/// <summary>
		/// Parent Item
		/// </summary>
		public Item EducationMediaFolder
		{
			get
			{
				// case where we have already retrieved the exam item
				if (_parentItem != null) return _parentItem;

				string parentItemId = WebUtil.GetQueryString("parentFolder");
				if (string.IsNullOrEmpty(parentItemId))
				{
					return null;
				}

				// get the item
				_parentItem = _database.GetItem(parentItemId);
				return _parentItem;
			}
			set { _parentItem = value; }
		}

		/// <summary>
		/// Template Item to be used when creating items
		/// </summary>
		public TemplateItem TemplateItem
		{
			get
			{
				// case where we have already retrieved the exam item
				if (_templateItem != null) return _templateItem;

				// get the item
				_templateItem = _database.GetItem(WebUtil.GetQueryString("templateid"));
				return _templateItem;
			}
			set { _templateItem = value; }
		}

		protected override void OnLoad(EventArgs args)
		{
			base.OnLoad(args);
			ValidatorPanel.Attributes.Add("style", "display:none;");
			dataTreeView.OnClick += new EventHandler(dataTreeView_OnClick);

			if (Context.ClientPage.IsEvent)
			{
				return;
			}

			// Sets up the data context to show all children of the /sitecore item in the master database
			TreeviewDataContext.DataViewName = "Master";

			// in questione preview mode we do not need these values.
			TreeviewDataContext.Root = EducationMediaFolder.ID.ToString();

			if (CurrentEditors == null)
			{
				CurrentEditors = new List<FieldSuiteEditor>();
			}

			if (CurrentItem.IsNull())
			{
				//there is nothing to render except the tree so the user can select an item
				if (AddFromExistingItem)
				{
					//set form
					SetDisplay();
					return;
				}
				CreateItem();
			}

			LoadCurrentItem();
		}

		/// <summary>
		/// Sets the button and content area appropiately
		/// </summary>
		private void SetDisplay()
		{
			if (CurrentItem == null)
			{
				addButton.Visible = false;
				removeButton.Visible = false;
				deleteButton.Visible = false;
				saveButton.Visible = false;
				ResetDisplay(true);
				return;
			}

			//make sure we are on a velir image
			IFieldSuiteImage fieldSuiteImage = FieldSuiteImageFactory.GetFieldSuiteImage(CurrentItem);
			if (fieldSuiteImage == null)
			{
				addButton.Visible = false;
				removeButton.Visible = false;
				deleteButton.Visible = false;
				saveButton.Visible = false;
				ResetDisplay(true);
				return; ;
			}

			//toggle add remove buttons
			bool isCurrentItemAdded = IsCurrentItemAdded();
			addButton.Visible = !isCurrentItemAdded;
			removeButton.Visible = isCurrentItemAdded;

			deleteButton.Visible = true;
			saveButton.Visible = true;

			ResetDisplay(false);
		}

		private bool IsCurrentItemAdded()
		{
			Item currentItem = CurrentItem;
			if (currentItem.IsNull())
			{
				return false;
			}

			Item primaryItem = PrimaryItem;
			if (primaryItem.IsNull())
			{
				return false;
			}

			if (string.IsNullOrEmpty(FieldName))
			{
				return false;
			}

			string fieldValue = primaryItem[FieldName];
			return fieldValue.Contains(currentItem.ID.ToString());
		}

		protected void dataTreeView_OnClick(object sender, EventArgs e)
		{
			Item item = dataTreeView.GetSelectionItem();
			if (item == null)
			{
				return;
			}

			CurrentItem = item;
			LoadCurrentItem();
		}

		/// <summary>
		/// Creates and Sets the Item
		/// </summary>
		private void CreateItem()
		{
			if (TemplateItem == null || EducationMediaFolder == null || PrimaryItem == null)
			{
				return;
			}

			//name of the item
			string itemName = GenerateItemName();

			//create item
			using (new SecurityDisabler())
			{
				CurrentItem = EducationMediaFolder.Add(itemName, TemplateItem);

				//verify this item is added to the field values
				string fieldValue = PrimaryItem[FieldName];
				if (string.IsNullOrEmpty(fieldValue))
				{
					fieldValue = CurrentItem.ID.ToString();

					PrimaryItem.Editing.BeginEdit();
					PrimaryItem[FieldName] = fieldValue;
					PrimaryItem.Editing.EndEdit();
				}
				else if (!fieldValue.Contains(CurrentItem.ID.ToString()))
				{
					fieldValue += "|" + CurrentItem.ID;
					PrimaryItem.Editing.BeginEdit();
					PrimaryItem[FieldName] = fieldValue;
					PrimaryItem.Editing.EndEdit();
				}
			}
		}

		/// <summary>
		/// This will construct an item name based off the template
		/// </summary>
		/// <returns></returns>
		private string GenerateItemName()
		{
			if (EducationMediaFolder == null)
			{
				return "Unknown Item Name";
			}

			string newNameFormat = "{0} {1}";

			if (!EducationMediaFolder.HasChildren)
			{
				return string.Format(newNameFormat, TemplateItem.Name, 1);
			}

			int currentKindCount = EducationMediaFolder.Children.Where(x => x.TemplateID == TemplateItem.ID).Count();
			currentKindCount += 1;
			return string.Format(newNameFormat, TemplateItem.Name, currentKindCount);
		}

		/// <summary>
		/// Loads the Current Item
		/// </summary>
		private void LoadCurrentItem()
		{
			if (CurrentItem == null)
			{
				return;
			}

			examGridPanel.Visible = true;
			ValidateItem();

			//set selected on menu
			DataUri[] dataUris = new DataUri[1];
			dataUris[0] = new DataUri(CurrentItem.ID);
			TreeviewDataContext.DefaultItem = CurrentItem.ID.ToString();
			TreeviewDataContext.Folder = CurrentItem.Paths.FullPath;
			TreeviewDataContext.Selected = dataUris;
			TreeviewDataContext.AddSelected(new DataUri(CurrentItem.ID));
			TreeviewDataContext.Refresh();

			//reset form
			SetDisplay();

			//make sure we are on a velir image
			IFieldSuiteImage fieldSuiteImage = FieldSuiteImageFactory.GetFieldSuiteImage(CurrentItem);
			if (fieldSuiteImage != null)
			{
				CreateContentEditor(CurrentItem);
			}
		}

		/// <summary>
		/// Renders the Content Editor for the Current Item's Fields
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public FieldSuiteEditor CreateContentEditor(Item item)
		{
			//instantiate editor
			FieldSuiteEditor editor = new FieldSuiteEditor
			{
				RenderHeader = false,
				RenderTabsAndBars = false,
				RenderWarnings = true,
				SectionsToRender = new List<string>() { "Spotlight Image" },
				NewSectionNames = new Dictionary<string, string>()
			};

			//render item in editor
			editor.Render(CurrentItem, CurrentItem.Parent, new Hashtable(), ContentEditor, true);
			
			//transfer rendering to the client page
			Context.ClientPage.ClientResponse.SetOuterHtml(ContentEditor.ID, ContentEditor);

			CurrentEditors.Add(editor);

			return editor;
		}

		/// <summary>
		/// Resests the Form
		/// </summary>
		protected void ResetDisplay(bool addEmptyText)
		{
			// Reset the editors list incase something happens, the old data wont be overwritten
			CurrentEditors = new List<FieldSuiteEditor>();
			ContentEditor.Controls.Clear();

			if (addEmptyText)
			{
				Literal emptyTextLiteral = new Literal("<div style=\"margin: 70px 0 50px 195px;\">Please select an image item</div>");
				ContentEditor.Controls.Add(emptyTextLiteral);
			}

			Context.ClientPage.ClientResponse.SetOuterHtml(ContentEditor.ID, ContentEditor);
		}

		/// <summary>
		/// Handles Sitecore messages. Think of this as a dispatch for commands made within the application
		/// </summary>
		/// <param name="message">The message.</param>
		public override void HandleMessage(Message message)
		{
			switch (message.Name.ToLower())
			{
				case "local:close":
					CloseApplication();
					break;
				case "local:save":
					SaveEditorFormData();
					break;
				case "local:add":
					AddItemToField();
					break;
				case "local:remove":
					RemoveItemFromField(true);
					break;
				case "local:delete":
					DeleteItem();
					break;
				case "local:addexistingitem":
					AddExistingItem();
					break;
			}

			base.HandleMessage(message);
		}

		/// <summary>
		/// This will delete the current item from sitecore by instantiating the delete pipeline
		/// </summary>
		private void DeleteItem()
		{
			Item selectedItem = dataTreeView.GetSelectionItem();
			if (selectedItem == null)
			{
				return;
			}

			//make sure we are on a velir image
			IFieldSuiteImage fieldSuiteImage = FieldSuiteImageFactory.GetFieldSuiteImage(selectedItem);
			if (fieldSuiteImage == null)
			{
				return;
			}

			//remove from field
			RemoveItemFromField(false);

			Item parentItem = selectedItem.Parent;

			Item[] items = new Item[1];
			items[0] = selectedItem;

			//perform action
			Items.Delete(items);

			//set parent as selected
			if (parentItem.IsNull())
			{
				return;
			}

			//load parent
			CurrentItem = parentItem;

			SetDisplay();
		}

		/// <summary>
		/// Adds the selected item in the tree picker to the primary item
		/// </summary>
		private void AddExistingItem()
		{
			Item selectedItem = CurrentItem;
			if (selectedItem.IsNull() || !selectedItem.IsOfTemplate(TemplateItem.ID.ToString()))
			{
				return;
			}

			string newValue = string.Empty;
			string[] itemIds = PrimaryItem[FieldName].Split('|');
			foreach (string itemId in itemIds)
			{
				if (string.IsNullOrEmpty(itemId) || itemId == selectedItem.ID.ToString())
				{
					continue;
				}

				if (!string.IsNullOrEmpty(newValue))
				{
					newValue += "|";
				}

				newValue += itemId;
			}

			if (!string.IsNullOrEmpty(newValue))
			{
				newValue += "|";
			}

			newValue += selectedItem.ID.ToString();

			using (new SecurityDisabler())
			{
				PrimaryItem.Editing.BeginEdit();
				PrimaryItem[FieldName] = newValue;
				PrimaryItem.Editing.EndEdit();
			}

			CurrentItem = selectedItem;
			LoadCurrentItem();
		}

		/// <summary>
		/// Removes the Current Item from the Primary Item that launched this modal
		/// </summary>
		private void RemoveItemFromField(bool loadCurrentItem)
		{
			if (PrimaryItem == null || string.IsNullOrEmpty(FieldName))
			{
				return;
			}

			Item selectedItem = dataTreeView.GetSelectionItem();
			if (selectedItem == null)
			{
				return;
			}

			string newValue = string.Empty;
			foreach (string itemId in PrimaryItem[FieldName].Split('|'))
			{
				if (string.IsNullOrEmpty(itemId) || itemId == selectedItem.ID.ToString())
				{
					continue;
				}

				if (!string.IsNullOrEmpty(newValue))
				{
					newValue += "|";
				}

				newValue += itemId;
			}

			using (new SecurityDisabler())
			{
				PrimaryItem.Editing.BeginEdit();
				PrimaryItem[FieldName] = newValue;
				PrimaryItem.Editing.EndEdit();
			}

			if (loadCurrentItem)
			{
				CurrentItem = selectedItem;
				LoadCurrentItem();
			}
		}

		
		/// <summary>
		/// Removes the Current Item from the Primary Item that launched this modal
		/// </summary>
		private void AddItemToField()
		{
			if (PrimaryItem == null || string.IsNullOrEmpty(FieldName))
			{
				return;
			}

			Item selectedItem = dataTreeView.GetSelectionItem();
			if (selectedItem == null)
			{
				return;
			}

			string selectedId = selectedItem.ID.ToString();
			string fieldValue = PrimaryItem[FieldName];
			if (fieldValue.Contains(selectedId))
			{
				return;
			}

			if (!string.IsNullOrEmpty(fieldValue))
			{
				fieldValue += "|";
			}

			fieldValue += selectedItem.ID.ToString();

			using (new SecurityDisabler())
			{
				PrimaryItem.Editing.BeginEdit();
				PrimaryItem[FieldName] = fieldValue;
				PrimaryItem.Editing.EndEdit();
			}

			CurrentItem = selectedItem;
			LoadCurrentItem();
		}

		/// <summary>
		/// Closes the modal
		/// </summary>
		private void CloseApplication()
		{
			SheerResponse.CloseWindow();
		}

		public static List<FieldSuiteEditor> DirtyEditors = null;
		public static List<SaveArgs> DirtySargs = null;
		private void ProcessDirtyBit(NameValueCollection args)
		{
			ClientPipelineArgs pipelineArgs = new ClientPipelineArgs(args);

			FieldSuiteEditor[] editors = new FieldSuiteEditor[CurrentEditors.Count];
			CurrentEditors.CopyTo(editors);
			DirtyEditors = editors.ToList();

			DirtySargs = new List<SaveArgs>();
			foreach (FieldSuiteEditor editor in CurrentEditors)
			{
				Hashtable fieldInfo = editor.FieldInfo;
				SaveArgs sargs = new SaveArgs(GetSavePacket(fieldInfo).XmlDocument) { SaveAnimation = false };
				DirtySargs.Add(sargs);
			}

			Sitecore.Pipelines.Pipeline pipe = Context.ClientPage.Start(this, "ShowSaveMessage", pipelineArgs);

			LoadCurrentItem();
		}

		/// <summary>
		/// Makes all available content editor forms save their information to the sitecore item fields.
		/// </summary>
		private void SaveEditorFormData()
		{
			SaveEditorFormData(true, "");
		}

		/// <summary>
		/// Makes all available content editor forms save their information to the sitecore item fields.
		/// </summary>
		private void SaveEditorFormData(bool alert, string postAction)
		{
			SaveEditorFormData(alert, CurrentEditors, postAction);
		}

		/// <summary>
		/// Makes all available content editor forms save their information to the sitecore item fields.
		/// </summary>
		private void SaveEditorFormData(bool alert, IEnumerable<FieldSuiteEditor> editors, string postAction)
		{
			foreach (FieldSuiteEditor editor in editors)
			{
				LockItem(editor.Item);

				Hashtable fieldInfo = editor.FieldInfo;
				SaveArgs args = new SaveArgs(GetSavePacket(fieldInfo).XmlDocument) { SaveAnimation = false };
				if (!string.IsNullOrEmpty(postAction))
					args.PostAction = postAction;
				Context.ClientPage.Start("saveUI", (ClientPipelineArgs)args);
				if (args.Error.Length > 0)
				{
					SheerResponse.Alert(args.Error, new string[0]);
					return;
				}

				editor.ClientPage.Modified = false;
			}

			if (alert) SheerResponse.Alert("Content Saved", new string[0]);
		}

		private static Packet GetSavePacket(Hashtable fieldInfo)
		{
			Assert.ArgumentNotNull(fieldInfo, "fieldInfo");
			Packet result = new Packet();
			foreach (FieldInfo info in fieldInfo.Values)
			{
				System.Web.UI.Control control = Context.ClientPage.FindSubControl(info.ID);
				if (control != null)
				{
					string str;
					if (control is IContentField)
					{
						str = StringUtil.GetString(new string[] { (control as IContentField).GetValue() });
					}
					else
					{
						str = StringUtil.GetString(ReflectionUtil.GetProperty(control, "Value"));
					}
					if (str != "__#!$No value$!#__")
					{
						result.StartElement("field");
						result.SetAttribute("itemid", info.ItemID.ToString());
						result.SetAttribute("language", info.Language.ToString());
						result.SetAttribute("version", info.Version.ToString());
						switch (info.Type.ToLowerInvariant())
						{
							case "rich text":
							case "html":
								str = str.TrimEnd(new char[] { ' ' });
								break;
						}

						result.SetAttribute("fieldid", info.FieldID.ToString());
						result.AddElement("value", str, new string[0]);
						result.EndElement();
					}
				}
			}
			return Assert.ResultNotNull<Packet>(result);
		}

		/// <summary>
		/// Gets all the assigned validators for the given Editor.
		/// </summary>
		/// <param name="editor"></param>
		/// <returns>ValidatorCollection</returns>
		private ValidatorCollection GetEditorValidators(FieldSuiteEditor editor)
		{
			ValidatorCollection validators = ValidatorManager.BuildValidators(ValidatorsMode.ValidatorBar, editor.Item);
			ValidatorOptions options = new ValidatorOptions(false);

			foreach (string marker in editor.FieldInfo.Keys)
			{
				FieldInfo fieldInfo = editor.FieldInfo[marker] as FieldInfo;
				if (fieldInfo == null) continue;
				Sitecore.Data.Validators.BaseValidator validator = validators.Where(x => x.FieldID == fieldInfo.FieldID).FirstOrDefault();
				if (validator == null) continue;
				validator.ControlToValidate = marker;
			}

			ValidatorManager.Validate(validators, options);
			ValidatorManager.UpdateValidators(validators);

			return validators;
		}

		/// <summary>
		/// Checks validation for the set of Editors applicable to the current state
		/// of the form.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private bool ValidateCurrentState(string message)
		{
			return ValidateState(CurrentEditors, message);
		}

		/// <summary>
		/// Given a list of Editors, checks validation for Errors.  If any, returns false, else true.
		/// </summary>
		/// <param name="editors"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		private bool ValidateState(List<FieldSuiteEditor> editors, string message)
		{
			foreach (FieldSuiteEditor editor in editors)
			{
				ValidatorCollection validators = GetEditorValidators(editor);

				foreach (BaseValidator baseValidator in validators)
				{
					if (baseValidator.Name.ToLower().Equals("required") && (baseValidator.Result == ValidatorResult.FatalError ||
						baseValidator.Result == ValidatorResult.CriticalError ||
						baseValidator.Result == ValidatorResult.Error))
					{
						Context.ClientPage.ClientResponse.Alert(message);
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// If the item isn't already locked, acquire the lock for the item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns>Returns true if item was successfully locked, else false.</returns>
		private bool LockItem(Item item)
		{
			if (item != null)
			{
				bool currentUserLock = item.Locking.HasLock();
				if (!currentUserLock)
				{
					if (item.Locking.IsLocked())
					{
						item.Locking.Unlock();
					}
					return item.Locking.Lock();
				}
			}
			return false;
		}

		/// <summary>
		/// If item is locked, try to unlock the item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns>Returns true if item was successfully unlocked, else false.</returns>
		private bool UnLockItem(Item item)
		{
			if (item != null)
			{
				bool currentUserLock = item.Locking.HasLock();
				if (currentUserLock)
				{
					return item.Locking.Unlock();
				}
			}
			return false;
		}

		/// <summary>
		/// Function used to render validators on the front-end.  Function signature must not be changed in order to work.
		/// Native Sitecore functionality looks for this function (called from javascript).
		/// </summary>
		protected void ValidateItem()
		{
			string str = string.Empty;
			foreach (FieldSuiteEditor editor in CurrentEditors)
			{
				ValidatorCollection validators = GetEditorValidators(editor);

				str += ValidatorBarFormatter.RenderValidationResult(validators);
			}
			SheerResponse.Eval(string.Concat(new object[] { "scContent.renderValidators(", StringUtil.EscapeJavascriptString(str), ",", Settings.Validators.UpdateFrequency, ")" }));
		}
	}
}