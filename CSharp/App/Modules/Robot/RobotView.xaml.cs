using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
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

		private void btnStart_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Start();
		}

		private void tbLog_TextChanged(object sender, TextChangedEventArgs e)
		{
			this.tbLog.ScrollToEnd();
		}
	}
}