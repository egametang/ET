namespace ET.Client
{
	[Event(SceneType.Demo)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<Scene, AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
			await YIUIMgrComponent.Inst.OpenPanelAsync<LoginPanelComponent>();
			//await UIHelper.Create(root, UIType.UILogin, UILayer.Mid);
		}
	}
}
