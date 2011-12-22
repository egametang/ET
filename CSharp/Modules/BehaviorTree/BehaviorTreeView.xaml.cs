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
using System.ComponentModel.Composition;
using Infrastructure;

namespace BehaviorTree
{
	/// <summary>
	/// TreeCanvasView.xaml 的交互逻辑
	/// </summary>
	[ViewExport(RegionName = "TreeCanvasRegion")]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class BehaviorTreeView : UserControl
	{
		public BehaviorTreeView()
		{
			InitializeComponent();
		}

		[Import]
		BehaviorTreeViewModel ViewModel
		{
			get
			{
				return this.DataContext as BehaviorTreeViewModel;
			}
			set
			{
				this.DataContext = value;
			}
		}

		private void NewNode_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Point point = Mouse.GetPosition(lstTree);
			var treeNode = new TreeNode(point.X, point.Y);
			this.ViewModel.Add(treeNode, null);
		}
	}
}
