using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Tree
{
    public class NodeDataEditorViewModel: BindableBase
    {
        private IEventAggregator EventAggregator { get; set; }

        public TreeNodeViewModel TreeNodeViewModel { get; set; }

        public IEventNotifyView EventNotifyView { get; set; }

        public NodeDataEditorViewModel()
        {
        }

        public NodeDataEditorViewModel(IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            this.EventAggregator.GetEvent<SelectNodeChangeEvent>().Subscribe(this.OnSelectNodeChange);
        }

        private void OnSelectNodeChange(TreeNodeViewModel treeNodeViewModel)
        {
            this.TreeNodeViewModel = treeNodeViewModel;
            // 通知view层datacontext已经发生改变
            this.EventNotifyView.OnDataContextChange();
        }
    }
}