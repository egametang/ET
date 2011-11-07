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
			set
			{
				this.DataContext = value;
			}
		}
	}
}
