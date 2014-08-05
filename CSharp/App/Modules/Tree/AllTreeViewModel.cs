using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using Common.Helper;

namespace Modules.Tree
{
    [Export(contractType: typeof (AllTreeViewModel)),
     PartCreationPolicy(creationPolicy: CreationPolicy.NonShared)]
    public class AllTreeViewModel
    {
        public int MaxNodeId { get; set; }
        public int MaxTreeId { get; set; }
        
        private readonly Dictionary<int, TreeViewModel> treeViewModelsDict = new Dictionary<int, TreeViewModel>();

        public ObservableCollection<TreeNodeViewModel> rootList = new ObservableCollection<TreeNodeViewModel>();

        public ObservableCollection<TreeNodeViewModel> RootList
        {
            get
            {
                return this.rootList;
            }
        }

        public void Open(string file)
        {
            this.rootList.Clear();
            treeViewModelsDict.Clear();

            var treeDict = new Dictionary<int, List<TreeNodeData>>();

            byte[] bytes = File.ReadAllBytes(file);
            var allTreeData = ProtobufHelper.FromBytes<AllTreeData>(bytes);

            this.MaxNodeId = 0;
            this.MaxTreeId = 0;
            foreach (TreeNodeData treeNodeData in allTreeData.TreeNodeDatas)
            {
                List<TreeNodeData> tree;
                treeDict.TryGetValue(treeNodeData.TreeId, out tree);
                if (tree == null)
                {
                    tree = new List<TreeNodeData>();
                    treeDict[treeNodeData.TreeId] = tree;
                }
                tree.Add(treeNodeData);
                if (treeNodeData.Id > this.MaxNodeId)
                {
                    this.MaxNodeId = treeNodeData.Id;
                }
                if (treeNodeData.TreeId > this.MaxTreeId)
                {
                    this.MaxTreeId = treeNodeData.TreeId;
                }
            }

            foreach (KeyValuePair<int, List<TreeNodeData>> pair in treeDict)
            {
                var treeViewModel = new TreeViewModel(pair.Value)
                {
                    AllTreeViewModel = this,
                    TreeId = pair.Key
                };
                treeViewModelsDict[pair.Key] = treeViewModel;
                this.RootList.Add(treeViewModel.Root);
            }
        }

        public void Save(string file)
        {

            AllTreeData allTreeData = new AllTreeData();
            foreach (var value in treeViewModelsDict.Values)
            {
                List<TreeNodeData> list = value.GetDatas();
                allTreeData.TreeNodeDatas.AddRange(list);
            }

            using (Stream stream = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                byte[] bytes = ProtobufHelper.ToBytes(allTreeData);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public void New(TreeViewModel treeViewModel)
        {
        }

        public void Add(TreeViewModel treeViewModel)
        {
            treeViewModelsDict[treeViewModel.TreeId] = treeViewModel;
            this.rootList.Add(treeViewModel.Root);
        }

        public void Remove(TreeNodeViewModel treeViewModel)
        {
            treeViewModelsDict.Remove(treeViewModel.TreeId);
            rootList.Remove(treeViewModel);
        }

        public TreeViewModel Get(int treeId)
        {
            return this.treeViewModelsDict[treeId];
        }

    }
}