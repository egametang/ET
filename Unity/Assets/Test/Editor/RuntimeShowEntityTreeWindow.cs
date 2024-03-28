using ET;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 游戏运行时组件查看
    /// </summary>
    public class RuntimeShowEntityTreeWindow : EditorWindow
    {
        private static RuntimeShowEntityTreeWindow _runtimeShowEntityWindow;
        private Vector2 _scorllPos;

        [MenuItem("BFramework/Runtime Show EntityTree", false, 100)]
        private static void OpenWindow()
        {
            RuntimeShowEntityTreeWindow win = CreateWindow<RuntimeShowEntityTreeWindow>("游戏运行时组件查看");
            win.position = new Rect(100, 100, 600, 450);
            win.OnInit();
            _runtimeShowEntityWindow?.Close();
            _runtimeShowEntityWindow = win;
        }

        private void OnInit()
        {
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                GUI.contentColor = Color.red;
                GUILayout.Label("仅在运行下显示池对象概况");
                return;
            }

            _scorllPos = EditorGUILayout.BeginScrollView(_scorllPos);
            
            //根节点
            EditorGUI.indentLevel = 0;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Foldout(true, "FiberManager");
            EditorGUILayout.EndHorizontal();

            //Fiber
            var fibers = FiberManager.Instance.GetFibers();
            foreach (var item in fibers)
            {
                Scene node = item.Value.Root;
                this.DrawNode(node, 1);
            }

            EditorGUILayout.EndScrollView();

            //实时重绘
            this.Repaint();
        }

        private void DrawNode(Entity entity, int depth)
        {
            EditorGUI.indentLevel = depth;
            EditorGUILayout.BeginHorizontal();
            string key = entity is Scene ? entity.Scene().SceneType.ToString() : entity.GetType().Name;
            entity.IsExpanded = EditorGUILayout.Foldout(entity.IsExpanded, key);
            EditorGUILayout.EndHorizontal();

            if (entity.IsExpanded)
            {
                foreach (var component in entity.Components)
                {
                    this.DrawNode(component.Value, depth + 1);
                }
                foreach (var children in entity.Children)
                {
                    this.DrawNode(children.Value, depth + 1);
                }
            }
        }
    }
}