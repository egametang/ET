using System;
using System.Collections.Generic;
using ETModel;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
	public class BehaviorTreeNodeClassPopup
	{
		private string mSearchNode = "";
		private Vector2 mTreeScrollPos = Vector2.zero;
		private const float mWidth = 400f;

		public GraphDesigner GraphDesigner;
		private SubWinType mSubWinType;
		public Rect windowRect; //子窗口的大小和位置
		private string[] mEnumNodeTypeArr;
		private Rect toolbarRect = new Rect(0f, 0f, 0, 0);
		private int mEnumNodeTypeSelection;

		public string DrawSearchList()
		{
			List<string> targetList = new List<string>();
			if (mSubWinType == SubWinType.CreateNode)
			{
				targetList = GraphDesigner.GetCanCreateList();
			}
			else if (mSubWinType == SubWinType.ReplaceNode)
			{
				targetList = GraphDesigner.GetCanRepalceList();
			}

			List<string> nodeNameList = Filter(targetList, mSearchNode);

			GUILayout.BeginHorizontal();
			GUI.SetNextControlName("Search");
			this.mSearchNode = GUILayout.TextField(this.mSearchNode, GUI.skin.FindStyle("ToolbarSeachTextField"));
			GUI.FocusControl("Search");
			GUILayout.EndHorizontal();
			//
			toolbarRect = new Rect(0f, 15f + 20, mWidth, 25f);
			GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
			GUILayout.BeginHorizontal();

			GUILayout.Label("Filter");
			Array strArr = Enum.GetValues(typeof(NodeClassifyType));
			List<string> strList = new List<string>();
			strList.Add("All");
			foreach (object str in strArr)
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
			//

			GUILayout.BeginArea(new Rect(0, 0, windowRect.width, windowRect.height));
			float topSpace = 60;
            mTreeScrollPos = GUI.BeginScrollView(new Rect(0f, topSpace, windowRect.width, windowRect.height - topSpace), mTreeScrollPos,
			                                          new Rect(0f, 0f, windowRect.width - 20f, nodeNameList.Count * 19), false, true);

			foreach (string name in nodeNameList)
			{
				NodeMeta proto = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta(name);
				if (GUILayout.Button(name + $"({proto.describe})", GetButtonStyle()))
				{
					if (SubWinType.CreateNode == mSubWinType)
					{
						GraphDesigner.onCreateNode(name, Vector2.zero);
					}
					else if (SubWinType.ReplaceNode == mSubWinType)
					{
						GraphDesigner.onChangeNodeType(name, Vector2.zero);
					}
					BTEditorWindow.Instance.CloseSubWin();
				}
			}

			GUI.EndScrollView();
			GUILayout.EndArea();

			return "";
		}

		private void ClearNodes()
		{
			mEnumNodeTypeSelection = 0;
			mSearchNode = "";
		}

		public List<string> Filter(List<string> list, string text)
		{
			List<string> result1 = new List<string>();
			string selectType;
			if (mEnumNodeTypeSelection == 0)
			{
				selectType = "All";
				result1 = list;
			}
			else
			{
				selectType = Enum.GetName(typeof(NodeClassifyType), mEnumNodeTypeSelection - 1);
				foreach (string name in list)
				{
					NodeMeta proto = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta(name);
					if (selectType == proto.classify)
					{
						result1.Add(name);
					}
				}
			}

			if (string.IsNullOrEmpty(text))
			{
				return result1;
			}

			List<string> result2 = new List<string>();
			foreach (string name in result1)
			{
				NodeMeta proto = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta(name);
				if (name.ToUpper().Contains(text.ToUpper()) || proto.describe.ToUpper().Contains(text.ToUpper()))
				{
					result2.Add(name);
				}
			}
			return result2;
		}
		
		public GUIStyle GetButtonStyle()
		{
            GUIStyle style = new GUIStyle()
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleLeft
            };
            GUIStyleState onHoverStyleState = new GUIStyleState()
            {
                background = BTDesignerUtility.GetTexture("blue")
            };
            style.hover = onHoverStyleState;

			GUIStyleState onNormalStyleState = new GUIStyleState();
			//onNormalStyleState.textColor = textColor;
			style.normal = onNormalStyleState;
			return style;
		}

		public void Show(Rect rect, SubWinType subWinType)
		{
			windowRect = rect;
			mSubWinType = subWinType;
		}
	}
}