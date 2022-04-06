namespace ET
{
	// 分发数值监听
	public class NumericChangeEventAsyncNotifyWatcher: AEventClass<EventType.NumbericChange>
	{
		protected override void Run(object args)
		{
			EventType.NumbericChange numbericChange = args as EventType.NumbericChange;
			NumericWatcherComponent.Instance.Run(numbericChange);
		}
	}
}
