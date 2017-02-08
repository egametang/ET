using Model;
using UnityEditor;

namespace MyEditor
{
	[InitializeOnLoad]
	internal class EditorInit
	{
		static EditorInit()
		{
			Game.EntityEventManager.Register("Model", typeof (Game).Assembly);
			Game.EntityEventManager.Register("Editor", typeof (EditorInit).Assembly);
		}
	}
}