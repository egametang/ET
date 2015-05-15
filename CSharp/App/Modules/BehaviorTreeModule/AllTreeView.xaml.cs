using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Infrastructure;
using Application = System.Windows.Application;

namespace Modules.BehaviorTreeModule
{
	/// <summary>
	/// BehaviorTreeView.xaml 的交互逻辑
	/// </summary>
	[ViewExport(RegionName = "BehaviorTreeRegion"), PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class AllTreeView
	{
		public AllTreeView()
		{
			this.InitializeComponent();

			this.nodeDataEditor.AllTreeView = this;
			this.treeView.AllTreeView = this;
		    this.ViewModel = AllTreeViewModel.Instance;
		}

		private AllTreeViewModel ViewModel
		{
			get
			{
				return this.DataContext as AllTreeViewModel;
			}
			set
			{
				this.DataContext = value;
			}
		}

		private void MenuItem_Open(object sender, RoutedEventArgs e)
		{
			string nodePath = Path.Combine(ConfigurationManager.AppSettings["NodePath"], "TreeProto.txt");
			this.ViewModel.Open(nodePath);
			this.lbTrees.SelectedIndex = -1;
			this.treeView.ViewModel = null;
		}

		private void MenuItem_Save(object sender, RoutedEventArgs e)
		{
			string nodePath = Path.Combine(ConfigurationManager.AppSettings["NodePath"], "TreeProto.txt");
			this.ViewModel.Save(nodePath);
		}

		private void MenuItem_New(object sender, RoutedEventArgs e)
		{
			TreeViewModel treeViewModel = this.ViewModel.New();
			this.lbTrees.SelectedItem = treeViewModel;
			this.treeView.ViewModel = treeViewModel;
		}

		private void MenuItem_Clone(object sender, RoutedEventArgs e)
		{
			if (this.lbTrees.SelectedItem == null)
			{
				return;
			}
			TreeViewModel treeViewModel = this.lbTrees.SelectedItem as TreeViewModel;
			TreeViewModel newTreeViewModel = this.ViewModel.Clone(treeViewModel);
			this.treeView.ViewModel = newTreeViewModel;
		}

		private void MenuItem_Path(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			dialog.SelectedPath = ConfigurationManager.AppSettings["NodePath"];
			dialog.ShowDialog();
			string path = dialog.SelectedPath;
			if (path.Length < 1 || !Directory.Exists(path))
			{
				System.Windows.MessageBox.Show("选择的目录无效");
				return;
			}
			Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			cfg.AppSettings.Settings["NodePath"].Value = path;
			cfg.Save();
			ConfigurationManager.AppSettings["NodePath"] = path;
			Application.Current.MainWindow.Title = string.Format("行为树编辑器 当前工作目录[{0}]", ConfigurationManager.AppSettings["NodePath"]);
		}

		private void MenuItem_Remove(object sender, RoutedEventArgs e)
		{
			if (this.lbTrees.SelectedItem == null)
			{
				return;
			}
			TreeViewModel treeViewModel = this.lbTrees.SelectedItem as TreeViewModel;
			this.ViewModel.Remove(treeViewModel);
			this.lbTrees.SelectedItem = null;
			e.Handled = true;
		}

		private void ListBoxItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			FrameworkElement item = (FrameworkElement) sender;
			TreeViewModel treeViewModel = item.DataContext as TreeViewModel;
			if (this.treeView.ViewModel != null)
			{
				if (this.treeView.ViewModel.Id == treeViewModel.Id)
				{
					return;
				}
			}
			this.lbTrees.SelectedItem = treeViewModel;
			this.treeView.ViewModel = treeViewModel;
		}
	}
}