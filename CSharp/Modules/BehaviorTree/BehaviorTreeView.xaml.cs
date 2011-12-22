using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Infrastructure;

namespace BehaviorTree
{
	/// <summary>
	/// BehaviorTreeView.xaml 的交互逻辑
	/// </summary>
	[ViewExport(RegionName = "TreeCanvasRegion")]
	[PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class BehaviorTreeView : UserControl
	{
		private static readonly double DragThreshold = 5;

		private bool isDragging = false;
		private bool isControlDown = false;
		private bool isLeftButtonDown = false;
		private Point origMouseDownPoint;

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

		private void MenuNewNode_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Point point = Mouse.GetPosition(listBox);
			var treeNode = new TreeNode(point.X, point.Y);

			// one root node
			if (this.ViewModel.TreeNodes.Count == 0)
			{
				this.ViewModel.Add(treeNode, null);
			}
			else
			{
				if (listBox.SelectedItem != null)
				{
					var treeNodeViewModel = listBox.SelectedItem as TreeNodeViewModel;
					this.ViewModel.Add(treeNode, treeNodeViewModel);
				}
			}
			listBox.SelectedItem = null;
			e.Handled = true;
		}

		private void ListBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
			{
				return;
			}
			var item = (FrameworkElement)sender;
			var treeNodeViewModel = item.DataContext as TreeNodeViewModel;

			isLeftButtonDown = true;

			if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
			{
				isControlDown = true;
			}
			else
			{
				isControlDown = false;

				if (!listBox.SelectedItems.Contains(treeNodeViewModel))
				{
					listBox.SelectedItems.Clear();
					listBox.SelectedItems.Add(treeNodeViewModel);
				}
			}

			item.CaptureMouse();
			origMouseDownPoint = e.GetPosition(this);
			e.Handled = true;
		}

		private void ListBoxItem_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (!isLeftButtonDown)
			{
				isDragging = false;
				return;
			}

			var item = (FrameworkElement)sender;
			var treeNodeViewModel = item.DataContext as TreeNodeViewModel;

			if (!isDragging)
			{
				if (isLeftButtonDown && isControlDown)
				{
					if (listBox.SelectedItems.Contains(treeNodeViewModel))
					{
						listBox.SelectedItems.Remove(treeNodeViewModel);
					}
					else
					{
						listBox.SelectedItems.Add(treeNodeViewModel);
					}
				}
				else
				{
					if (listBox.SelectedItems.Count != 1 || listBox.SelectedItem != treeNodeViewModel)
					{
						listBox.SelectedItems.Clear();
						listBox.SelectedItem = treeNodeViewModel;
						listBox.SelectedItems.Add(treeNodeViewModel);
					}
				}
			}

			isLeftButtonDown = false;
			isControlDown = false;
			isDragging = false;

			item.ReleaseMouseCapture();
			e.Handled = true;
		}

		private void ListBoxItem_MouseMove(object sender, MouseEventArgs e)
		{
			if (isDragging)
			{
				Point curMouseDownPoint = e.GetPosition(this);
				var dragDelta = curMouseDownPoint - origMouseDownPoint;

				origMouseDownPoint = curMouseDownPoint;

				foreach (TreeNodeViewModel item in listBox.SelectedItems)
				{
					item.X += dragDelta.X;
					item.Y += dragDelta.Y;
				}
			}
			else if (isLeftButtonDown)
			{
				Point curMouseDownPoint = e.GetPosition(this);
				var dragDelta = curMouseDownPoint - origMouseDownPoint;
				double dragDistance = Math.Abs(dragDelta.Length);
				if (dragDistance > DragThreshold)
				{
					isDragging = true;
				}
				e.Handled = true;
			}
		}
	}
}
