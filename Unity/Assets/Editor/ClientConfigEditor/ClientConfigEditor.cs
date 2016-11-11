using System.IO;
using Base;
using Model;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	public class ClientConfigEditor : EditorWindow
	{
		private const string Path = "./StartConfig.txt";

		private StartConfig config;

		[MenuItem("Tools/客户端配置")]
		private static void ShowWindow()
		{
			GetWindow(typeof(ClientConfigEditor));
		}

		private void OnEnable()
		{
			if (!File.Exists(Path))
			{
				this.config = new StartConfig();
				this.config.AppType = AppType.Client;
				this.config.ServerIP = "*";
				this.config.AddComponent<ClientConfig>();
				return;
			}

			string s = File.ReadAllText(Path);
			this.config = MongoHelper.FromJson<StartConfig>(s);
		}

		private void OnGUI()
		{
			ClientConfig clientConfig = this.config.GetComponent<ClientConfig>();
			clientConfig.Host = EditorGUILayout.TextField("地址: ", clientConfig.Host);
			clientConfig.Port = EditorGUILayout.IntField("端口: ", clientConfig.Port);
			
			if (GUILayout.Button("保存"))
			{
				using (StreamWriter sw = new StreamWriter(new FileStream(Path, FileMode.Create)))
				{
					sw.Write(this.config);
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