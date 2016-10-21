using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Base;
using Model;
using MongoDB.Bson.Serialization;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	public class ServerCommandLineEditor : EditorWindow
	{
		private const string Path = @"..\Server\App\Start.txt";

		private int copyNum = 1;

		private string AppType = Model.AppType.Realm;

		private List<StartConfig> startConfigs = new List<StartConfig>();

		[MenuItem("Tools/服务端命令行配置")]
		private static void ShowWindow()
		{
			BsonClassMapRegister.Register();
			GetWindow(typeof(ServerCommandLineEditor));
		}

		private void OnEnable()
		{
			if (!File.Exists(Path))
			{
				return;
			}

			string s2 = "";
			try
			{
				string[] ss = File.ReadAllText(Path).Split('\n');
				foreach (string s in ss)
				{
					s2 = s.Trim();
					if (s2 == "")
					{
						continue;
					}

					StartConfig startConfig = MongoHelper.FromJson<StartConfig>(s2);
					this.startConfigs.Add(startConfig);
				}
			}
			catch (Exception e)
			{
				Log.Error($"加载配置失败! {s2} \n {e}");
			}
		}

		private void OnGUI()
		{
			for (int i = 0; i < this.startConfigs.Count; ++i)
			{
				StartConfig startConfig = this.startConfigs[i];
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Id:");
				startConfig.Options.Id = EditorGUILayout.IntField(startConfig.Options.Id);
				GUILayout.Label($"服务器IP:");
				startConfig.IP = EditorGUILayout.TextField(startConfig.IP);
				GUILayout.Label($"AppType:");
				startConfig.Options.AppType = EditorGUILayout.TextField(startConfig.Options.AppType);

				InnerConfig innerConfig = startConfig.Config.GetComponent<InnerConfig>();
				if (innerConfig != null)
				{
					GUILayout.Label($"Host:");
					innerConfig.Host = EditorGUILayout.TextField(innerConfig.Host);
					GUILayout.Label($"Port:");
					innerConfig.Port = EditorGUILayout.IntField(innerConfig.Port);
				}

				OuterConfig outerConfig = startConfig.Config.GetComponent<OuterConfig>();
				if (outerConfig != null)
				{
					GUILayout.Label($"OuterHost:");
					outerConfig.Host = EditorGUILayout.TextField(outerConfig.Host);
					GUILayout.Label($"OuterHost:");
					outerConfig.Port = EditorGUILayout.IntField(outerConfig.Port);
				}


				if (GUILayout.Button("删除"))
				{
					this.startConfigs.Remove(startConfig);
					break;
				}
				if (GUILayout.Button("复制"))
				{
					for (int j = 1; j < this.copyNum + 1; ++j)
					{
						StartConfig newStartConfig = (StartConfig)startConfig.Clone();
						newStartConfig.Options.Id += j;
						this.startConfigs.Add(newStartConfig);
					}
					break;
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			this.copyNum = EditorGUILayout.IntField("复制数量: ", this.copyNum);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.Label($"添加的AppType:");
			this.AppType = EditorGUILayout.TextField(this.AppType);

			if (GUILayout.Button("添加"))
			{
				StartConfig newStartConfig = new StartConfig();

				newStartConfig.Options.AppType = this.AppType;
				newStartConfig.Config.AddComponent<InnerConfig>();

				if (this.AppType == Model.AppType.Gate || this.AppType == Model.AppType.Realm || this.AppType == Model.AppType.Manager)
				{
					newStartConfig.Config.AddComponent<OuterConfig>();
				}

				this.startConfigs.Add(newStartConfig);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			if (GUILayout.Button("保存"))
			{
				using (StreamWriter sw = new StreamWriter(new FileStream(Path, FileMode.Create)))
				{
					foreach (StartConfig startConfig in this.startConfigs)
					{
						sw.Write(MongoHelper.ToJson(startConfig));
						sw.Write('\n');
					}
				}
			}

			if (GUILayout.Button("启动"))
			{
				StartConfig startConfig = null;
				foreach (StartConfig config in this.startConfigs)
				{
					if (config.Options.AppType == Model.AppType.Manager)
					{
						startConfig = config;
					}
				}

				if (startConfig == null)
				{
					Log.Error("没有配置Manager!");
					return;
				}
				
				string arguments = $"--id={startConfig.Options.Id} --appType={startConfig.Options.AppType}";

				ProcessStartInfo info = new ProcessStartInfo(@"App.exe", arguments)
				{
					UseShellExecute = true,
					WorkingDirectory = @"..\Server\Bin\Debug"
				};
				Process.Start(info);
			}
			GUILayout.EndHorizontal();
		}

		private void OnDisable()
		{
		}

		private void OnDestroy()
		{
		}
	}
}