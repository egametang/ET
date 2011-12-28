using System.ComponentModel.Composition.Hosting;
using System.Windows;
using BehaviorTree;
using Infrastructure;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Regions;

namespace Editor
{
	public class Bootstrapper : MefBootstrapper
	{
		protected override void ConfigureAggregateCatalog()
		{
			AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof (Bootstrapper).Assembly));
			AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof (ViewExportAttribute).Assembly));
			AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof (BehaviorTreeModule).Assembly));
		}

		protected override void InitializeShell()
		{
			base.InitializeShell();
			Application.Current.MainWindow = (Shell) Shell;
			Application.Current.MainWindow.Show();
		}

		protected override IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
		{
			IRegionBehaviorFactory factory = base.ConfigureDefaultRegionBehaviors();
			factory.AddIfMissing("AutoPopulateExportedViewsBehavior", typeof (AutoPopulateExportedViewsBehavior));
			return factory;
		}

		protected override DependencyObject CreateShell()
		{
			return Container.GetExportedValue<Shell>();
		}
	}
}