using System.Windows;

namespace Editor
{
    public partial class App: Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            RunInDebugMode();
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private static void RunInDebugMode()
        {
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}