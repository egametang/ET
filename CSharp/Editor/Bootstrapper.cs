using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Microsoft.Practices.Prism.MefExtensions;
using Infrastructure;
using Microsoft.Practices.Prism.Regions;
using TreeCanvas;

namespace Editor
{
	public partial class Bootstrapper : MefBootstrapper
	{
		protected override void ConfigureAggregateCatalog()
		{
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(Bootstrapper).Assembly));
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ViewExportAttribute).Assembly));
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(TreeCanvasModule).Assembly));
		}

		protected override void ConfigureContainer()
		{
			base.ConfigureContainer();
		}

		protected override void InitializeShell()
		{
			base.InitializeShell();
			Application.Current.MainWindow = (Shell)this.Shell;
			Application.Current.MainWindow.Show();
		}

		protected override IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
		{
			var factory = base.ConfigureDefaultRegionBehaviors();
			factory.AddIfMissing("AutoPopulateExportedViewsBehavior", typeof(AutoPopulateExportedViewsBehavior));
			return factory;
		}

		protected override DependencyObject CreateShell()
		{
			return this.Container.GetExportedValue<Shell>();
		}
	}
}
