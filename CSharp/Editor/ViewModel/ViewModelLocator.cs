
namespace Egametang
{
	public class ViewModelLocator
	{
		static private MainViewModel mainViewModel = new MainViewModel();

		public static MainViewModel MainViewModel
		{
			get
			{
				return mainViewModel;
			}
		}

		public static void Cleanup()
		{
		}
	}
}