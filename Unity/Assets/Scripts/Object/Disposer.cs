using System;
using Base;

namespace Model
{
	public abstract class Disposer : Object, IDisposable 
	{
		protected Disposer(): base(IdGenerater.GenerateId())
		{
			DisposerManager.Instance.Add(this);
		}

		protected Disposer(long id): base(id)
		{
			DisposerManager.Instance.Add(this);
		}

		public virtual void Dispose()
		{
			DisposerManager.Instance.Remove(this);
			this.Id = 0;
		}
	}
}