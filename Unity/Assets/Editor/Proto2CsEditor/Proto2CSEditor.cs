using System;
using System.Diagnostics;
using ETModel;
using UnityEditor;

namespace ETEditor
{
	internal class OpcodeInfo
	{
		public string Name;
		public int Opcode;
	}
	
	public class Proto2CSEditor : EditorWindow
	{
		[MenuItem("Tools/Proto2CS")]
		public static void AllProto2CS()
		{
			CommandRun();
			AssetDatabase.Refresh();
		}

		public static void CommandRun()
		{
			try
			{
				ProcessStartInfo info = new ProcessStartInfo
				{
					CreateNoWindow = true,
					FileName = "dotnet", 
					Arguments = "Proto2CS.dll", 
					UseShellExecute = false,
					WorkingDirectory = "../Proto/",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				};
				Process process = Process.Start(info);
				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					throw new Exception(process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd());
				}
				Log.Info(process.StandardOutput.ReadToEnd());
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
