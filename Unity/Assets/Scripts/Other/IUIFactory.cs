using UnityEngine;

namespace Model
{
	public interface IUIFactory
	{
		UI Create(Scene scene, UIType type, GameObject parent);
		void Remove(UIType type);
	}
}