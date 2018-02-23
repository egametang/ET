using Model;
using UnityEngine;

namespace Hotfix
{
	public interface IUIFactory
	{
		UI Create(Scene scene, UIType type, GameObject parent);
		void Remove(UIType type);
	}
}