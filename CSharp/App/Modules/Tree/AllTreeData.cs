using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tree
{
    [DataContract]
    public class AllTreeData
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
    }
}