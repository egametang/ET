using System.Diagnostics;

using UnityEditor;

namespace ET
{
	internal class OpcodeInfo
	{
		public string Name;
		public int Opcode;
	}

	public class Proto2CSEditor: EditorWindow
	{
		[MenuItem("Tools/Proto2CS")]
		public static void AllProto2CS()
		{
			Process process = ProcessHelper.Run("dotnet", "Proto2CS.dll", "../Proto/", false);
			Log.Info(process.StandardOutput.ReadToEnd());
			AssetDatabase.Refresh();
		}
	}
}
