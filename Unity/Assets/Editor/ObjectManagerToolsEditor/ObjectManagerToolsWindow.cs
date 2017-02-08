using Base;
using Model;
using UnityEditor;

public class ObjectManagerToolsWindow: EditorWindow
{
	[MenuItem("Tools/ObjectManagerTools/显示未Dispose的对象")]
	private static void ShowUnDisposeObjects()
	{
		Log.Info(Game.DisposerInfo());
	}

	[MenuItem("Tools/ObjectManagerTools/清除所有对象")]
	private static void ClearAllObjects()
	{
		Game.ClearDisposers();
	}
}