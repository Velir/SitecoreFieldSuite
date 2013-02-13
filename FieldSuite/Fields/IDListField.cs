using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sitecore.Data.Fields;
using Sitecore.Data;
using Sitecore.Data.Items;
using System.Collections;
using Sitecore.Web.UI.HtmlControls.Data;
using Sitecore.Links;
using Sitecore.Text;
using Sitecore.Diagnostics;

namespace FieldSuite.Fields
{
	public class IDListField : CustomField {
		
		#region Properties

		/// <summary>
		/// the character delimiter for the item id's
		/// </summary>
		private char _delimiter = '|';

		/// <summary>
		/// the number of item id's in the List property
		/// </summary>
		public int Count {
			get {
				return this.List.Count;
			}
		}

		/// <summary>
		/// gets the id string from the List property at the specified index
		/// </summary>
		public string this[int index] {
			get {
				Error.AssertInt(index, "index", true, false);
				return this.List[index];
			}
		}

		/// <summary>
		/// returns a string array of the item id's in the List property
		/// </summary>
		public string[] Items {
			get {
				return this.List.Items;
			}
		}

		/// <summary>
		/// accessor for the item id's in the stored value
		/// </summary>
		public ListString List {
			get {
				return new ListString(base.Value, this._delimiter);
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

		/// <summary>
		/// gets an ID array of the id's in the stored value
		/// </summary>
		public ID[] TargetIDs {
			get {
				ArrayList list = new ArrayList();
				foreach (string str2 in base.Value.Split(new char[] { _delimiter })) {
					if ((str2.Length > 0) && ID.IsID(str2)) {
						list.Add(ID.Parse(str2));
					}
				}
				return (list.ToArray(typeof(ID)) as ID[]);
			}
		}

		#endregion Properties

		#region Constructors

		public IDListField(Field innerField)
			: base(innerField) {
		}

		public static implicit operator IDListField(Field field) {
			if (field != null) {
				return new IDListField(field);
			}
			return null;
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// adds the itemID to the stored value
		/// </summary>
		/// <param name="item"></param>
		public string Add(string itemID)
		{
			Error.AssertString(itemID, "itemID", true);
			return this.Add(itemID, false);
		}

		/// <summary>
		/// adds the itemID to the stored value and if includeBlank is true will add the value itemID even if it's empty
		/// </summary>
		public string Add(string itemID, bool includeBlank)
		{
			Error.AssertString(itemID, "itemID", true);
			string str = this.List.Add(itemID, includeBlank);
			base.Value = str;
			return str;
		}

		/// <summary>
		/// returns the CharIndex of the itemID in the stored value
		/// </summary>
		public int CharIndexOf(string itemID)
		{
			Error.AssertString(itemID, "itemID", false);
			return this.List.CharIndexOf(itemID);
		}

		/// <summary>
		/// determines if the stored value contains the itemID
		/// </summary>
		public bool Contains(string itemID)
		{
			Error.AssertString(itemID, "itemID", false);
			return this.List.Contains(itemID);
		}

		/// <summary>
		/// returns the enumerator of the List property
		/// </summary>
		public IEnumerator GetEnumerator()
		{
			return this.List.GetEnumerator();
		}

		/// <summary>
		/// returns the index location of the itemID in the stored value
		/// </summary>
		public int IndexOf(string itemID)
		{
			Error.AssertString(itemID, "itemID", false);
			return this.List.IndexOf(itemID);
		}

		/// <summary>
		/// removes the itemID from the stored value
		/// </summary>
		public string Remove(string itemID)
		{
			Error.AssertString(itemID, "itemID", true);
			string str = this.List.Remove(itemID);
			base.Value = str;
			return str;
		}

		/// <summary>
		/// Replaces the oldItemId in the base value with the newItemID
		/// </summary>
		public string Replace(string oldItemID, string newItemID)
		{
			Error.AssertString(oldItemID, "item", true);
			string str = this.List.ToString().Replace(oldItemID, newItemID);
			base.Value = str;
			return str;
		}

		/// <summary>
		/// returns an item array using the id's in the stored value
		/// </summary>
		public Item[] GetItems() {
			ArrayList list = new ArrayList();
			Database database = Database;
			if (database == null)
				return null;
			foreach (ID id in this.TargetIDs) {
				Item item = database.GetItem(id);
				if (item != null)
					list.Add(item);
			}
			return (list.ToArray(typeof(Item)) as Item[]);
		}

		#endregion Methods

		#region Override Methods

		/// <summary>
		/// removes the itemLink's TargetItemID from the stored value
		/// </summary>
		public override void RemoveLink(ItemLink itemLink) {
			Assert.ArgumentNotNull(itemLink, "itemLink");
			string targetID = itemLink.TargetItemID.ToString();
			if (this.Contains(targetID))
				this.Remove(targetID);
		}

		/// <summary>
		/// replaces the the itemLink's TargetItemID from the stored value with the newLink ID property if the itemLink's TargetItemID is found
		/// </summary>
		public override void Relink(ItemLink itemLink, Item newLink) {
			Assert.ArgumentNotNull(itemLink, "itemLink");
			Assert.ArgumentNotNull(newLink, "newLink");
			string item = itemLink.TargetItemID.ToString();
			if (this.Contains(item))
				this.Replace(item, newLink.ID.ToString());
		}

		/// <summary>
		/// builds the string of IDs from the List property
		/// </summary>
		public override string ToString() {
			return this.List.ToString();
		}

		/// <summary>
		/// runs the LinkDatabase check to see if the fields are valid or not
		/// </summary>
		public override void ValidateLinks(LinksValidationResult result)
		{
			Database database = Database;
			if (database == null)
				return;
			
			foreach (string str in this.Items)
			{
				if (!ID.IsID(str))
					continue;
				
				ID id = ID.Parse(str);
				if (ItemUtil.IsNull(id) || id.IsNull)
					continue;
				
				Item targetItem = database.GetItem(id);
				if (targetItem != null)
					result.AddValidLink(targetItem, base.Value);
				else
					result.AddBrokenLink(base.Value);
			}
		}

		#endregion Override Methods
	}
}