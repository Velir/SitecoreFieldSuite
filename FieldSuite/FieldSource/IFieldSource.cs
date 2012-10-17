using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sitecore.Data.Items;

namespace FieldSuite.FieldSource
{
	public interface IFieldSource
	{
		/// <summary>
		/// Returns a list of items for a field
		/// </summary>
		/// <returns></returns>
		List<Item> GetItems();
	}
}