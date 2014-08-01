using System;
using System.Windows;
using System.Windows.Controls;
using Helper;

namespace Tree
{
    /// <summary>
    /// NodeDataEditor.xaml 的交互逻辑
    /// </summary>
    public partial class NodeDataEditor : IEventNotifyView
    {
        private NodeDataEditorViewModel nodeDataEditorViewModel;

        public NodeDataEditor()
        {
            this.InitializeComponent();

            string[] nodeTypes = Enum.GetNames(typeof (NodeType));
            this.cbType.ItemsSource = nodeTypes;
        }
       
        public NodeDataEditorViewModel NodeDataEditorViewModel
        {
            get
            {
                return this.nodeDataEditorViewModel;
            }
            set
            {
                this.nodeDataEditorViewModel = value;
                this.nodeDataEditorViewModel.EventNotifyView = this;
                this.TreeNodeViewModel = this.nodeDataEditorViewModel.TreeNodeViewModel;
            }
        }

        public TreeNodeViewModel TreeNodeViewModel
        {
            get
            {
                return this.DataContext as TreeNodeViewModel;
            }
            set
            {
                this.DataContext = value;
            }
        }

        public void OnDataContextChange()
        {
            this.DataContext = this.nodeDataEditorViewModel.TreeNodeViewModel;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.TreeNodeViewModel == null)
            {
                return;
            }
            this.cbType.SelectedIndex = EnumHelper.EnumIndex<NodeType>(this.TreeNodeViewModel.Type);
        }

        private void CbType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.TreeNodeViewModel == null)
            {
                return;
            }
            if (this.cbType.SelectedValue == null)
            {
                this.TreeNodeViewModel.Type = 0;
                return;
            }
            this.TreeNodeViewModel.Type =
                    (int) Enum.Parse(typeof (NodeType), this.cbType.SelectedValue.ToString().Trim());
        }
    }
}