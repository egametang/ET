using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace Tree
{
    [Export(contractType: typeof (AllTreeViewModel)),
     PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
    internal class AllTreeViewModel
    {
        private AllTreeData allTreeData;

        private readonly ObservableCollection<TreeInfoViewModel> treeInfos =
                new ObservableCollection<TreeInfoViewModel>();
    }
}