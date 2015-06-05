using System;
using System.Windows;
using System.Windows.Controls;
using Model;

namespace Modules.BehaviorTreeModule
{
	/// <summary>
	/// NodeDataEditor.xaml 的交互逻辑
	/// </summary>
	public partial class NodeDataEditor
	{
		private readonly string[] nodeTypes;

		public NodeDataEditor()
		{
			this.InitializeComponent();

			this.nodeTypes = Enum.GetNames(typeof (NodeType));
			Array.Sort(this.nodeTypes);
			this.cbType.ItemsSource = this.nodeTypes;
		}

		public AllTreeView AllTreeView { get; set; }

		public TreeNodeViewModel TreeNodeViewModel
		{
			get
			{
				return this.DataContext as TreeNodeViewModel;
			}
			set
			{
				this.DataContext = value;
			}
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.TreeNodeViewModel == null)
			{
				return;
			}
			string typeStr = ((NodeType) this.TreeNodeViewModel.Type).ToString();
			int selectIndex = Array.IndexOf(this.nodeTypes, typeStr);
			this.cbType.SelectedIndex = selectIndex;
		}

		private void CbType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.TreeNodeViewModel == null)
			{
				return;
			}
			if (this.cbType.SelectedValue == null)
			{
				this.TreeNodeViewModel.Type = 0;
				return;
			}
			this.TreeNodeViewModel.Type =
					(int) Enum.Parse(typeof (NodeType), this.cbType.SelectedValue.ToString().Trim());
		}
	}
}