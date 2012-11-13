
namespace FieldSuite.ImageMapping
{
	public abstract class AFieldSuiteImage : IFieldSuiteImage
	{
		protected AFieldSuiteImage(FieldSuiteImageArgs args)
		{
		}

		/// <summary>
		/// Title Field
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Image Url
		/// </summary>
		public string ImageUrl { get; set; }
	}
}
