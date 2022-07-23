using System;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class GTree : GList
    {
        public delegate void TreeNodeRenderDelegate(GTreeNode node, GComponent obj);
        public delegate void TreeNodeWillExpandDelegate(GTreeNode node, bool expand);

        /// <summary>
        /// 当TreeNode需要更新时回调
        /// </summary>
        public TreeNodeRenderDelegate treeNodeRender;

        /// <summary>
        /// 当TreeNode即将展开或者收缩时回调。可以在回调中动态增加子节点。
        /// </summary>
        public TreeNodeWillExpandDelegate treeNodeWillExpand;

        int _indent;
        GTreeNode _rootNode;
        int _clickToExpand;
        bool _expandedStatusInEvt;

        private static List<int> helperIntList = new List<int>();

        /// <summary>
        /// 
        /// </summary>
        public GTree()
        {
            _indent = 30;

            _rootNode = new GTreeNode(true);
            _rootNode._SetTree(this);
            _rootNode.expanded = true;

        }

        /// <summary>
        /// TreeView的顶层节点，这是个虚拟节点，也就是他不会显示出来。
        /// </summary>
        public GTreeNode rootNode
        {
            get { return _rootNode; }
        }

        /// <summary>
        /// TreeView每级的缩进，单位像素。
        /// </summary>
        public int indent
        {
            get { return _indent; }
            set { _indent = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int clickToExpand
        {
            get { return _clickToExpand; }
            set { _clickToExpand = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public GTreeNode GetSelectedNode()
        {
            int i = this.selectedIndex;
            if (i != -1)
                return (GTreeNode)this.GetChildAt(i)._treeNode;
            else
                return null;
        }

        /// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public List<GTreeNode> GetSelectedNodes()
        {
            return GetSelectedNodes(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public List<GTreeNode> GetSelectedNodes(List<GTreeNode> result)
        {
            if (result == null)
                result = new List<GTreeNode>();
            helperIntList.Clear();
            List<int> sels = GetSelection(helperIntList);
            int cnt = sels.Count;
            for (int i = 0; i < cnt; i++)
            {
                GTreeNode node = GetChildAt(sels[i])._treeNode;
                result.Add(node);
            }
            return result;
        }
        /// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
        public void SelectNode(GTreeNode node)
        {
            SelectNode(node, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="scrollItToView"></param>
        public void SelectNode(GTreeNode node, bool scrollItToView)
        {
            GTreeNode parentNode = node.parent;
            while (parentNode != null && parentNode != _rootNode)
            {
                parentNode.expanded = true;
                parentNode = parentNode.parent;
            }
            AddSelection(GetChildIndex(node.cell), scrollItToView);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void UnselectNode(GTreeNode node)
        {
            RemoveSelection(GetChildIndex(node.cell));
        }

        /// <summary>
		/// 
		/// </summary>
        public void ExpandAll()
        {
            ExpandAll(_rootNode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderNode"></param>
        public void ExpandAll(GTreeNode folderNode)
        {
            folderNode.expanded = true;
            int cnt = folderNode.numChildren;
            for (int i = 0; i < cnt; i++)
            {
                GTreeNode node = folderNode.GetChildAt(i);
                if (node.isFolder)
                    ExpandAll(node);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderNode"></param>
        public void CollapseAll()
        {
            CollapseAll(_rootNode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderNode"></param>
        public void CollapseAll(GTreeNode folderNode)
        {
            if (folderNode != _rootNode)
                folderNode.expanded = false;
            int cnt = folderNode.numChildren;
            for (int i = 0; i < cnt; i++)
            {
                GTreeNode node = folderNode.GetChildAt(i);
                if (node.isFolder)
                    CollapseAll(node);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        void CreateCell(GTreeNode node)
        {
            GComponent child = itemPool.GetObject(string.IsNullOrEmpty(node._resURL) ? this.defaultItem : node._resURL) as GComponent;
            if (child == null)
                throw new Exception("FairyGUI: cannot create tree node object.");
            child.displayObject.home = this.displayObject.cachedTransform;
            child._treeNode = node;
            node._cell = child;

            GObject indentObj = node.cell.GetChild("indent");
            if (indentObj != null)
                indentObj.width = (node.level - 1) * indent;

            Controller cc;

            cc = child.GetController("expanded");
            if (cc != null)
            {
                cc.onChanged.Add(__expandedStateChanged);
                cc.selectedIndex = node.expanded ? 1 : 0;
            }

            cc = child.GetController("leaf");
            if (cc != null)
                cc.selectedIndex = node.isFolder ? 0 : 1;

            if (node.isFolder)
                child.onTouchBegin.Add(__cellTouchBegin);

            if (treeNodeRender != null)
                treeNodeRender(node, node._cell);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        internal void _AfterInserted(GTreeNode node)
        {
            if (node._cell == null)
                CreateCell(node);

            int index = GetInsertIndexForNode(node);
            AddChildAt(node.cell, index);
            if (treeNodeRender != null)
                treeNodeRender(node, node._cell);

            if (node.isFolder && node.expanded)
                CheckChildren(node, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        int GetInsertIndexForNode(GTreeNode node)
        {
            GTreeNode prevNode = node.GetPrevSibling();
            if (prevNode == null)
                prevNode = node.parent;
            int insertIndex = GetChildIndex(prevNode.cell) + 1;
            int myLevel = node.level;
            int cnt = this.numChildren;
            for (int i = insertIndex; i < cnt; i++)
            {
                GTreeNode testNode = GetChildAt(i)._treeNode;
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
        internal void _AfterRemoved(GTreeNode node)
        {
            RemoveNode(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        internal void _AfterExpanded(GTreeNode node)
        {
            if (node == _rootNode)
            {
                CheckChildren(_rootNode, 0);
                return;
            }

            if (this.treeNodeWillExpand != null)
                this.treeNodeWillExpand(node, true);

            if (node._cell == null)
                return;

            if (this.treeNodeRender != null)
                this.treeNodeRender(node, node._cell);

            Controller cc = node._cell.GetController("expanded");
            if (cc != null)
                cc.selectedIndex = 1;

            if (node._cell.parent != null)
                CheckChildren(node, GetChildIndex(node._cell));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        internal void _AfterCollapsed(GTreeNode node)
        {
            if (node == _rootNode)
            {
                CheckChildren(_rootNode, 0);
                return;
            }

            if (this.treeNodeWillExpand != null)
                this.treeNodeWillExpand(node, false);

            if (node._cell == null)
                return;

            if (this.treeNodeRender != null)
                this.treeNodeRender(node, node._cell);

            Controller cc = node._cell.GetController("expanded");
            if (cc != null)
                cc.selectedIndex = 0;

            if (node._cell.parent != null)
                HideFolderNode(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        internal void _AfterMoved(GTreeNode node)
        {
            int startIndex = GetChildIndex(node._cell);
            int endIndex;
            if (node.isFolder)
                endIndex = GetFolderEndIndex(startIndex, node.level);
            else
                endIndex = startIndex + 1;
            int insertIndex = GetInsertIndexForNode(node);
            int cnt = endIndex - startIndex;

            if (insertIndex < startIndex)
            {
                for (int i = 0; i < cnt; i++)
                {
                    GObject obj = GetChildAt(startIndex + i);
                    SetChildIndex(obj, insertIndex + i);
                }
            }
            else
            {
                for (int i = 0; i < cnt; i++)
                {
                    GObject obj = GetChildAt(startIndex);
                    SetChildIndex(obj, insertIndex);
                }
            }
        }

        private int GetFolderEndIndex(int startIndex, int level)
        {
            int cnt = this.numChildren;
            for (int i = startIndex + 1; i < cnt; i++)
            {
                GTreeNode node = GetChildAt(i)._treeNode;
                if (node.level <= level)
                    return i;
            }

            return cnt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderNode"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        int CheckChildren(GTreeNode folderNode, int index)
        {
            int cnt = folderNode.numChildren;
            for (int i = 0; i < cnt; i++)
            {
                index++;
                GTreeNode node = folderNode.GetChildAt(i);
                if (node.cell == null)
                    CreateCell(node);

                if (node.cell.parent == null)
                    AddChildAt(node.cell, index);

                if (node.isFolder && node.expanded)
                    index = CheckChildren(node, index);
            }

            return index;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderNode"></param>
        void HideFolderNode(GTreeNode folderNode)
        {
            int cnt = folderNode.numChildren;
            for (int i = 0; i < cnt; i++)
            {
                GTreeNode node = folderNode.GetChildAt(i);
                if (node.cell != null && node.cell.parent != null)
                    RemoveChild(node.cell);

                if (node.isFolder && node.expanded)
                    HideFolderNode(node);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        void RemoveNode(GTreeNode node)
        {
            if (node.cell != null)
            {
                if (node.cell.parent != null)
                    RemoveChild(node.cell);
                itemPool.ReturnObject(node.cell);
                node._cell._treeNode = null;
                node._cell = null;
            }

            if (node.isFolder)
            {
                int cnt = node.numChildren;
                for (int i = 0; i < cnt; i++)
                {
                    GTreeNode node2 = node.GetChildAt(i);
                    RemoveNode(node2);
                }
            }
        }

        void __cellTouchBegin(EventContext context)
        {
            GTreeNode node = ((GObject)context.sender)._treeNode;
            _expandedStatusInEvt = node.expanded;
        }

        void __expandedStateChanged(EventContext context)
        {
            Controller cc = (Controller)context.sender;
            GTreeNode node = cc.parent._treeNode;
            node.expanded = cc.selectedIndex == 1;
        }

        override protected void DispatchItemEvent(GObject item, EventContext context)
        {
            if (_clickToExpand != 0)
            {
                GTreeNode node = item._treeNode;
                if (node != null && _expandedStatusInEvt == node.expanded)
                {
                    if (_clickToExpand == 2)
                    {
                        if (context.inputEvent.isDoubleClick)
                            node.expanded = !node.expanded;
                    }
                    else
                        node.expanded = !node.expanded;
                }
            }

            base.DispatchItemEvent(item, context);
        }

        override public void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_BeforeAdd(buffer, beginPos);

            buffer.Seek(beginPos, 9);

            _indent = buffer.ReadInt();
            _clickToExpand = buffer.ReadByte();
        }

        override protected void ReadItems(ByteBuffer buffer)
        {
            int nextPos;
            string str;
            bool isFolder;
            GTreeNode lastNode = null;
            int level;
            int prevLevel = 0;

            int cnt = buffer.ReadShort();
            for (int i = 0; i < cnt; i++)
            {
                nextPos = buffer.ReadUshort();
                nextPos += buffer.position;

                str = buffer.ReadS();
                if (str == null)
                {
                    str = this.defaultItem;
                    if (str == null)
                    {
                        buffer.position = nextPos;
                        continue;
                    }
                }

                isFolder = buffer.ReadBool();
                level = buffer.ReadByte();

                GTreeNode node = new GTreeNode(isFolder, str);
                node.expanded = true;
                if (i == 0)
                    _rootNode.AddChild(node);
                else
                {
                    if (level > prevLevel)
                        lastNode.AddChild(node);
                    else if (level < prevLevel)
                    {
                        for (int j = level; j <= prevLevel; j++)
                            lastNode = lastNode.parent;
                        lastNode.AddChild(node);
                    }
                    else
                        lastNode.parent.AddChild(node);
                }
                lastNode = node;
                prevLevel = level;

                SetupItem(buffer, node.cell);

                buffer.position = nextPos;
            }
        }
    }
}
