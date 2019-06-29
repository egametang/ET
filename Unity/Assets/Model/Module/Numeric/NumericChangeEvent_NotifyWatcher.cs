namespace ETModel
{
	// 分发数值监听
	[Event(EventIdType.NumbericChange)]
	public class NumericChangeEvent_NotifyWatcher: AEvent<long, NumericType, int>
	{
		public override void Run(long id, NumericType numericType, int value)
		{
			Game.Scene.GetComponent<NumericWatcherComponent>().Run(numericType, id, value);
		}
	}
}
