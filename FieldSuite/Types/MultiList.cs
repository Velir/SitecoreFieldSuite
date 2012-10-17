using System.Text;
using System.Web.UI;
using Sitecore;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Data.Items;

namespace FieldSuite.Types
{
	public class MultiList : AFieldSuiteField
	{
		/// <summary>
		/// Current Field Type
		/// </summary>
		public override Item FieldTypeItem
		{
			get
			{
				string id = "{75895043-557E-4E3B-A52F-033791E7B347}";
				if (!string.IsNullOrEmpty(FieldTypeItemId))
				{
					id = FieldTypeItemId;
				}

				Database database = Sitecore.Data.Database.GetDatabase("core");
				if (database == null)
				{
					return null;
				}

				return database.GetItem(id);
			}
		}

		/// <summary>
		/// Render the Field
		/// </summary>
		/// <param name="output"></param>
		protected override void Render(HtmlTextWriter output)
		{
			Assert.ArgumentNotNull(output, "output");
			base.ServerProperties["ID"] = this.ID;
			string str = string.Empty;
			if (this.ReadOnly)
			{
				str = " disabled=\"disabled\"";
			}
			output.Write("<input id=\"" + this.ID + "_Value\" type=\"hidden\" value=\"" + StringUtil.EscapeQuote(this.Value) + "\" />");

			//build the menu items
			//output.Write(BuildMenuItems());

			output.Write("<table" + this.GetControlAttributes() + ">");
			output.Write("<tr>");
			output.Write("<td class=\"scContentControlMultilistCaption\" width=\"50%\"><span class=\"allSpan\">" + Translate.Text("All") + "</span></td>");
			output.Write("<td class=\"scContentControlMultilistCaption\" width=\"50%\">" + Translate.Text("Selected") + "</td>");

			output.Write("<td width=\"20\">" + Images.GetSpacer(20, 1) + "</td>");

			output.Write("</tr>");
			output.Write("<tr>");

			//build available list
			output.Write("<td valign=\"top\" height=\"100%\">");
			output.Write(BuildAvailableItems());
			output.Write("</td>");

			//build selected list
			output.Write("<td valign=\"top\" height=\"100%\">");
			output.Write(BuildSelectedItems());
			output.Write("</td>");

			//build sort icons
			output.Write("<td valign=\"top\">");
			this.RenderButton(output, "Core/16x16/arrow_blue_up.png", "FieldSuite.Fields.MoveItemUp('" + this.ID + "')");
			output.Write("<br />");
			this.RenderButton(output, "Core/16x16/arrow_blue_down.png", "FieldSuite.Fields.MoveItemDown('" + this.ID + "')");
			output.Write("</td>");

			output.Write("</tr>");
			output.Write("</table>");
		}

		/// <summary>
		/// Rendering the Sort Icons
		/// </summary>
		/// <param name="output"></param>
		/// <param name="icon"></param>
		/// <param name="click"></param>
		private void RenderButton(HtmlTextWriter output, string icon, string click)
		{
			Assert.ArgumentNotNull(output, "output");
			Assert.ArgumentNotNull(icon, "icon");
			Assert.ArgumentNotNull(click, "click");
			ImageBuilder builder = new ImageBuilder
			{
				Src = icon,
				Width = 0x10,
				Height = 0x10,
				Margin = "2px"
			};
			if (!this.ReadOnly)
			{
				builder.OnClick = click;
			}
			output.Write(builder.ToString());
		}
	}
}