namespace ET.Client
{
	[Event(SceneType.Demo)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<Scene, AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
			await UIHelper.Create(root, UIType.UILogin, UILayer.Mid);

			//创建电脑实体，给1挂载组件，2不动
			var computer1 = root.GetComponent<ComputersComponent>().AddChild<Computer>();
			var computer2 = root.GetComponent<ComputersComponent>().AddChild<Computer>();

			//添加机箱组件
			computer1.AddComponent<PCCaseComponent>();
			//添加显示器组件，并指定亮度
			computer1.AddComponent<MonitorComponent,int>(30);
		}
	}
}
