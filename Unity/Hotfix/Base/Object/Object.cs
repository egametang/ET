namespace ETHotfix
{
	public interface IDisposable2
	{
		void Dispose();
	}

	public interface ISupportInitialize2
	{
		void BeginInit();
		void EndInit();
	}

	public abstract class Object: ISupportInitialize2
	{
		public virtual void BeginInit()
		{
		}

		public virtual void EndInit()
		{
		}

		public override string ToString()
		{
			return JsonHelper.ToJson(this);
		}
	}
}