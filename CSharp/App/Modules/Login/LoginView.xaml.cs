using System.ComponentModel.Composition;
using System.Windows;
using Infrastructure;

namespace Modules.Login
{
	/// <summary>
	/// LoginView.xaml 的交互逻辑
	/// </summary>
	[ViewExport(RegionName = "LoginRegion"), PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class LoginView
	{
		public LoginView()
		{
			InitializeComponent();
		}

		[Import]
		private LoginViewModel ViewModel
		{
			get
			{
				return this.DataContext as LoginViewModel;
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
	}
}
