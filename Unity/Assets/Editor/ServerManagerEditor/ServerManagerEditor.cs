using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ETModel;
using MongoDB.Bson;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
	public class ServerManagerEditor: EditorWindow
	{
		private string managerAddress;
		private string account;
		private string password;

		[MenuItem("Tools/服务器管理工具")]
		private static void ShowWindow()
		{
			GetWindow(typeof (ServerManagerEditor));
		}

		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			
			GUILayout.Label("Manager外网地址:");
			managerAddress = EditorGUILayout.TextField(this.managerAddress);
			
			GUILayout.Label("帐号:");
			this.account = GUILayout.TextField(this.account);
			
			GUILayout.Label("密码:");
			this.password = GUILayout.TextField(this.password);
			
			if (GUILayout.Button("Reload"))
			{
				if (!Application.isPlaying)
				{
					Log.Error($"Reload必须先启动客户端!");
					return;
				}

				ReloadAsync(this.managerAddress, this.account, this.password).NoAwait();
			}
			
			GUILayout.EndHorizontal();
		}
		
		private static async ETVoid ReloadAsync(string address, string account, string password)
		{
			using (Session session = Game.Scene.GetComponent<NetOuterComponent>().Create(address))
			{
				try
				{
					await session.Call(new C2M_Reload() {Account = account, Password = password});	
					Log.Info($"Reload服务端成功!");
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
	}
}