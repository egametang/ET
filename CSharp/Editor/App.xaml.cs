using System.Threading.Tasks.Schedulers;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using NLog;

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
			Logger = LogManager.GetLogger("Editor");
		}

		public static OrderedTaskScheduler OrderedTaskScheduler
		{
			get;
			private set;
		}

		public static Logger Logger
		{
			get;
			private set;
		}
	}
}
