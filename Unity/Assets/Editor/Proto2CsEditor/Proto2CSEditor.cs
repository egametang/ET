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

	public class Proto2CSEditor: EditorWindow
	{
		[MenuItem("Tools/Proto2CS")]
		public static void AllProto2CS()
		{
			Process process = ProcessHelper.Run("dotnet", "Proto2CS.dll", "../Proto/", true);
			Log.Info(process.StandardOutput.ReadToEnd());
			AssetDatabase.Refresh();
		}
	}
}
