using System;
using System.Collections.Generic;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete("Use GTree and GTreeNode instead")]
    public class TreeView : EventDispatcher
    {
        /// <summary>
        /// TreeView使用的List对象
        /// </summary>
        public GList list { get; private set; }

        /// <summary>
        /// TreeView的顶层节点，这是个虚拟节点，也就是他不会显示出来。
        /// </summary>
        public TreeNode root { get; private set; }

        /// <summary>
        /// TreeView每级的缩进，单位像素。
        /// </summary>
        public int indent;

        public delegate GComponent TreeNodeCreateCellDelegate(TreeNode node);
        public delegate void TreeNodeRenderDelegate(TreeNode node);
        public delegate void TreeNodeWillExpandDelegate(TreeNode node, bool expand);

        /// <summary>
        /// 当TreeNode需要创建对象的显示对象时回调
        /// </summary>
        public TreeNodeCreateCellDelegate treeNodeCreateCell;

        /// <summary>
        /// 当TreeNode需要更新时回调
        /// </summary>
        public TreeNodeRenderDelegate treeNodeRender;

        /// <summary>
        /// 当TreeNode即将展开或者收缩时回调。可以在回调中动态增加子节点。
        /// </summary>
        public TreeNodeWillExpandDelegate treeNodeWillExpand;

        /// <summary>
        /// 点击任意TreeNode时触发
        /// </summary>
        public EventListener onClickNode { get; private set; }

        /// <summary>
        /// 右键点击任意TreeNode时触发
        /// </summary>
        public EventListener onRightClickNode { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        public TreeView(GList list)
        {
            this.list = list;
            list.onClickItem.Add(__clickItem);
            list.onRightClickItem.Add(__clickItem);
            list.RemoveChildrenToPool();

            root = new TreeNode(true);
            root.SetTree(this);
            root.cell = list;
            root.expanded = true;

            indent = 30;

            onClickNode = new EventListener(this, "onClickNode");
            onRightClickNode = new EventListener(this, "onRightClickNode");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TreeNode GetSelectedNode()
        {
            if (list.selectedIndex != -1)
                return (TreeNode)list.GetChildAt(list.selectedIndex).data;
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<TreeNode> GetSelection()
        {
            List<int> sels = list.GetSelection();
            int cnt = sels.Count;
            List<TreeNode> ret = new List<TreeNode>();
            for (int i = 0; i < cnt; i++)
            {
                TreeNode node = (TreeNode)list.GetChildAt(sels[i]).data;
                ret.Add(node);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="scrollItToView"></param>
        public void AddSelection(TreeNode node, bool scrollItToView = false)
        {
            TreeNode parentNode = node.parent;
            while (parentNode != null && parentNode != root)
            {
                parentNode.expanded = true;
                parentNode = parentNode.parent;
            }
            list.AddSelection(list.GetChildIndex(node.cell), scrollItToView);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void RemoveSelection(TreeNode node)
        {
            list.RemoveSelection(list.GetChildIndex(node.cell));
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearSelection()
        {
            list.ClearSelection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetNodeIndex(TreeNode node)
        {
            return list.GetChildIndex(node.cell);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void UpdateNode(TreeNode node)
        {
            if (node.cell == null)
                return;

            if (treeNodeRender != null)
                treeNodeRender(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        public void UpdateNodes(List<TreeNode> nodes)
        {
            int cnt = nodes.Count;
            for (int i = 0; i < cnt; i++)
            {
                TreeNode node = nodes[i];
                if (node.cell == null)
                    return;

                if (treeNodeRender != null)
                    treeNodeRender(node);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderNode"></param>
        public void ExpandAll(TreeNode folderNode)
        {
            folderNode.expanded = true;
            int cnt = folderNode.numChildren;
            for (int i = 0; i < cnt; i++)
            {
                TreeNode node = folderNode.GetChildAt(i);
                if (node.isFolder)
                    ExpandAll(node);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderNode"></param>
        public void CollapseAll(TreeNode folderNode)
        {
            if (folderNode != root)
                folderNode.expanded = false;
            int cnt = folderNode.numChildren;
            for (int i = 0; i < cnt; i++)
            {
                TreeNode node = folderNode.GetChildAt(i);
                if (node.isFolder)
                    CollapseAll(node);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        void CreateCell(TreeNode node)
        {
            if (treeNodeCreateCell != null)
                node.cell = treeNodeCreateCell(node);
            else
                node.cell = list.itemPool.GetObject(list.defaultItem) as GComponent;
            if (node.cell == null)
                throw new Exception("Unable to create tree cell");
            node.cell.data = node;

            GObject indentObj = node.cell.GetChild("indent");
            if (indentObj != null)
                indentObj.width = (node.level - 1) * indent;

            GButton expandButton = (GButton)node.cell.GetChild("expandButton");
            if (expandButton != null)
            {
                if (node.isFolder)
                {
                    expandButton.visible = true;
                    expandButton.onClick.Add(__clickExpandButton);
                    expandButton.data = node;
                    expandButton.selected = node.expanded;
                }
                else
                    expandButton.visible = false;
            }

            if (treeNodeRender != null)
                treeNodeRender(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        internal void AfterInserted(TreeNode node)
        {
            CreateCell(node);

            int index = GetInsertIndexForNode(node);
            list.AddChildAt(node.cell, index);
            if (treeNodeRender != null)
                treeNodeRender(node);

            if (node.isFolder && node.expanded)
                CheckChildren(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        int GetInsertIndexForNode(TreeNode node)
        {
            TreeNode prevNode = node.GetPrevSibling();
            if (prevNode == null)
                prevNode = node.parent;
            int insertIndex = list.GetChildIndex(prevNode.cell) + 1;
            int myLevel = node.level;
            int cnt = list.numChildren;
            for (int i = insertIndex; i < cnt; i++)
            {
                TreeNode testNode = (TreeNode)list.GetChildAt(i).data;
                if (testNode.level <= myLevel)
                    break;

                insertIndex++;
            }

            return insertIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        internal void AfterRemoved(TreeNode node)
        {
            RemoveNode(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        internal void AfterExpanded(TreeNode node)
        {
            if (node != root && treeNodeWillExpand != null)
                treeNodeWillExpand(node, true);

            if (node.cell == null)
                return;

            if (node != root)
            {
                if (treeNodeRender != null)
                    treeNodeRender(node);

                GButton expandButton = (GButton)node.cell.GetChild("expandButton");
                if (expandButton != null)
                    expandButton.selected = true;
            }

            if (node.cell.parent != null)
                CheckChildren(node, list.GetChildIndex(node.cell));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        internal void AfterCollapsed(TreeNode node)
        {
            if (node != root && treeNodeWillExpand != null)
                treeNodeWillExpand(node, false);

            if (node.cell == null)
                return;

            if (node != root)
            {
                if (treeNodeRender != null)
                    treeNodeRender(node);

                GButton expandButton = (GButton)node.cell.GetChild("expandButton");
                if (expandButton != null)
                    expandButton.selected = false;
            }

            if (node.cell.parent != null)
                HideFolderNode(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        internal void AfterMoved(TreeNode node)
        {
            if (!node.isFolder)
                list.RemoveChild(node.cell);
            else
                HideFolderNode(node);

            int index = GetInsertIndexForNode(node);
            list.AddChildAt(node.cell, index);

            if (node.isFolder && node.expanded)
                CheckChildren(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderNode"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        int CheckChildren(TreeNode folderNode, int index)
        {
            int cnt = folderNode.numChildren;
            for (int i = 0; i < cnt; i++)
            {
                index++;
                TreeNode node = folderNode.GetChildAt(i);
                if (node.cell == null)
                    CreateCell(node);

                if (node.cell.parent == null)
                    list.AddChildAt(node.cell, index);

                if (node.isFolder && node.expanded)
                    index = CheckChildren(node, index);
            }

            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderNode"></param>
        void HideFolderNode(TreeNode folderNode)
        {
            int cnt = folderNode.numChildren;
            for (int i = 0; i < cnt; i++)
            {
                TreeNode node = folderNode.GetChildAt(i);
                if (node.cell != null)
                {
                    if (node.cell.parent != null)
                        list.RemoveChild(node.cell);
                    list.itemPool.ReturnObject(node.cell);
                    node.cell.data = null;
                    node.cell = null;
                }
                if (node.isFolder && node.expanded)
                    HideFolderNode(node);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        void RemoveNode(TreeNode node)
        {
            if (node.cell != null)
            {
                if (node.cell.parent != null)
                    list.RemoveChild(node.cell);
                list.itemPool.ReturnObject(node.cell);
                node.cell.data = null;
                node.cell = null;
            }

            if (node.isFolder)
            {
                int cnt = node.numChildren;
                for (int i = 0; i < cnt; i++)
                {
                    TreeNode node2 = node.GetChildAt(i);
                    RemoveNode(node2);
                }
            }
        }

        void __clickExpandButton(EventContext context)
        {
            context.StopPropagation();

            GButton expandButton = (GButton)context.sender;
            TreeNode node = (TreeNode)expandButton.parent.data;
            if (list.scrollPane != null)
            {
                float posY = list.scrollPane.posY;
                if (expandButton.selected)
                    node.expanded = true;
                else
                    node.expanded = false;
                list.scrollPane.posY = posY;
                list.scrollPane.ScrollToView(node.cell);
            }
            else
            {
                if (expandButton.selected)
                    node.expanded = true;
                else
                    node.expanded = false;
            }
        }

        void __clickItem(EventContext context)
        {
            float posY = 0;
            if (list.scrollPane != null)
                posY = list.scrollPane.posY;

            TreeNode node = (TreeNode)((GObject)context.data).data;
            if (context.type == list.onRightClickItem.type)
                onRightClickNode.Call(node);
            else
                onClickNode.Call(node);

            if (list.scrollPane != null)
            {
                list.scrollPane.posY = posY;
                list.scrollPane.ScrollToView(node.cell);
            }
        }
    }
}
