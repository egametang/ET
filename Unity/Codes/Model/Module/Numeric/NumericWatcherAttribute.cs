namespace ET
{
	public class NumericWatcherAttribute : BaseAttribute
	{
		public int NumericType { get; }

		public NumericWatcherAttribute(int type)
		{
			this.NumericType = type;
		}
	}
}