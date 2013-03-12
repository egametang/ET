using System;
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

		private void btnFindPlayer_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.FindPlayer();
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}

		private void menuLogin_Click(object sender, RoutedEventArgs e)
		{
		}

		private async void menuServers_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.Servers();
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}

		private async void btnForbiddenBuy_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.ForbiddenBuy();
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}

		private async void btnAllowBuy_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.AllowBuy();
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}
	}
}