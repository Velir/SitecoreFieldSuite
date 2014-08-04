using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Validators;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Save;
using Sitecore.Reflection;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.ContentManager;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Xml;
using Sitecore.SharedSource.Commons.Extensions;
using Sitecore.SharedSource.FieldSuite.Editors;

namespace Sitecore.SharedSource.FieldSuite.Xaml
{
	public class FieldSuiteEditForm : BaseForm
	{
		private readonly Database _database = Database.GetDatabase("master");

		protected Literal HelpLiteral;
		protected Panel ContentPanel;
		protected Scrollbox ContentViewScrollbox;
		private static List<FieldSuiteEditor> CurrentEditors;
		protected Border ContentEditor;
		private Item _currentItem;
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
				if (WebUtil.GetQueryString("id") != null)
				{
					_currentItem = _database.GetItem(WebUtil.GetQueryString("id"));
				}

				return _currentItem;
			}
			set { _currentItem = value; }
		}

		protected override void OnLoad(EventArgs args)
		{
			base.OnLoad(args);
			if (Context.ClientPage.IsEvent)
			{
				return;
			}

			if (CurrentEditors == null)
			{
				CurrentEditors = new List<FieldSuiteEditor>();
			}

			if (CurrentItem.IsNull())
			{
				return;
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
				saveButton.Visible = false;
				ResetDisplay(true);
				return;
			}

			ResetDisplay(false);
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

			ValidateItem();

			//reset form
			SetDisplay();
			CreateContentEditor(CurrentItem);
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
				RenderHeader = true,
				RenderTabsAndBars = false,
				RenderWarnings = true
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
				//case "item:checkout":
				//    LockAndLoadItem();
				//    return;
			}

			base.HandleMessage(message);
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
		/// <returns>Returns true if item was successfully locked, else false.</returns>
		private void LockAndLoadItem()
		{
			LockItem(CurrentItem);

			// reset item, get the item from querystring
			if (WebUtil.GetQueryString("id") != null)
			{
				_currentItem = _database.GetItem(WebUtil.GetQueryString("id"));
			}

			ResetDisplay(true);
			LoadCurrentItem();
			OnLoad(new EventArgs());
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