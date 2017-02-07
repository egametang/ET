using System;
using System.Collections.Generic;
using Model;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
    public class RightDesigner : Editor
    {
        private readonly string[] mBehaviorToolbarStrings = {"节点", "工具"};
        private int mBehaviorToolbarSelection;
        private float mWidth = 380f;
      
        private BehaviorNodeData mCurBehaviorNode;
        private string mSearchNode = "";
        private FoldoutNode mCurNode;

        private int mNodeTypeSelection;
        private string[] mNodeTypeToolbarStrings = { "All", "Composite", "Decorator", "Action", "Condition", "Root", "DataTrans" };
        private int mEnumNodeTypeSelection;
        string[] mEnumNodeTypeArr;
       // private FoldoutFolder mNodeFoldout;

        private Vector2 mTreeScrollPos = Vector2.zero;
        private int mNodeCount;
        private FoldoutFolder mNodeFoldout;
        private FoldoutFolder mCurNodeFolder;
        public RightDesigner()
        {
            UpdateList();
        }
        Rect toolbarRect = new Rect(0f, 0f, 0, 0);
        public void Draw()
        {
           // HandleEvents();
            toolbarRect = new Rect(Screen.width - mWidth, 0f, mWidth, 18f);
            Rect boxRect = new Rect(Screen.width - mWidth, toolbarRect.height, this.mWidth, (Screen.height - toolbarRect.height) - 21f);
            GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
            this.mBehaviorToolbarSelection = GUILayout.Toolbar(this.mBehaviorToolbarSelection, this.mBehaviorToolbarStrings, EditorStyles.toolbarButton);
            GUILayout.EndArea();
            GUILayout.BeginArea(boxRect);
            Filter();
            if (mBehaviorToolbarSelection == 0)
            {
                DrawNodes();
            }
            else if (mBehaviorToolbarSelection == 1)
            {
                DrawTools();
            }
//             else if (mBehaviorToolbarSelection == 2)
//             {
//                 DrawTools();
//             }
//             else if (mBehaviorToolbarSelection == 3)
//             {
//                 DrawDebugView();
//             }
            GUILayout.EndArea();
        }
        private void DrawNodes()
        {
            float offset = 190f;
            if (mCurNode == null)
            {
                offset = 55f;
            }
            DrawSearchList(offset);
            DrawNodeFunctions(offset);
        }
        private string searchNodeName = "";
        private BehaviorTreeConfig searchTree;
        private GameObject[] searchGoArr = new GameObject[0];
        private Rect mBorderRect; //边框
        private Vector2 mScrollPosition = Vector2.zero;
        private Rect mGraphRect = new Rect(0, 0, 50, 50); //绘图区域
        private Vector2 scrollPosition = Vector2.zero;

        private void DrawTools()
        {
        }

        private void ShowResult()
        {
            Rect boxRect1 = new Rect(0f, 100, this.mWidth, Screen.height);
            GUILayout.BeginArea(boxRect1);

            scrollPosition = GUI.BeginScrollView(new Rect(0, 0, this.mWidth, 220), scrollPosition, new Rect(0, 0, this.mWidth - 20, searchGoArr.Length * 20));
            for (int i = 0; i < this.searchGoArr.Length; i++)
            {
                searchGoArr[i] = BehaviourTreeField((i + 1).ToString(), searchGoArr[i]);
            }

            GUI.EndScrollView();
            GUILayout.EndArea();
        }
        private BehaviorTreeConfig treeConfig;

        public GameObject BehaviourTreeField(string desc, GameObject value)
        {
            EditorGUILayout.BeginHorizontal();
            value = (GameObject)EditorGUILayout.ObjectField(desc, value, typeof(GameObject), false);
            if (value.GetComponent<BehaviorTreeConfig>() != null && GUILayout.Button("打开行为树"))
            {
                BehaviorManager.GetInstance().OpenBehaviorEditor(value);
                SetToolBar(2);
            }
            EditorGUILayout.EndHorizontal();
            return value;
        }
        public void SetToolBar(int select)
        {
            this.mBehaviorToolbarSelection = select;
        }

        private string DrawSearchList(float offset)
        {
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("Search");
            this.mSearchNode = GUILayout.TextField(this.mSearchNode, GUI.skin.FindStyle("ToolbarSeachTextField"));
            GUILayout.EndHorizontal();

            toolbarRect = new Rect(0f, 15f, mWidth, 25f);
            Rect boxRect = new Rect(0f, toolbarRect.height, this.mWidth, (Screen.height - toolbarRect.height) - 21f + 10);
            GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            GUILayout.Label("Filter");
            Array strArr = Enum.GetValues(typeof(NodeClassifyType));
            List<string> strList = new List<string>();
            strList.Add("All");
            foreach (var str in strArr)
            {
                strList.Add(str.ToString());
            }
            mEnumNodeTypeArr = strList.ToArray();
            mEnumNodeTypeSelection = EditorGUILayout.Popup(mEnumNodeTypeSelection, mEnumNodeTypeArr);
            if (GUILayout.Button("Clear"))
            {
                ClearNodes();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(0f, 15f + 20, this.mWidth, Screen.height - offset));
            this.mTreeScrollPos = GUI.BeginScrollView(new Rect(0f, 0f, this.mWidth, Screen.height - offset), this.mTreeScrollPos,
                    new Rect(0f, 0f, this.mWidth - 20f, mNodeCount * 19), false, false);
            mNodeFoldout.Draw();
            GUI.EndScrollView();
            GUILayout.EndArea();
            if (mCurNode != null)
            {
                string[] arr = mCurNode.Text.Split(' ');
                string name = arr[0];
                return name;
            }
            return "";
        }
        private void ClearNodes()
        {
            BehaviorManager.GetInstance().selectNodeName = "";
            mEnumNodeTypeSelection = 0;
            mSearchNode = "";
            foreach (FoldoutFolder folder in mNodeFoldout.Folders)
            {
                foreach (FoldoutNode node in folder.Nodes)
                {
                    node.Hide = false;
                }
            }
        }

        private void DrawNodeFunctions(float offset)
        {
            Rect boxRect = new Rect(0f, Screen.height - offset + 15f, this.mWidth, 200f);
            GUILayout.BeginArea(boxRect);
            BehaviorManager.GetInstance().selectNodeName = "";
            if (mCurNode != null)
            {
                string[] arr = mCurNode.Text.Split(' ');
                string name = arr[0];
                BehaviorManager.GetInstance().selectNodeName = name;
                BehaviorManager.GetInstance().selectNodeType = mCurNode.folderName;
                if (mCurNode.folderName != NodeClassifyType.Root.ToString())
                {
                    if (GUILayout.Button("新建"))
                    {
                        Game.Scene.GetComponent<EventComponent>().Run(EventIdType.BehaviorTreePropertyDesignerNewCreateClick, name, Vector2.zero);
                    }
                }
                if (mCurNode.folderName != NodeClassifyType.Root.ToString() ||
                    (mCurNode.folderName == NodeClassifyType.Root.ToString() && mCurBehaviorNode.IsRoot()))
                {
                    if (GUILayout.Button("替换"))
                    {
                        Game.Scene.GetComponent<EventComponent>().Run(EventIdType.BehaviorTreeReplaceClick, name, Vector2.zero);
                    }
                }

                if (GUILayout.Button("保存"))
                {
                    BehaviorManager.GetInstance().SaveAll();
                }
                var node = BehaviorManager.GetInstance().GetNodeTypeProto(name);
                GUILayout.Label("节点名:" + node.name);
                GUILayout.Label("描述:" + node.describe);
            }

            GUILayout.EndArea();
        }
        private void UpdateList()
        {
            mNodeFoldout = new FoldoutFolder("所有节点", SelectNodeFolderCallback);
            mNodeFoldout.Fold = true;

            foreach (var kv in BehaviorManager.GetInstance().Classify2NodeProtoList)
            {
                string classify = kv.Key;
                var nodeTypeList = kv.Value;
                FoldoutFolder folder = mNodeFoldout.AddFolder(classify, SelectNodeFolderCallback);
                folder.Fold = true;

                mNodeCount++;
                foreach (var nodeType in nodeTypeList)
                {
                    folder.AddNode(classify, nodeType.name + " (" + nodeType.describe + ")", SelectNodeCallback);
                    mNodeCount++;
                }
            }
        }
        private void Filter()
        {
            foreach (FoldoutFolder folder in mNodeFoldout.Folders)
            {
                string selectType;
                if (mEnumNodeTypeSelection == 0)
                {
                    selectType = "All";
                }
                else
                {
                    selectType = Enum.GetName(typeof(NodeClassifyType), mEnumNodeTypeSelection - 1);
                }

                if (selectType == folder.Text || selectType == "All")
                {
                    folder.Hide = false;
                    foreach (FoldoutNode node in folder.Nodes)
                    {
                        if (node.Text.ToUpper().IndexOf(mSearchNode.ToUpper()) == -1)
                        {
                            node.Hide = true;
                        }
                        else
                        {
                            node.Hide = false;
                        }
                    }
                }
                else
                {
                    foreach (FoldoutNode node in folder.Nodes)
                    {
                        node.Hide = true;
                    }
                    folder.Hide = true;
                }
            }
        }
        public void SelectNodeFolderCallback(FoldoutFolder folder)
        {
            folder.Select = true;
            if (mCurNodeFolder != null && mCurNodeFolder != folder)
            {
                mCurNodeFolder.Select = false;
                mCurNodeFolder = null;
            }
            mCurNodeFolder = folder;
        }

        public void SelectNodeCallback(FoldoutNode node)
        {
            node.Select = true;
            if (mCurNode != null && mCurNode != node)
            {
                mCurNode.Select = false;
                mCurNode = null;
            }
            mCurNode = node;
        }

        public void onDraggingBorder(float deltaX)
        {
            mWidth -= deltaX;
        }

        public void onSelectNode(params object[] list)
        {
            mCurBehaviorNode = (BehaviorNodeData)list[0];
            GUI.FocusControl("");
        }
    }
}