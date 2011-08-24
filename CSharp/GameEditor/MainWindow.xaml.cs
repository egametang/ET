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
using System.Collections;

namespace GameEditor
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		private Hashtable treeViewNodes = new Hashtable();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void behaviorTreeView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			var item = e.Source as TreeViewItem;
			if (item == null)
			{
				return;
			}
			item.ContextMenu.IsOpen = true;
			e.Handled = true;
		}

		private void NewCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void OnNewNode(object sender, ExecutedRoutedEventArgs e)
		{
		}

		private void DeleteCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void OnDeleteNode(object sender, ExecutedRoutedEventArgs e)
		{

		}
	}
}
