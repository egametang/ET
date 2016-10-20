using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Base;
using Model;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
	public class ServerCommandLineEditor : EditorWindow
	{
		private const string Path = @"..\Server\Bin\Debug\CommandLineConfig.txt";

		private int copyNum = 1;

		private NetworkProtocol protocol;

		private CommandLines commandLines;

		[MenuItem("Tools/服务端命令行配置")]
		static void ShowWindow()
		{
			GetWindow(typeof(ServerCommandLineEditor));
		}

		void OnEnable()
		{
			if (!File.Exists(Path))
			{
				this.commandLines = new CommandLines();
				return;
			}
			string s = File.ReadAllText(Path);
			this.commandLines = MongoHelper.FromJson<CommandLines>(s);
		}

		void OnGUI()
		{
			for (int i = 0; i < this.commandLines.Options.Count; ++i)
			{
				Options options = this.commandLines.Options[i];
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Id:");
				options.Id = EditorGUILayout.IntField(options.Id);
				GUILayout.Label($"服务器IP:");
				options.IP = EditorGUILayout.TextField(options.IP);
				GUILayout.Label($"AppType:");
				options.AppType = EditorGUILayout.TextField(options.AppType);
				GUILayout.Label($"Protocol:");
				options.Protocol = (NetworkProtocol)EditorGUILayout.EnumPopup(options.Protocol);
				GUILayout.Label($"Host:");
				options.Host = EditorGUILayout.TextField(options.Host);
				GUILayout.Label($"Port:");
				options.Port = EditorGUILayout.IntField(options.Port);
				if (GUILayout.Button("删除"))
				{
					this.commandLines.Options.Remove(options);
					break;
				}
				if (GUILayout.Button("复制"))
				{
					for (int j = 1; j < this.copyNum + 1; ++j)
					{
						Options newOptions = (Options)options.Clone();
						newOptions.Id += j;
						newOptions.Port += j;
						newOptions.Protocol = this.protocol;
						this.commandLines.Options.Add(newOptions);
					}
					break;
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			this.copyNum = EditorGUILayout.IntField("复制数量: ", this.copyNum);
			this.protocol = (NetworkProtocol)EditorGUILayout.EnumPopup("协议: ", this.protocol);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("添加"))
			{
				Options newOptions = new Options();
				newOptions.Protocol = this.protocol;
				this.commandLines.Options.Add(newOptions);
			}

			if (GUILayout.Button("保存"))
			{
				File.WriteAllText(Path, MongoHelper.ToJson(this.commandLines));
			}

			if (GUILayout.Button("启动"))
			{
				Options options = this.commandLines.Manager;
				
				string arguments = $"--appType={options.AppType} --id={options.Id} --Protocol={options.Protocol} --Host={options.Host} --Port={options.Port}";

				ProcessStartInfo info = new ProcessStartInfo(@"App.exe", arguments)
				{
					UseShellExecute = true,
					WorkingDirectory = @"..\Server\Bin\Debug"
				};
				Process.Start(info);
			}
			GUILayout.EndHorizontal();
		}

		void OnDisable()
		{
		}

		void OnDestroy()
		{
		}
	}
}