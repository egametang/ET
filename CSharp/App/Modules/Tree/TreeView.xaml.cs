using System;
using System.Windows;
using System.Windows.Input;
using Logger;

namespace Tree
{
    /// <summary>
    /// TreeView.xaml 的交互逻辑
    /// </summary>
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

        public TreeViewModel TreeViewModel
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

        private void ListBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            // 双击鼠标
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                var item = (FrameworkElement)sender;
                var treeNodeViewModel = item.DataContext as TreeNodeViewModel;
                if (treeNodeViewModel.IsFolder)
                {
                    this.TreeViewModel.UnFold(treeNodeViewModel);
                }
                else
                {
                    this.TreeViewModel.Fold(treeNodeViewModel);
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

            var item = (FrameworkElement)sender;
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
            var item = (FrameworkElement)sender;
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

                this.TreeViewModel.MoveToPosition(dragDelta.X, dragDelta.Y);
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
            this.origMouseDownPoint = e.GetPosition(this);
            var item = (FrameworkElement)sender;
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
            var item = (FrameworkElement)sender;
            var moveToNode = item.DataContext as TreeNodeViewModel;
            Log.Debug("move to node: {0} {1}", this.moveFromNode.Id, moveToNode.Id);
            if (this.moveFromNode.Id == moveToNode.Id)
            {
                return;
            }
            this.TreeViewModel.MoveToNode(this.moveFromNode, moveToNode);
            this.moveFromNode = null;
        }

        private void MenuItem_New(object sender, RoutedEventArgs e)
        {
            Point point = Mouse.GetPosition(this.listBox);

            // one root node
            if (this.TreeViewModel.TreeNodes.Count == 0)
            {
                var addTreeNode = new TreeNodeViewModel(this.TreeViewModel, point.X, point.Y)
                {
                    Type = (int)NodeType.Selector
                };
                this.TreeViewModel.Add(addTreeNode, null);
            }
            else
            {
                if (this.listBox.SelectedItem != null)
                {
                    var parentNode = this.listBox.SelectedItem as TreeNodeViewModel;
                    var addTreeNode = new TreeNodeViewModel(this.TreeViewModel, parentNode)
                    {
                        Type = (int)NodeType.Selector
                    };
                    this.TreeViewModel.Add(addTreeNode, parentNode);
                }
            }
            this.listBox.SelectedItem = null;
            e.Handled = true;
        }

        private void MenuItem_Delete(object sender, RoutedEventArgs e)
        {
            if (this.listBox.SelectedItem == null)
            {
                return;
            }
            var treeNodeViewModel = this.listBox.SelectedItem as TreeNodeViewModel;
            this.TreeViewModel.Remove(treeNodeViewModel);
            this.listBox.SelectedItem = null;
            e.Handled = true;
        }
    }
}
