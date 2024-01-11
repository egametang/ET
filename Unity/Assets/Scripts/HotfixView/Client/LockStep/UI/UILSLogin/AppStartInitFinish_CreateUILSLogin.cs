namespace ET.Client
{
	[Event(SceneType.LockStep)]
	public class AppStartInitFinish_CreateUILSLogin: AEvent<Scene, AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
            Log.Error($"帧同步Demo 未接入YIUI 请自行接入");
            await UIHelper.Create(root, UIType.UILSLogin, UILayer.Mid);
		}
	}
}
