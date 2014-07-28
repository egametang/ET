using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Infrastructure;
using Logger;

namespace Tree
{
	/// <summary>
	/// BehaviorTreeView.xaml 的交互逻辑
	/// </summary>
	[ViewExport(RegionName = "BehaviorTreeRegion"), PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class BehaviorTreeView
	{
		private const double DragThreshold = 5;
		private bool isDragging;
		private bool isLeftButtonDown;
		private Point origMouseDownPoint;
		private TreeNodeViewModel moveFromNode;

		public BehaviorTreeView()
		{
			this.InitializeComponent();
		}

		[Import]
		private BehaviorTreeViewModel ViewModel
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

		private void MenuNode_New(object sender, ExecutedRoutedEventArgs e)
		{
			Point point = Mouse.GetPosition(this.listBox);

			// one root node
			if (this.ViewModel.TreeNodes.Count == 0)
			{
				var addTreeNode = new TreeNodeViewModel(point.X, point.Y) { Type = (int) NodeType.Selector };
				this.ViewModel.Add(addTreeNode, null);
			}
			else
			{
				if (this.listBox.SelectedItem != null)
				{
					var parentNode = this.listBox.SelectedItem as TreeNodeViewModel;
					var addTreeNode = new TreeNodeViewModel(parentNode) { Type = (int) NodeType.Selector };
					this.ViewModel.Add(addTreeNode, parentNode);
				}
			}
			this.listBox.SelectedItem = null;
			e.Handled = true;
		}

		private void MenuNode_Delete(object sender, ExecutedRoutedEventArgs e)
		{
			if (this.listBox.SelectedItem == null)
			{
				return;
			}
			var treeNodeViewModel = this.listBox.SelectedItem as TreeNodeViewModel;
			this.ViewModel.Remove(treeNodeViewModel);
			this.listBox.SelectedItem = null;
			e.Handled = true;
		}

		private void MenuNode_Save(object sender, ExecutedRoutedEventArgs e)
		{
			this.ViewModel.Save("node.bytes");
		}

		private void MenuNode_Open(object sender, ExecutedRoutedEventArgs e)
		{
			this.ViewModel.Load("node.bytes");
		}

		private void ListBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
			{
				return;
			}

			// 双击鼠标
			if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
			{
				var item = (FrameworkElement) sender;
				var treeNodeViewModel = item.DataContext as TreeNodeViewModel;
				if (treeNodeViewModel.IsFolder)
				{
					this.ViewModel.UnFold(treeNodeViewModel);
				}
				else
				{
					this.ViewModel.Fold(treeNodeViewModel);	
				}
			}
			e.Handled = true;
		}

		private void ListBoxItem_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (!this.isLeftButtonDown)
			{
				this.isDragging = false;
				return;
			}

			var item = (FrameworkElement) sender;
			var treeNodeViewModel = item.DataContext as TreeNodeViewModel;

			if (!this.isDragging)
			{
				this.listBox.SelectedItem = treeNodeViewModel;
			}

			this.isLeftButtonDown = false;
			this.isDragging = false;

			item.ReleaseMouseCapture();
			e.Handled = true;
		}

		private void ListBoxItem_MouseMove(object sender, MouseEventArgs e)
		{
			var item = (FrameworkElement) sender;
			var treeNodeViewModel = item.DataContext as TreeNodeViewModel;
			if (treeNodeViewModel == null)
			{
				return;
			}

			Point curMouseDownPoint;
			Vector dragDelta;
			// 拖动根节点,移动整个树
			if (this.isDragging && treeNodeViewModel.IsRoot)
			{
				if (this.moveFromNode == null || !this.moveFromNode.IsRoot)
				{
					return;
				}
				curMouseDownPoint = e.GetPosition(this);
				dragDelta = curMouseDownPoint - this.origMouseDownPoint;

				this.origMouseDownPoint = curMouseDownPoint;

				this.ViewModel.MoveToPosition(dragDelta.X, dragDelta.Y);
				return;
			}

			if (e.LeftButton != MouseButtonState.Pressed)
			{
				this.isDragging = false;
				this.moveFromNode = null;
				return;
			}

			curMouseDownPoint = e.GetPosition(this);
			dragDelta = curMouseDownPoint - this.origMouseDownPoint;
			double dragDistance = Math.Abs(dragDelta.Length);
			if (dragDistance > DragThreshold)
			{
				this.isDragging = true;
			}
			e.Handled = true;
		}

		private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			origMouseDownPoint = e.GetPosition(this);
			var item = (FrameworkElement)sender;
			var treeNodeViewModel = item.DataContext as TreeNodeViewModel;

			this.listBox.SelectedItem = treeNodeViewModel;
			this.moveFromNode = treeNodeViewModel;

			this.nodeDataEditor.DataContext = treeNodeViewModel;
		}

		private void ListBoxItem_PreviewMouseLeftButtonUp(object sender, MouseEventArgs e)
		{
			if (this.moveFromNode == null)
			{
				return;
			}
			if (this.moveFromNode.IsRoot)
			{
				return;
			}
			var item = (FrameworkElement)sender;
			var moveToNode = item.DataContext as TreeNodeViewModel;
			Log.Debug("move to node: {0} {1}", moveFromNode.Id, moveToNode.Id);
			if (this.moveFromNode.Id == moveToNode.Id)
			{
				return;
			}
			this.ViewModel.MoveToNode(this.moveFromNode, moveToNode);
			this.moveFromNode = null;
		}
	}
}