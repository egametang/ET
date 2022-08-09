#if ENABLE_CODES
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ET
{
    public class EntityTreeView : TreeView
    {
        private EntityTreeViewItem root;
        private int                id;

        public Dictionary<int, Entity> all = new();

        public EntityTreeView(TreeViewState state) : base(state)
        {
            Reload();
            useScrollView = true;
        }

        protected override TreeViewItem BuildRoot()
        {
            this.id = 0;

            this.root       = PreOrder(Game.Scene);
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

            this.id++;

            var item = new EntityTreeViewItem(root, this.id);

            all[this.id] = root;

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
            all.TryGetValue(id, out var entity);

            if(entity is null)
            {
                return;
            }

            EntityTreeWindow.VIEW_MONO.Component = entity;
            Selection.activeObject            = EntityTreeWindow.VIEW_MONO;
        }
    }
}
#endif