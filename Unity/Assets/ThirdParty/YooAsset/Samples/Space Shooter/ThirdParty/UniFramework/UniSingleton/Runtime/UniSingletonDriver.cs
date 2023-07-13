using UnityEngine;

namespace UniFramework.Singleton
{
	internal class UniSingletonDriver : MonoBehaviour
	{
		void Update()
		{
			UniSingleton.Update();
		}
	}
}