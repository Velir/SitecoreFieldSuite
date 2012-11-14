using System.Collections.Generic;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.FieldSuite.FieldSource
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