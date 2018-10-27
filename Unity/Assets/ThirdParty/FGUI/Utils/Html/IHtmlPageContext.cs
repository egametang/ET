
namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public interface IHtmlPageContext
	{
		IHtmlObject CreateObject(RichTextField owner, HtmlElement element);
		void FreeObject(IHtmlObject obj);

		NTexture GetImageTexture(HtmlImage image);
		void FreeImageTexture(HtmlImage image, NTexture texture);
	}
}
