namespace ET
{
	/// <summary>
	/// 监视hp数值变化，改变血条值
	/// </summary>
	[NumericWatcher(NumericType.HP)]
	public class NumericWatcher_Hp_ShowUI : INumericWatcher
	{
		public void Run(EventType.NumbericChange args)
		{
		}
	}
}
