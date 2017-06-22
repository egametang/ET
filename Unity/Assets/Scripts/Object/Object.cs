using System;

namespace Model
{
	public abstract class Object: IDisposable
	{
		public long Id { get; protected set; }

		protected Object()
		{
			Id = IdGenerater.GenerateId();
		}

		protected Object(long id)
		{
			this.Id = id;
		}

		public virtual void Dispose()
		{
		}
	}
}