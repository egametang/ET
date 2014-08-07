using System.ComponentModel.Composition;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using Infrastructure;

namespace Modules.Tree
{
    /// <summary>
    /// BehaviorTreeView.xaml 的交互逻辑
    /// </summary>
    [ViewExport(RegionName = "BehaviorTreeRegion"), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class AllTreeView
    {
        public AllTreeView()
        {
            this.InitializeComponent();

            this.nodeDataEditor.AllTreeView = this;
            this.treeView.AllTreeView = this;
        }

        [Import]
        private AllTreeViewModel ViewModel
        {
            get
            {
                return this.DataContext as AllTreeViewModel;
            }
            set
            {
                this.DataContext = value;
            }
        }

        private void MenuItem_Open(object sender, RoutedEventArgs e)
        {
            string nodePath = ConfigurationManager.AppSettings["NodePath"];
            this.ViewModel.Open(nodePath);
            this.lbTreeRoots.SelectedIndex = -1;
            this.treeView.ViewModel = null;
        }

        private void MenuItem_Save(object sender, RoutedEventArgs e)
        {
            string nodePath = ConfigurationManager.AppSettings["NodePath"];
            this.ViewModel.Save(nodePath);
        }

        private void MenuItem_New(object sender, RoutedEventArgs e)
        {
            TreeViewModel treeViewModel = new TreeViewModel(this.ViewModel);
            this.ViewModel.Add(treeViewModel);
            this.treeView.ViewModel = treeViewModel;
        }

        private void MenuItem_Remove(object sender, RoutedEventArgs e)
        {
            if (this.lbTreeRoots.SelectedItem == null)
            {
                return;
            }
            var treeNodeViewModel = this.lbTreeRoots.SelectedItem as TreeNodeViewModel;
            this.ViewModel.Remove(treeNodeViewModel);
            this.lbTreeRoots.SelectedItem = null;
            e.Handled = true;
        }

        private void ListBoxItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var treeNodeViewModel = item.DataContext as TreeNodeViewModel;
            if (this.treeView.ViewModel != null)
            {
                if (this.treeView.ViewModel.TreeId == treeNodeViewModel.TreeId)
                {
                    return;
                }
            }
            this.treeView.ViewModel = this.ViewModel.Get(treeNodeViewModel.TreeId);
        }

        private void ListBoxItem_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var treeNodeViewModel = item.DataContext as TreeNodeViewModel;

            this.lbTreeRoots.SelectedItem = treeNodeViewModel;
        }
    }
}