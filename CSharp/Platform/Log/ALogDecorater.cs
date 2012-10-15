
namespace Log
{
	public abstract class ALogDecorater
	{
		private int level;

		protected ALogDecorater decorater;

		public int Level
		{
			get
			{
				return this.level;
			}
			set
			{
				if (decorater != null)
				{
					decorater.Level = value + 1;
				}
				this.level = value;
			}
		}
		public abstract string Decorate(string message);
	}
}
