using System.Windows;

namespace Editor
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			RunInDebugMode();
			this.ShutdownMode = ShutdownMode.OnMainWindowClose;
		}

		private static void RunInDebugMode()
		{
			Bootstrapper bootstrapper = new Bootstrapper();
			bootstrapper.Run();
		}
	}
}
