using System;
using System.Windows;
using System.Windows.Input;
using Common.Logger;

namespace Modules.BehaviorTreeModule
{
	public partial class TreeView
	{
		private const double DragThreshold = 5;
		private bool isDragging;
		private bool isLeftButtonDown;
		private Point origMouseDownPoint;
		private TreeNodeViewModel moveFromNode;

		public AllTreeView AllTreeView { get; set; }

		public TreeView()
		{
			this.InitializeComponent();
		}

		public TreeViewModel ViewModel
		{
			get
			{
				return this.DataContext as TreeViewModel;
			}
			set
			{
				this.DataContext = value;
			}
		}

		public bool IsDragging
		{
			get
			{
				return this.isDragging;
			}
			set
			{
				//Mouse.SetCursor(value == false? Cursors.Arrow : Cursors.Hand);
				this.isDragging = value;
			}
		}

		private void ListBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
		{
		}

		private void ListBoxItem_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (!this.isLeftButtonDown)
			{
				this.IsDragging = false;
				return;
			}

			var item = (FrameworkElement) sender;
			var treeNodeViewModel = item.DataContext as TreeNodeViewModel;

			if (!this.IsDragging)
			{
				this.listBox.SelectedItem = treeNodeViewModel;
			}

			this.isLeftButtonDown = false;
			this.IsDragging = false;

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
			if (this.IsDragging && treeNodeViewModel.IsRoot)
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
				this.IsDragging = false;
				this.moveFromNode = null;
				return;
			}

			curMouseDownPoint = e.GetPosition(this);
			dragDelta = curMouseDownPoint - this.origMouseDownPoint;
			double dragDistance = Math.Abs(dragDelta.Length);
			if (dragDistance > DragThreshold)
			{
				this.IsDragging = true;
			}
			e.Handled = true;
		}

		private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			this.origMouseDownPoint = e.GetPosition(this);
			var item = (FrameworkElement) sender;
			var treeNodeViewModel = item.DataContext as TreeNodeViewModel;

			this.listBox.SelectedItem = treeNodeViewModel;
			this.moveFromNode = treeNodeViewModel;

			this.AllTreeView.nodeDataEditor.DataContext = treeNodeViewModel;
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
			var item = (FrameworkElement) sender;
			var moveToNode = item.DataContext as TreeNodeViewModel;
			Log.Debug("move to node: {0} {1}", this.moveFromNode.Id, moveToNode.Id);
			if (this.moveFromNode.Id == moveToNode.Id)
			{
				return;
			}
			this.ViewModel.MoveToNode(this.moveFromNode, moveToNode);
			this.moveFromNode = null;
		}

		private void MenuItem_New(object sender, RoutedEventArgs e)
		{
			if (this.ViewModel == null)
			{
				return;
			}

			Point point = Mouse.GetPosition(this.listBox);

			// one root node
			if (this.ViewModel.TreeNodes.Count == 0)
			{
				var addTreeNode = new TreeNodeViewModel(this.ViewModel, point.X, point.Y)
				{
					Type = (int) NodeType.Selector
				};
				this.ViewModel.Add(addTreeNode, null);
			}
			else
			{
				if (this.listBox.SelectedItem != null)
				{
					var parentNode = this.listBox.SelectedItem as TreeNodeViewModel;
					var addTreeNode = new TreeNodeViewModel(this.ViewModel, parentNode)
					{
						Type = (int) NodeType.Selector,
					};
					this.ViewModel.Add(addTreeNode, parentNode);
				}
			}
			this.listBox.SelectedItem = null;
			e.Handled = true;
		}

		private void MenuItem_Remove(object sender, RoutedEventArgs e)
		{
			if (this.listBox.SelectedItem == null)
			{
				return;
			}
			var treeNodeViewModel = this.listBox.SelectedItem as TreeNodeViewModel;
			if (treeNodeViewModel.IsRoot)
			{
				return;
			}
			this.ViewModel.Remove(treeNodeViewModel);
			this.listBox.SelectedItem = null;
			e.Handled = true;
		}

		private void MenuItem_Fold(object sender, RoutedEventArgs e)
		{
			if (this.listBox.SelectedItem == null)
			{
				return;
			}
			var treeNodeViewModel = this.listBox.SelectedItem as TreeNodeViewModel;

			if (treeNodeViewModel.IsFold)
			{
				this.ViewModel.UnFold(treeNodeViewModel);
			}
			else
			{
				this.ViewModel.Fold(treeNodeViewModel);
			}
		}

		private void MenuItem_MoveLeft(object sender, RoutedEventArgs e)
		{
			if (this.listBox.SelectedItem == null)
			{
				return;
			}
			var treeNodeViewModel = this.listBox.SelectedItem as TreeNodeViewModel;
			this.ViewModel.MoveLeft(treeNodeViewModel);
		}

		private void MenuItem_MoveRight(object sender, RoutedEventArgs e)
		{
			if (this.listBox.SelectedItem == null)
			{
				return;
			}
			var treeNodeViewModel = this.listBox.SelectedItem as TreeNodeViewModel;
			this.ViewModel.MoveRight(treeNodeViewModel);
		}
	}
}