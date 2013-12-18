using System;
using System.ComponentModel.Composition;
using System.Windows;
using Infrastructure;

namespace Robot
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

		private async void btnFindPlayer_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.GetCharacterInfo();
			this.tcCharacterInfo.IsEnabled = true;
			this.btnForbidCharacter.IsEnabled = true;
			this.btnAllowCharacter.IsEnabled = true;
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}

		private void menuLogin_Click(object sender, RoutedEventArgs e)
		{
			this.ViewModel.ReLogin();
		}

		private async void menuServers_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.Servers();
			this.tcAll.SelectedIndex = 0;
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}

		private async void btnForbidCharacter_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.ForbidCharacter(cbForbiddenType.SelectedValue.ToString(), tbForbiddenTime.Text);
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}

		private async void btnAllowCharacter_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.ForbidCharacter(cbForbiddenType.SelectedValue.ToString(), "-1");
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}

		private async void btnSendCommand_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.SendCommand();
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}

		private async void btnForbiddenLogin_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.ForbiddenLogin(
				cbForbiddenLogin.SelectedValue.ToString(), 
				tbForbiddenLoginContent.Text, tbForbiddenLoginTime.Text);
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}

		private async void btnAllowLogin_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.ForbiddenLogin(
				cbForbiddenLogin.SelectedValue.ToString(), tbForbiddenLoginContent.Text, "-1");
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}

		private async void btnSendMail_Click(object sender, RoutedEventArgs e)
		{
			await this.ViewModel.SendMail();
			this.tbLog.AppendText(Environment.NewLine + this.ViewModel.ErrorInfo);
			this.tbLog.ScrollToEnd();
		}
	}
}