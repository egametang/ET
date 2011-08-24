using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
