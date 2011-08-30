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

		private void behaviorTreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
		{
			newMenuItem.IsEnabled = true;
			copyMenuItem.IsEnabled = true;
			pasteMenuItem.IsEnabled = true;
			delMenuItem.IsEnabled = true;
		}

		private void newMenuItem_Click(object sender, RoutedEventArgs e)
		{

		}

		private void delMenuItem_Click(object sender, RoutedEventArgs e)
		{

		}

		private void copyMenuItem_Click(object sender, RoutedEventArgs e)
		{

		}

		private void pasteMenuItem_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
