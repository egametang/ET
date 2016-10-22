using System;
using System.Diagnostics;
using UnityEditor;

namespace MyEditor
{
	public class RsyncEditor : EditorWindow
	{
		[MenuItem("Tools/Rsync")]
		private static void ShowUnDisposeObjects()
		{
			Process.Start("../Tools/cwRsync/rsync.exe", "");
		}
	}
}