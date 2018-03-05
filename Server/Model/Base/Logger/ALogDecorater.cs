namespace ETModel
{
	public abstract class ALogDecorater
	{
		protected const string SEP = " ";
		private int level;
		protected readonly ALogDecorater decorater;

		protected ALogDecorater(ALogDecorater decorater = null)
		{
			this.decorater = decorater;
			this.Level = 0;
		}

		protected int Level
		{
			get
			{
				return this.level;
			}
			set
			{
				this.level = value;
				if (this.decorater != null)
				{
					this.decorater.Level = value + 1;
				}
			}
		}

		public virtual string Decorate(string message)
		{
			if (this.decorater == null)
			{
				return message;
			}
			return this.decorater.Decorate(message);
		}
	}
}