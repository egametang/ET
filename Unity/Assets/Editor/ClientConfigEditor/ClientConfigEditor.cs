using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Base;
using Model;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	public class ClientConfigEditor : EditorWindow
	{
		private const string Path = "./ClientConfig.txt";

		private ClientConfig config;

		[MenuItem("Tools/客户端配置")]
		private static void ShowWindow()
		{
			GetWindow(typeof(ClientConfigEditor));
		}

		private void OnEnable()
		{
			if (!File.Exists(Path))
			{
				this.config = new ClientConfig();
				return;
			}

			string s = File.ReadAllText(Path);
			this.config = MongoHelper.FromJson<ClientConfig>(s);
		}

		private void OnGUI()
		{
			this.config.Host = EditorGUILayout.TextField("地址: ", this.config.Host);
			this.config.Port = EditorGUILayout.IntField("端口: ", this.config.Port);

			GUILayout.BeginHorizontal();

			if (GUILayout.Button("保存"))
			{
				using (StreamWriter sw = new StreamWriter(new FileStream(Path, FileMode.Create)))
				{
					sw.Write(MongoHelper.ToJson(this.config));
				}
			}
		}

		private void OnDisable()
		{
		}

		private void OnDestroy()
		{
			this.config.Dispose();
		}
	}
}