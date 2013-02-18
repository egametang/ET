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

		private async void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.Login();
		}

		private void btnSendCommand_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.SendCommand();
		}
	}
}