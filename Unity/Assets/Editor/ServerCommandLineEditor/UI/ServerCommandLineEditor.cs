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
			for (int i = 0; i < this.commandLines.Commands.Count; ++i)
			{
				CommandLine commandLine = this.commandLines.Commands[i];
				GUILayout.BeginHorizontal();
				GUILayout.Label($"IP:");
				commandLine.IP = EditorGUILayout.TextField(commandLine.IP);
				GUILayout.Label($"AppType:");
				commandLine.Options.AppType = EditorGUILayout.TextField(commandLine.Options.AppType);
				GUILayout.Label($"Id:");
				commandLine.Options.Id = EditorGUILayout.IntField(commandLine.Options.Id);
				GUILayout.Label($"Protocol:");
				commandLine.Options.Protocol = (NetworkProtocol)EditorGUILayout.EnumPopup(commandLine.Options.Protocol);
				GUILayout.Label($"Host:");
				commandLine.Options.Host = EditorGUILayout.TextField(commandLine.Options.Host);
				GUILayout.Label($"Port:");
				commandLine.Options.Port = EditorGUILayout.IntField(commandLine.Options.Port);
				if (GUILayout.Button("删除"))
				{
					this.commandLines.Commands.Remove(commandLine);
					break;
				}
				if (GUILayout.Button("复制"))
				{
					for (int j = 1; j < this.copyNum + 1; ++j)
					{
						CommandLine newCommandLine = (CommandLine) commandLine.Clone();
						newCommandLine.Options.Id += j;
						newCommandLine.Options.Port += j;
						newCommandLine.Options.Protocol = this.protocol;
						this.commandLines.Commands.Add(newCommandLine);
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
				CommandLine newCommandLine = new CommandLine();
				newCommandLine.Options.Protocol = this.protocol;
				this.commandLines.Commands.Add(newCommandLine);
			}

			if (GUILayout.Button("保存"))
			{
				File.WriteAllText(Path, MongoHelper.ToJson(this.commandLines));
			}

			if (GUILayout.Button("启动"))
			{
				foreach (CommandLine commandLine in this.commandLines.Commands)
				{
					string arguments = $"--appType={commandLine.Options.AppType} --id={commandLine.Options.Id} --Protocol={commandLine.Options.Protocol} --Host={commandLine.Options.Host} --Port={commandLine.Options.Port}";

					ProcessStartInfo info = new ProcessStartInfo(@"App.exe", arguments)
					{
						UseShellExecute = true,
						WorkingDirectory = @"..\Server\Bin\Debug"
					};
					Process.Start(info);
				}
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