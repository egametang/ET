using System;

namespace Model
{
	public abstract class Disposer: Object, IDisposable
	{
		protected Disposer(): base(IdGenerater.GenerateId())
		{
			ObjectEvents.Instance.Add(this);
		}

		protected Disposer(long id): base(id)
		{
			ObjectEvents.Instance.Add(this);
		}

		public virtual void Dispose()
		{
			this.Id = 0;
		}
	}
}