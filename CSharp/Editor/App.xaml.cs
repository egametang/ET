using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System.ComponentModel;

namespace Editor
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		static App()
		{
			DispatcherHelper.Initialize();
		}
	}
}
