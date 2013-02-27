using System.ComponentModel.Composition;
using System.Windows;
using Infrastructure;

namespace Modules.Robot
{
	/// <summary>
	/// LoginView.xaml 的交互逻辑
	/// </summary>
	// [ViewExport(RegionName = "RobotRegion"), PartCreationPolicy(CreationPolicy.Shared)]
	public partial class LoginView
	{
		public LoginView()
		{
			InitializeComponent();
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

		private void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Login();
		}
	}
}
