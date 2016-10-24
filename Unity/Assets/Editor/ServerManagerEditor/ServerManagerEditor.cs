using System;
using System.Collections.Generic;
using System.Diagnostics;
using Base;
using Model;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	public class ServerManagerEditor : EditorWindow
	{
		private string managerAddress;

		private bool isAll;

		private string[] appTypes = { "Manager", "Realm", "Gate" };
		private bool[] isCheck = { false, false, false };

		[MenuItem("Tools/服务器管理")]
		private static void ShowWindow()
		{
			GetWindow(typeof(ServerManagerEditor));
		}

		private void OnEnable()
		{
		}

		private void OnGUI()
		{
			if (!Application.isPlaying)
			{
				GUILayout.Label("请启动游戏!");
				return;
			}

			List<string> selected = new List<string>();
			this.isAll = GUILayout.Toggle(this.isAll, "All");
			if (this.isAll)
			{
				for (int i = 0; i < this.isCheck.Length; ++i)
				{
					this.isCheck[i] = true;
				}
			}

			for (int i = 0; i < this.appTypes.Length; ++i)
			{
				this.isCheck[i] = GUILayout.Toggle(this.isCheck[i], this.appTypes[i]);
				if (!this.isCheck[i])
				{
					this.isAll = false;
				}
			}
			
			this.managerAddress = EditorGUILayout.TextField("Manager Address: ", this.managerAddress);

			if (GUILayout.Button("Reload"))
			{
				for(int i = 0; i < this.isCheck.Length; ++i)
				{
					if (this.isCheck[i])
					{
						selected.Add(this.appTypes[i]);
					}
				}
				NetworkComponent networkComponent = Game.Scene.GetComponent<NetOuterComponent>();
				Entity session = networkComponent.Get($"{this.managerAddress}");
				try
				{
					session.GetComponent<MessageComponent>().Call<C2M_Reload, M2C_Reload>(new C2M_Reload { AppType = selected });
				}
				catch (RpcException e)
				{
					Log.Error(e.ToString());
				}
				Log.Info("Reload OK!");
			}
		}
	}
}
