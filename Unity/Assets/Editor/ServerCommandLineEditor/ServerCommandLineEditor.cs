using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Base;
using Model;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	public class ServerCommandLineEditor : EditorWindow
	{
		private const string ConfigDir = @"..\Config\StartConfig\";

		private List<string> files;

		private int selectedIndex;

		private string fileName;

		private string addFileName;

		private int copyNum = 1;

		private AppType AppType = AppType.Manager;

		private readonly List<StartConfig> startConfigs = new List<StartConfig>();

		[MenuItem("Tools/命令行配置")]
		private static void ShowWindow()
		{
			GetWindow(typeof(ServerCommandLineEditor));
		}

		private void OnEnable()
		{
			this.files = this.GetConfigFiles();
			if (this.files.Count > 0)
			{
				this.fileName = this.files[this.selectedIndex];
				this.LoadConfig();
			}
		}

		private List<string> GetConfigFiles()
		{
			List<string> fs = Directory.GetFiles(ConfigDir).ToList();
			DirectoryInfo directoryInfo = new DirectoryInfo(ConfigDir);
			FileInfo[] fileInfo = directoryInfo.GetFiles();
			fs = fileInfo.Select(x => x.Name).ToList();
			return fs;
		}

		private void LoadConfig()
		{
			string filePath = this.GetFilePath();
			if (!File.Exists(filePath))
			{
				return;
			}

			string s2 = "";
			try
			{
				this.startConfigs.Clear();
				string[] ss = File.ReadAllText(filePath).Split('\n');
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

		private string GetFilePath()
		{
			return Path.Combine(ConfigDir, this.fileName);
		}

		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			string[] filesArray = this.files.ToArray();
			this.selectedIndex = EditorGUILayout.Popup(this.selectedIndex, filesArray);

			string lastFile = this.fileName;
			this.fileName = this.files[this.selectedIndex];

			if (this.fileName != lastFile)
			{
				this.LoadConfig();
			}
			
			this.addFileName = EditorGUILayout.TextField("文件名", this.addFileName);

			if (GUILayout.Button("添加配置文件"))
			{
				this.fileName = this.addFileName;
				this.addFileName = "";
				File.WriteAllText(this.GetFilePath(), "");
				this.files = this.GetConfigFiles();
				this.selectedIndex = this.files.IndexOf(this.fileName);
			}
			GUILayout.EndHorizontal();

			GUILayout.Label("配置内容:");
			for (int i = 0; i < this.startConfigs.Count; ++i)
			{
				StartConfig startConfig = this.startConfigs[i];
				GUILayout.BeginHorizontal();
				GUILayout.Label($"AppId:");
				startConfig.AppId = EditorGUILayout.IntField(startConfig.AppId);
				GUILayout.Label($"服务器IP:");
				startConfig.ServerIP = EditorGUILayout.TextField(startConfig.ServerIP);
				GUILayout.Label($"AppType:");
				startConfig.AppType = (AppType)EditorGUILayout.EnumPopup(startConfig.AppType);

				InnerConfig innerConfig = startConfig.GetComponent<InnerConfig>();
				if (innerConfig != null)
				{
					GUILayout.Label($"Host:");
					innerConfig.Host = EditorGUILayout.TextField(innerConfig.Host);
					GUILayout.Label($"Port:");
					innerConfig.Port = EditorGUILayout.IntField(innerConfig.Port);
				}

				OuterConfig outerConfig = startConfig.GetComponent<OuterConfig>();
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
						newStartConfig.AppId += j;
						this.startConfigs.Add(newStartConfig);
					}
					break;
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.Label("");

			GUILayout.BeginHorizontal();
			this.copyNum = EditorGUILayout.IntField("复制数量: ", this.copyNum);

			GUILayout.Label($"添加的AppType:");
			this.AppType = (AppType)EditorGUILayout.EnumPopup(this.AppType);

			if (GUILayout.Button("添加一行配置"))
			{
				StartConfig newStartConfig = new StartConfig();

				newStartConfig.AppType = this.AppType;
				newStartConfig.AddComponent<InnerConfig>();

				if (this.AppType.Is(AppType.Gate) || this.AppType.Is(AppType.Realm) || this.AppType.Is(AppType.Manager))
				{
					newStartConfig.AddComponent<OuterConfig>();
				}

				this.startConfigs.Add(newStartConfig);
			}
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();

			if (GUILayout.Button("保存"))
			{
				string path = this.GetFilePath();
				using (StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.Create)))
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
					if (config.AppType.Is(AppType.Manager))
					{
						startConfig = config;
					}
				}

				if (startConfig == null)
				{
					Log.Error("没有配置Manager!");
					return;
				}

				string arguments = $"--appId={startConfig.AppId} --appType={startConfig.AppType} --config=../Config/StartConfig/{this.fileName}";

				ProcessStartInfo info = new ProcessStartInfo(@"App.exe", arguments)
				{
					UseShellExecute = true,
					WorkingDirectory = @"..\Bin\"
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