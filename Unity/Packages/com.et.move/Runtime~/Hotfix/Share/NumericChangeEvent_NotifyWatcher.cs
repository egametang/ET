namespace ET
{
	// 分发数值监听
	[Event(SceneType.All)]  // 服务端Map需要分发, 客户端CurrentScene也要分发
	public class NumericChangeEvent_NotifyWatcher: AEvent<Scene, NumbericChange>
	{
		protected override async ETTask Run(Scene scene, NumbericChange args)
		{
			NumericWatcherComponent.Instance.Run(args.Unit, args);
			await ETTask.CompletedTask;
		}
	}
}
