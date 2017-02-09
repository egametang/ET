using UnityEditor;

public static class UnitySchedulerEditor
{
	[InitializeOnLoadMethod]
	private static void InitializeInEditor()
	{
		UnityScheduler.InitializeInEditor();
		EditorApplication.update += UnityScheduler.ProcessEditorUpdate;
	}
}
