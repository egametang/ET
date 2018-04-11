namespace ETHotfix
{
	public interface IDisposable
	{
		void Dispose();
	}

	public interface ISupportInitialize
	{
		void BeginInit();
		void EndInit();
	}

	public abstract class Object: ISupportInitialize
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