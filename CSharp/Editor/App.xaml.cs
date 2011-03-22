using System.Threading.Tasks.Schedulers;
using System.Windows;
using GalaSoft.MvvmLight.Threading;

namespace Egametang
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		static App()
		{
			DispatcherHelper.Initialize();
			OrderedTaskScheduler = new OrderedTaskScheduler();
		}

		public static OrderedTaskScheduler OrderedTaskScheduler
		{
			get;
			private set;
		}
	}
}
