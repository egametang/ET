namespace ET
{
	// 分发数值监听
	public class NumericChangeEventAsyncNotifyWatcher: AEvent<EventType.NumbericChange>
	{
		protected override void Run(EventType.NumbericChange args)
		{
			NumericWatcherComponent.Instance.Run(args);
		}
	}
}
