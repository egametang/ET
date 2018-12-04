namespace ETModel
{
	public class NumericWatcherAttribute : BaseAttribute
	{
		public NumericType NumericType { get; }

		public NumericWatcherAttribute(NumericType type)
		{
			this.NumericType = type;
		}
	}
}