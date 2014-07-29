using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tree
{
    [DataContract]
    public class TreeNodeDataArray
    {
        private readonly List<TreeNodeData> treeNodeDatas = new List<TreeNodeData>();

        [DataMember(Order = 1)]
        public List<TreeNodeData> TreeNodeDatas
        {
            get
            {
                return this.treeNodeDatas;
            }
        }

        private readonly Dictionary<int, TreeNodeData> treeNodeDict =
                new Dictionary<int, TreeNodeData>();

        public void Init()
        {
            foreach (TreeNodeData nodeData in this.treeNodeDatas)
            {
                this.treeNodeDict[nodeData.Id] = nodeData;
            }
        }

        public void Add(TreeNodeData treeNodeData)
        {
            this.treeNodeDatas.Add(treeNodeData);
        }

        public TreeNodeData this[int id]
        {
            get
            {
                return this.treeNodeDict[id];
            }
        }
    }
}