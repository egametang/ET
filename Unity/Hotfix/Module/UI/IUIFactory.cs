using UnityEngine;

namespace ETHotfix
{
	public interface IUIFactory
	{
		UI Create(Scene scene, int type, GameObject parent);
		void Remove(int type);
	}
}