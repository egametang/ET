using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Infrastructure;
using Microsoft.Practices.Prism.PubSubEvents;

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
        }

        private void MenuItem_Save(object sender, RoutedEventArgs e)
        {
        }

        private void MenuItem_New(object sender, RoutedEventArgs e)
        {
            TreeViewModel treeViewModel = new TreeViewModel(new List<TreeNodeData>());
            this.ViewModel.Add(treeViewModel);
            this.treeView.ViewModel = treeViewModel;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (FrameworkElement)sender;
            var treeInfoViewModel = item.DataContext as TreeInfoViewModel;
            if (this.treeView.ViewModel.TreeId == treeInfoViewModel.Id)
            {
                return;
            }
            this.treeView.ViewModel = this.ViewModel.Get(treeInfoViewModel.Id);
        }
    }
}