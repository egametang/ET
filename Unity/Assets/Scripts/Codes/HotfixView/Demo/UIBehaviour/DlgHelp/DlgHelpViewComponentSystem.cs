
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	[ObjectSystem]
	public class DlgHelpViewComponentAwakeSystem : AwakeSystem<DlgHelpViewComponent> 
	{
		protected override void Awake(DlgHelpViewComponent self)
		{
			self.uiTransform = self.GetParent<UIBaseWindow>().uiTransform;
		}
	}


	[ObjectSystem]
	public class DlgHelpViewComponentDestroySystem : DestroySystem<DlgHelpViewComponent> 
	{
		protected override void Destroy(DlgHelpViewComponent self)
		{
			self.DestroyWidget();
		}
	}
}
