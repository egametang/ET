using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Infrastructure;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.Prism.Regions;
using Modules.BehaviorTree;
using Modules.Robot;
using Modules.WaiGua;

namespace Editor
{
	public class Bootstrapper : MefBootstrapper
	{
		protected override void ConfigureAggregateCatalog()
		{
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof (Bootstrapper).Assembly));
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof (ViewExportAttribute).Assembly));
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof (BehaviorTreeModule).Assembly));
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof (RobotModule).Assembly));
			this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof (WaiGuaModule).Assembly));
		}

		protected override void InitializeShell()
		{
			base.InitializeShell();
			Application.Current.MainWindow = (Shell) this.Shell;
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
			return this.Container.GetExportedValue<Shell>();
		}
	}
}