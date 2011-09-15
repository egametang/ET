using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections;

namespace Egametang
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
	}
}
