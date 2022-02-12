namespace ET
{
	// 分发数值监听
	public class NumericChangeEvent_NotifyWatcher: AEvent<EventType.NumbericChange>
	{
		protected override async ETTask Run(EventType.NumbericChange arg)
		{
			NumericWatcherComponent.Instance.Run(arg.NumericType, arg.Parent.Id, arg.New);
			await ETTask.CompletedTask;
		}
	}
}
