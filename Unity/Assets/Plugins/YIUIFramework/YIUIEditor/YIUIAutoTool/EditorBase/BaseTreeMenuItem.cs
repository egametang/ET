#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    [HideReferenceObjectPicker]
    public abstract class BaseTreeMenuItem : BaseYIUIToolModule
    {
        private bool m_InitEnd;

        protected BaseTreeMenuItem(OdinMenuEditorWindow autoTool, OdinMenuTree tree)
        {
            AutoTool = autoTool;
            Tree     = tree;
        }

        public override void SelectionMenu()
        {
            Init();
            Select();
        }

        private void Init()
        {
            if (m_InitEnd) return;
            Initialize();
            m_InitEnd = true;
        }

        protected abstract void Select();
    }

    [HideReferenceObjectPicker]
    public class TreeMenuItem<T> : BaseTreeMenuItem where T : BaseYIUIToolModule, new()
    {
        [ShowInInspector]
        [HideLabel]
        [HideReferenceObjectPicker]
        public T Instance { get; internal set; }

        public TreeMenuItem(OdinMenuEditorWindow autoTool, OdinMenuTree tree, string menuName, Texture icon) : base(autoTool,
            tree)
        {
            Tree.Add(menuName, this, icon);
            ModuleName = menuName;
        }

        public TreeMenuItem(OdinMenuEditorWindow autoTool, OdinMenuTree tree, string menuName, EditorIcon icon) : base(autoTool,
            tree)
        {
            Tree.Add(menuName, this, icon);
            ModuleName = menuName;
        }

        public override void Initialize()
        {
            Instance = new T
            {
                AutoTool   = AutoTool,
                Tree       = Tree,
                ModuleName = ModuleName,
            };
            Instance?.Initialize();
        }

        public override void OnDestroy()
        {
            Instance?.OnDestroy();
        }

        protected override void Select()
        {
            Instance?.SelectionMenu();
        }
    }
}
#endif