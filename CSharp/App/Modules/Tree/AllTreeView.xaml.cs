using System.ComponentModel.Composition;
using System.Windows;
using Infrastructure;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Tree
{
    /// <summary>
    /// BehaviorTreeView.xaml 的交互逻辑
    /// </summary>
    [ViewExport(RegionName = "BehaviorTreeRegion"), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class AllTreeView
    {
        private AllTreeViewModel allTreeViewModel;

        public AllTreeView()
        {
            this.InitializeComponent();

            this.nodeDataEditor.AllTreeView = this;
            this.treeView.AllTreeView = this;
            this.treeView.TreeViewModel = new TreeViewModel();
        }

        [Import]
        private AllTreeViewModel AllTreeViewModel
        {
            get
            {
                return allTreeViewModel;
            }
            set
            {
                this.allTreeViewModel = value;
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
            this.treeView.TreeViewModel = new TreeViewModel();
        }
    }
}