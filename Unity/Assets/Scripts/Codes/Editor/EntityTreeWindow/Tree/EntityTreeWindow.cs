#if ENABLE_VIEW
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ET
{
    public class EntityTreeWindow: EditorWindow
    {
        internal static ComponentView VIEW_MONO;

        private static EntityTreeWindow WINDOW;

        private EntityTreeView treeView;
        private SearchField    searchField;

        [MenuItem("ET/Entity Tree Window")]
        private static void OpenWindow()
        {
            if(!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("警告", "运行后才可使用", "确定");
                return;
            }

            VIEW_MONO = new GameObject("View").AddComponent<ComponentView>();
            DontDestroyOnLoad(VIEW_MONO);

            WINDOW              = GetWindow<EntityTreeWindow>(DockDefine.Types);
            WINDOW.titleContent = new GUIContent("Entity Tree Window");
            WINDOW.Show();
        }

        private void OnEnable()
        {
            this.treeView                            =  new EntityTreeView(new TreeViewState());
            this.searchField                         =  new SearchField();
            this.searchField.downOrUpArrowKeyPressed += this.treeView.SetFocusAndEnsureSelectedItem;
            EditorApplication.playModeStateChanged   += OnPlayModeStateChange;
        }

        private void OnPlayModeStateChange(PlayModeStateChange state)
        {
            if(state != PlayModeStateChange.ExitingPlayMode)
            {
                return;
            }

            WINDOW.Close();
        }

        private void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            DestroyImmediate(VIEW_MONO.gameObject);
            VIEW_MONO = null;
        }

        private void OnInspectorUpdate()
        {
            this.treeView?.Refresh();
        }

        private void OnGUI()
        {
            this.treeView.searchString = this.searchField.OnGUI(
                new Rect(
                    0,
                    0,
                    position.width - 40f,
                    20f
                ),
                this.treeView.searchString
            );

            this.treeView.OnGUI(
                new Rect(
                    0,
                    20f,
                    position.width,
                    position.height - 40f
                )
            );


            GUILayout.BeginArea(
                new Rect(
                    20f,
                    position.height - 18f,
                    position.width  - 40f,
                    16f
                )
            );

            using(new EditorGUILayout.HorizontalScope())
            {
                string style = "miniButton";
                if(GUILayout.Button("Expand all", style))
                {
                    this.treeView.ExpandAll();
                }

                if(GUILayout.Button("Collapse all", style))
                {
                    this.treeView.CollapseAll();
                }
            }

            GUILayout.EndArea();
        }
    }
}
#endif