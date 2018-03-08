using UnityEngine;

namespace ETModel
{
	public interface IUIFactory
	{
		UI Create(Scene scene, int type, GameObject parent);
		void Remove(int type);
	}
}