using System.ComponentModel.Composition;
using System.Windows;
using Infrastructure;

namespace Modules.Robot
{
	/// <summary>
	/// RobotView.xaml 的交互逻辑
	/// </summary>
	[ViewExport(RegionName = "RobotRegion"), PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class RobotView
	{
		public RobotView()
		{
			this.InitializeComponent();
		}

		[Import]
		private RobotViewModel ViewModel
		{
			get
			{
				return this.DataContext as RobotViewModel;
			}
			set
			{
				this.DataContext = value;
			}
		}

		private void menuReload_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Reload();
		}

		private void btnFind_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Find();
		}

		private void menuLogin_Click(object sender, RoutedEventArgs e)
		{
		}

		private void menuServers_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Servers();
		}

		private void btnForbiddenBuy_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.ForbiddenBuy();
		}

		private void btnAllowBuy_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.AllowBuy();
		}
	}
}