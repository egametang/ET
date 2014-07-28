using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Helper;

namespace Tree
{
	/// <summary>
	/// NodeDataEditor.xaml 的交互逻辑
	/// </summary>
	public partial class NodeDataEditor : UserControl
	{
		public NodeDataEditor()
		{
			InitializeComponent();

			string[] nodeTypes = Enum.GetNames(typeof(NodeType));
			this.cbType.ItemsSource = nodeTypes;
		}

		public TreeNodeViewModel ViewModel
		{
			get
			{
				return this.DataContext as TreeNodeViewModel;
			}
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.ViewModel == null)
			{
				return;
			}
			this.cbType.SelectedIndex = EnumHelper.EnumIndex<NodeType>(this.ViewModel.Type);
		}

		private void CbType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.cbType.SelectedValue == null)
			{
				this.ViewModel.Type = 0;
				return;
			}
			this.ViewModel.Type = (int)Enum.Parse(typeof(NodeType), this.cbType.SelectedValue.ToString().Trim());
		}
	}
}
