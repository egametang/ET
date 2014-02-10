using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using Infrastructure;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

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

		private void btnExcel_Click(object sender, RoutedEventArgs e)
		{
			var dict = new Dictionary<string, int>
			{
				{ "英雄令", 1 },
				{ "英雄令材料", 2 },
				{ "双倍", 3 },
				{ "会员", 4 },
				{ "聊天", 5 },
				{ "材料", 6 },
				{ "印记页", 7 },
				{ "血统页", 8 },
				{ "坐骑", 9 },
				{ "染色", 10 },
				{ "星之魂玉", 11 },
				{ "月之魂玉", 12 },
				{ "日之魂玉", 13 },
				{ "技能书", 14 },
				{ "混沌之玉", 15 },
				{ "战绩", 16 },
				{ "坐骑翅膀", 17 },
				{ "变身", 18 },
				{ "将星", 19 },
				{ "平台装备", 20 },
				{ "将星装备", 21 },
				{ "血魄", 22 },
				{ "装饰", 23 },
				{ "烟花", 24 },
				{ "英雄礼包", 25 },
				{ "套装", 26 },
			};

			HSSFWorkbook hssfWorkbook;
			const string path = @"F:\MallItemProto.xls";
			using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				hssfWorkbook = new HSSFWorkbook(file);
			}
			var sheet = hssfWorkbook.GetSheetAt(0);
			IEnumerator rows = sheet.GetRowEnumerator();

			const int nameIndex = 'Z' - 'A';
			rows.MoveNext();
			rows.MoveNext();
			rows.MoveNext();
			rows.MoveNext();
			while (rows.MoveNext())
			{
				var row = (HSSFRow)rows.Current;
				if (row.GetCell(nameIndex) == null)
				{
					continue;
				}
				var name = row.GetCell(nameIndex).ToString();

				if (!dict.ContainsKey(name))
				{
					continue;
				}

				ICell cell = row.GetCell(nameIndex - 1) ?? row.CreateCell(nameIndex - 1);
				cell.SetCellValue(dict[name].ToString());
			}

			using (var file = new FileStream(path, FileMode.Open, FileAccess.Write))
			{
				hssfWorkbook.Write(file);
			}
			lblShowResult.Content = "OK";
		}
	}
}