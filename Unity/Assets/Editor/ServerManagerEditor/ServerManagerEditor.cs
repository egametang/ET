using System.Collections.Generic;
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

		private readonly List<AppType> serverTypes = AppTypeHelper.GetServerTypes();
		private bool[] isCheck;

		[MenuItem("Tools/服务器管理")]
		private static void ShowWindow()
		{
			GetWindow(typeof(ServerManagerEditor));
		}

		private void OnEnable()
		{
			this.isCheck = new bool[this.serverTypes.Count];
		}

		private void OnGUI()
		{
			if (!Application.isPlaying)
			{
				GUILayout.Label("请启动游戏!");
				return;
			}


			AppType reloadType = AppType.None;
			this.isAll = GUILayout.Toggle(this.isAll, "All");
			if (this.isAll)
			{
				for (int i = 0; i < this.isCheck.Length; ++i)
				{
					this.isCheck[i] = true;
				}
			}

			for (int i = 0; i < this.serverTypes.Count; ++i)
			{
				this.isCheck[i] = GUILayout.Toggle(this.isCheck[i], this.serverTypes[i].ToString());
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
						reloadType = reloadType | this.serverTypes[i];
					}
				}
				NetOuterComponent networkComponent = Game.Scene.GetComponent<NetOuterComponent>();
				using (Session session = networkComponent.Create($"{this.managerAddress}"))
				{
					try
					{
						session.Call<C2M_Reload, M2C_Reload>(new C2M_Reload { AppType = reloadType });
					}
					catch (RpcException e)
					{
						Log.Error(e.ToString());
					}
				}

				Log.Info("Reload OK!");
			}
		}
	}
}
