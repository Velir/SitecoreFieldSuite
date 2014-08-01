using System;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using FieldSuite.ImageMapping;

namespace FieldSuite.Xaml
{
	public class FieldSuiteAddForm : BaseForm
	{
		protected readonly Database _database = Database.GetDatabase("master");
		protected Panel ContentPanel;
		protected Scrollbox ContentViewScrollbox;
		protected Border ContentEditor;
		protected Item _sourceItem;
		protected GridPanel examGridPanel;
		protected DataContext addItemDataContext;
		protected TreePicker addItemTreePicker;
		protected Toolbutton saveButton;

		/// <summary>
		/// Parent Item
		/// </summary>
		public virtual Item SourceItem
		{
			get
			{
				// case where we have already retrieved the exam item
				if (_sourceItem != null) return _sourceItem;

				string sourceItemPath = WebUtil.GetQueryString("source");
				sourceItemPath = sourceItemPath.Replace("query:", string.Empty);

				if (string.IsNullOrEmpty(sourceItemPath))
				{
					return null;
				}

				// get the item
				_sourceItem = _database.GetItem(sourceItemPath);
				return _sourceItem;
			}
			set { _sourceItem = value; }
		}

		protected override void OnLoad(EventArgs args)
		{
			base.OnLoad(args);
			if (Context.ClientPage.IsPostBack || Context.ClientPage.IsEvent)
			{
				return;
			}

			addItemTreePicker.OnChanged += new EventHandler(dataTreeView_OnClick);
			LoadDataContext();
		}

		/// <summary>
		/// Load Treepicker datacontext
		/// </summary>
		private void LoadDataContext()
		{
			if (SourceItem == null || Context.ClientPage.IsPostBack)
			{
				return;
			}

			DataUri[] dataUris = new DataUri[1];
			dataUris[0] = new DataUri(SourceItem.ID);

			addItemDataContext.Root = SourceItem.Paths.FullPath;
			addItemDataContext.DefaultItem = SourceItem.ID.ToString();
			addItemDataContext.Folder = SourceItem.Paths.FullPath;
			addItemDataContext.Selected = dataUris;
			addItemDataContext.AddSelected(new DataUri(SourceItem.ID));
			addItemDataContext.Refresh();
		}
		
		/// <summary>
		/// Verify Velir Image
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void dataTreeView_OnClick(object sender, EventArgs e)
		{
			Item item = addItemDataContext.GetFolder();
			if (item == null)
			{
				return;
			}

			//get interface item
			IFieldSuiteImage fieldSuiteImage = FieldSuiteImageFactory.GetFieldSuiteImage(item);
			if (fieldSuiteImage == null)
			{
				SheerResponse.Alert(string.Format("{0} does not implement the velir image interface.", item.Name));
				return;
			}
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
					SaveForm();
					break;
			}

			base.HandleMessage(message);
		}

		/// <summary>
		/// Saves the Form
		/// </summary>
		private void SaveForm()
		{
			Item item = addItemDataContext.GetFolder();
			if (item == null)
			{
				CloseApplication();
				return;
			}

			//get interface item
			IFieldSuiteImage fieldSuiteImage = FieldSuiteImageFactory.GetFieldSuiteImage(item);
			if (fieldSuiteImage == null)
			{
				SheerResponse.Alert(string.Format("{0} does not implement the velir image interface.", item.Name));
				return;
			}

			Context.ClientPage.ClientResponse.SetDialogValue(item.ID.ToString());
			CloseApplication();
		}

		/// <summary>
		/// Closes the modal
		/// </summary>
		private void CloseApplication()
		{
			SheerResponse.CloseWindow();
		}
	}
}