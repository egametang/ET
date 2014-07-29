using System;
using System.Windows;
using System.Windows.Controls;
using Helper;

namespace Tree
{
    /// <summary>
    /// NodeDataEditor.xaml 的交互逻辑
    /// </summary>
    public partial class NodeDataEditor: UserControl
    {
        public NodeDataEditor()
        {
            this.InitializeComponent();

            string[] nodeTypes = Enum.GetNames(typeof (NodeType));
            this.cbType.ItemsSource = nodeTypes;
        }

        public TreeNodeViewModel ViewModel
        {
            get
            {
                return this.DataContext as TreeNodeViewModel;
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.ViewModel == null)
            {
                return;
            }
            this.cbType.SelectedIndex = EnumHelper.EnumIndex<NodeType>(this.ViewModel.Type);
        }

        private void CbType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cbType.SelectedValue == null)
            {
                this.ViewModel.Type = 0;
                return;
            }
            this.ViewModel.Type =
                    (int) Enum.Parse(typeof (NodeType), this.cbType.SelectedValue.ToString().Trim());
        }
    }
}