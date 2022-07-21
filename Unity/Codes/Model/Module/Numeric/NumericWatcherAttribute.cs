namespace ET
{
	public class NumericWatcherAttribute : BaseAttribute
	{
		public int NumericType { get; }

		public NumericWatcherAttribute(NumericType type)
		{
			this.NumericType = (int)type;
		}
	}
}