using System.ComponentModel.Composition;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using Infrastructure;

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
			string nodePath = ConfigurationManager.AppSettings["NodePath"];
			this.ViewModel.Open(nodePath);
			this.lbTrees.SelectedIndex = -1;
			this.treeView.ViewModel = null;
		}

		private void MenuItem_Save(object sender, RoutedEventArgs e)
		{
			string nodePath = ConfigurationManager.AppSettings["NodePath"];
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
				if (this.treeView.ViewModel.TreeId == treeViewModel.TreeId)
				{
					return;
				}
			}
			this.lbTrees.SelectedItem = treeViewModel;
			this.treeView.ViewModel = treeViewModel;
		}
	}
}