namespace ET
{
	// 分发数值监听
	public class NumericChangeEvent_NotifyWatcher: AEvent<EventType.NumbericChange>
	{
		public override async ETTask Run(EventType.NumbericChange args)
		{
			NumericWatcherComponent.Instance.Run(args.NumericType, args.Parent.Id, args.New);
		}
	}
}
