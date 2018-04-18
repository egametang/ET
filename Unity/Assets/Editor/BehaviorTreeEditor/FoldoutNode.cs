using System.Collections.Generic;
using UnityEngine;

namespace ETEditor
{
	public class FoldoutNode //折叠的节点
	{
		public string folderName { get; set; }

		public delegate void DelegateMethod(FoldoutNode self);

		private readonly DelegateMethod mCallback;

		public string Text { get; }

		public bool Hide { get; set; } = false;

		public bool Select { get; set; } = false;

		public int Depth { get; set; }

		public FoldoutNode(string text, DelegateMethod callback = null)
		{
			this.Text = text;
			mCallback = callback;
		}

		public void Draw()
		{
			if (!this.Hide)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(15 * this.Depth);

				string str = "";
				if (this.Select)
				{
					str = str + "[" + this.Text + "]";
				}
				else
				{
					str = str + this.Text;
				}
				if (GUILayout.Button(str, GUI.skin.GetStyle("Label")))
				{
					mCallback.Invoke(this);
				}
				GUILayout.EndHorizontal();
			}
		}
	}

	public class FoldoutFolder //折叠的目录
	{
		public delegate void DelegateMethod(FoldoutFolder self);

		private readonly DelegateMethod mCallback;

		public string Text { get; }

		public bool Hide { get; set; }

		public bool Fold { get; set; }

		public int Depth { get; set; }

		public bool Select { get; set; } = false;

		public List<FoldoutFolder> Folders { get; } = new List<FoldoutFolder>();

		public List<FoldoutNode> Nodes { get; } = new List<FoldoutNode>();

		public FoldoutFolder(string text, DelegateMethod callback = null)
		{
			this.Text = text;
			mCallback = callback;
		}

		//添加折叠
		public FoldoutFolder AddFolder(string text, DelegateMethod callback)
		{
			FoldoutFolder folder = new FoldoutFolder(text, callback);
			this.Folders.Add(folder);
			folder.Depth = this.Depth + 1;
			return folder;
		}

		//添加叶子节点
		public FoldoutNode AddNode(string folderName, string text, FoldoutNode.DelegateMethod callback)
		{
			FoldoutNode node = new FoldoutNode(text, callback);
			node.folderName = folderName;
			this.Nodes.Add(node);
			node.Depth = this.Depth + 1;
			return node;
		}

		public FoldoutNode FindNode(string text)
		{
			foreach (FoldoutNode node in this.Nodes)
			{
				if (node.Text == text)
				{
					return node;
				}
			}
			return null;
		}

		public FoldoutFolder FindFolder(string text)
		{
			foreach (FoldoutFolder folder in this.Folders)
			{
				if (folder.Text == text)
				{
					return folder;
				}
			}
			return null;
		}

		public void Draw()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(15 * this.Depth);

			string str = "+";
			if (this.Fold)
			{
				str = " -";
			}
			if (this.Select)
			{
				str = str + "[" + this.Text + "]";
			}
			else
			{
				str = str + this.Text;
			}
			if (GUILayout.Button(str, GUI.skin.GetStyle("Label")))
			{
				this.Fold = !this.Fold;
				mCallback.Invoke(this);
			}
			GUILayout.EndHorizontal();

			if (this.Fold)
			{
				foreach (FoldoutFolder folder in this.Folders)
				{
					folder.Draw();
				}
				foreach (FoldoutNode node in this.Nodes)
				{
					node.Draw();
				}
			}
		}
	}
}