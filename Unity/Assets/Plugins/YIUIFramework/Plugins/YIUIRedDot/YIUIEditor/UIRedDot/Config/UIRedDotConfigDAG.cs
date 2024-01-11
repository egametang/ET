#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YIUIFramework.Editor
{
    internal class Node
    {
        public ERedDotKeyType Key      { get; set; }
        public List<Node>     Parents  { get; set; }
        public List<Node>     Children { get; set; }

        public Node(ERedDotKeyType key)
        {
            Key      = key;
            Parents  = new List<Node>();
            Children = new List<Node>();
        }
    }

    internal class UIRedDotConfigDAG
    {
        private List<Node> Nodes { get; set; }

        public UIRedDotConfigDAG()
        {
            Nodes = new List<Node>();
        }

        public void AddNode(ERedDotKeyType key)
        {
            Nodes.Add(new Node(key));
        }

        public void AddEdge(ERedDotKeyType parentKey, ERedDotKeyType childKey)
        {
            var parent = Nodes.Find(n => n.Key == parentKey);
            var child  = Nodes.Find(n => n.Key == childKey);

            parent.Children.Add(child);
            child.Parents.Add(parent);
        }

        public bool Check()
        {
            return CheckCyclesSort(Nodes);
        }

        //拓扑排序算法 检查循环引用
        private static bool CheckCyclesSort(List<Node> nodes)
        {
            var sortedNodes         = new List<Node>();
            var nodesWithoutParents = new Queue<Node>(nodes.Where(n => n.Parents.Count == 0));
            while (nodesWithoutParents.Count > 0)
            {
                var node = nodesWithoutParents.Dequeue();
                sortedNodes.Add(node);
                foreach (var child in node.Children)
                {
                    child.Parents.Remove(node);
                    if (child.Parents.Count == 0)
                    {
                        nodesWithoutParents.Enqueue(child);
                    }
                }
            }

            if (nodes.Any(n => n.Parents.Count > 0))
            {
                return false;
            }

            return true;
        }
    }
}
#endif