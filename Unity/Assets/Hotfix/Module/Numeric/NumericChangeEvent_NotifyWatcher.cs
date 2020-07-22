namespace ET
{
	// 分发数值监听
	[Event]
	public class NumericChangeEvent_NotifyWatcher: AEvent<EventType.NumbericChange>
	{
		public override void Run(EventType.NumbericChange args)
		{
			Game.Scene.GetComponent<NumericWatcherComponent>().Run(args.NumericType, args.Parent.Id, args.New);
		}
	}
}
