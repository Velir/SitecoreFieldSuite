using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Fields;
using Sitecore.Links;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.Configuration;
using Sitecore.Text;
using Sitecore.Diagnostics;
using Sitecore.Web.UI.HtmlControls.Data;

namespace FieldSuite.Fields
{
	public class IDField : CustomField
	{
		#region Properties

		/// <summary>
		/// uses the Database property and stored value to retrieve an ID object
		/// </summary>
		public ID TargetID {
			get {
				if (this.Database != null) {
					Item item = this.Database.Items[base.Value];
					if (item != null) {
						return item.ID;
					}
				}
				return ID.Null;
			}
		}

		/// <summary>
		/// uses the Database and TargetID properties to retrieve an Item object 
		/// </summary>
		public Item TargetItem {
			get {
				ID targetID = this.TargetID;
				if (targetID.IsNull)
					return null;
				
				Database database = Database;
				if (database == null)
					return null;
				
				return database.Items[targetID];
			}
		}

		private Database _database;
		/// <summary>
		/// will use the databasename from the source field if the datasource exists in the source field, otherwise will default to the fields' inner database
		/// </summary>
		public Database Database {
			get {
				if (_database == null) {
					_database = base.InnerField.Database;
					
					string source = base.InnerField.Source;
					if (!string.IsNullOrEmpty(source) && LookupSources.IsComplex(source)) {
						Database database = LookupSources.GetDatabase(source);
						if (database != null)
							_database = database;
					}
				}
				return _database;
			}
		}
		
		#endregion Properties

		#region Constructors

		public IDField(Field innerField) 
			: base(innerField) {
		}

		public static implicit operator IDField(Field field) {
			if (field == null)
				return null;
			return new IDField(field);
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Clears stored value
		/// </summary>
		public void Clear() {
			base.Value = string.Empty;
		}

		#endregion Methods

		#region Override Methods

		/// <summary>
		/// will update the stored value with the newLink ID property if the itemLink's TargetItemID matches the current TargetID
		/// </summary>
		public override void Relink(ItemLink itemLink, Item newLink) {
			Assert.ArgumentNotNull(itemLink, "itemLink");
			Assert.ArgumentNotNull(newLink, "newLink");
			if (this.TargetID == itemLink.TargetItemID)
				base.Value = newLink.ID.ToString();
		}

		/// <summary>
		/// clears the stored value
		/// </summary>
		public override void RemoveLink(ItemLink itemLink) {
			Assert.ArgumentNotNull(itemLink, "itemLink");
			this.Clear();
		}

		/// <summary>
		/// updates the current base value with the itemLink's TargetItemID string value 
		/// </summary>
		public override void UpdateLink(ItemLink itemLink) {
			Assert.ArgumentNotNull(itemLink, "itemLink");
			Item targetItem = itemLink.GetTargetItem();
			if (targetItem != null) 
				base.Value = targetItem.ID.ToString();
		}

		/// <summary>
		/// runs the LinkDatabase check to see if the fields are valid or not
		/// </summary>
		public override void ValidateLinks(LinksValidationResult result) {
			Assert.ArgumentNotNull(result, "result");
			if (this.TargetID.IsNull)
				return;
			Item targetItem = this.TargetItem;
			if (targetItem != null)
				result.AddValidLink(targetItem, base.Value);
			else
				result.AddBrokenLink(base.Value);
		}

		#endregion Override Methods
	}
}
