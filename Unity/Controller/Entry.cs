using Base;

namespace Controller
{
	[Message]
    public class Entry
    {
		public static void Init()
		{
			UnityEngine.Debug.Log("aaaaaaaaaaaaaaaaa" + typeof(Init).Name);
		}
    }
}
