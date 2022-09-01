#if ENABLE_VIEW
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ET
{
    public class EntityTreeView: TreeView
    {
        private EntityTreeViewItem root;
        private int                id;

        private readonly Dictionary<int, Entity> all             = new();
        private readonly Dictionary<Entity, int> entityHistoryID = new();

        public EntityTreeView(TreeViewState state): base(state)
        {
            Reload();
            useScrollView = true;
        }

        public void Refresh()
        {
            this.root = BuildRoot() as EntityTreeViewItem;
            BuildRows(this.root);

            this.Repaint();
        }

        protected override TreeViewItem BuildRoot()
        {
            this.id = 0;

            this.root       = PreOrder(Root.Instance.Scene);
            this.root.depth = -1;

            SetupDepthsFromParentsAndChildren(this.root);

            return this.root;
        }

        private EntityTreeViewItem PreOrder(Entity root)
        {
            if(root is null)
            {
                return null;
            }

            if(!this.entityHistoryID.TryGetValue(root, out var itemID))
            {
                this.id++;
                itemID = this.id;

                this.entityHistoryID[root] = itemID;
            }

            EntityTreeViewItem item = new(root, itemID);

            this.all[itemID] = root;

            if(root.Components.Count > 0)
            {
                foreach(var component in root.Components.Values)
                {
                    item.AddChild(PreOrder(component));
                }
            }

            if(root.Children.Count > 0)
            {
                foreach(var child in root.Children.Values)
                {
                    item.AddChild(PreOrder(child));
                }
            }

            return item;
        }

        /// <summary>
        /// 处理右键内容
        /// </summary>
        /// <param name="id"></param>
        protected override void ContextClickedItem(int id)
        {
            if(Event.current.button != 1)
            {
                return;
            }

            SingleClickedItem(id);

            EntityContextMenu.Show(EntityTreeWindow.VIEW_MONO.Component);
        }

        /// <summary>
        /// 处理左键内容
        /// </summary>
        /// <param name="id"></param>
        protected override void SingleClickedItem(int id)
        {
            this.all.TryGetValue(id, out Entity entity);

            if(entity is null)
            {
                return;
            }

            EntityTreeWindow.VIEW_MONO.Component = entity;
            Selection.activeObject               = null;

            // 刷新 Inspector 显示
            EditorApplication.delayCall += () => { Selection.activeObject = EntityTreeWindow.VIEW_MONO; };
        }
    }
}
#endif