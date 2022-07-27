namespace ET
{
	// 分发数值监听
	[Event(SceneType.None)]  // 服务端Map需要分发, 客户端CurrentScene也要分发
	public class NumericChangeEvent_NotifyWatcher: AEvent<EventType.NumbericChange>
	{
		protected override async ETTask Run(Scene scene, EventType.NumbericChange args)
		{
			NumericWatcherComponent.Instance.Run(args.Unit, args);
			await ETTask.CompletedTask;
		}
	}
}
