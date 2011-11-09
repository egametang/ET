using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using Infrastructure;

namespace Module.Login
{
	/// <summary>
	/// LoginView.xaml 的交互逻辑
	/// </summary>
	[ViewExport(RegionName = "MainRegion")]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class LoginView : UserControl
	{
		public LoginView()
		{
			InitializeComponent();
		}

		[Import]
		LoginViewModel ViewModel
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

		private void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Password = pbPassword.Password;
			this.ViewModel.Login();
		}

		private void tbLogInfo_TextChanged(object sender, TextChangedEventArgs e)
		{
			tbLogInfo.ScrollToEnd();
		}
	}
}
