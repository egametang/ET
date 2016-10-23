using System;
using System.Diagnostics;
using Base;
using UnityEditor;

namespace MyEditor
{
	public class RsyncEditor : EditorWindow
	{
		[MenuItem("Tools/同步到Linux")]
		private static void ShowUnDisposeObjects()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = @"Tools\cwRsync\rsync.exe";
			startInfo.Arguments = "-vzrtopg --password-file=./Tools/cwRsync/rsync.secrets --exclude-from=./Tools/cwRsync/exclude.txt --delete ./ tanghai@192.168.1.134::Tanghai/Source/Egametang --chmod=ugo=rwX";
			startInfo.UseShellExecute = true;
			startInfo.WorkingDirectory = @"..\";
			Process p = Process.Start(startInfo);
			p.WaitForExit();
			Log.Info("同步完成!");
		}
	}
}
