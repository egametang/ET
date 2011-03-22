using System.Windows;
using GalaSoft.MvvmLight.Threading;
using System.ComponentModel;

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
		}
	}
}
