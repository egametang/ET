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

        [ImportingConstructor]
        public AllTreeView(IEventAggregator eventAggregator)
        {
            this.InitializeComponent();

            this.nodeDataEditor.NodeDataEditorViewModel = new NodeDataEditorViewModel(eventAggregator);
            this.treeView.TreeViewModel = new TreeViewModel(eventAggregator);
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
    }
}