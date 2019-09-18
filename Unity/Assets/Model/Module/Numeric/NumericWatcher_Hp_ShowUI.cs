namespace ETModel
{
	/// <summary>
	/// 监视hp数值变化，改变血条值
	/// </summary>
	[NumericWatcher(NumericType.Hp)]
	public class NumericWatcher_Hp_ShowUI : INumericWatcher
	{
		public void Run(long id, int value)
		{
		}
	}
}
