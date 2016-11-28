using System;
using Base;

namespace Model
{
	public abstract class Disposer : Object, IDisposable 
	{
		protected Disposer(): base(IdGenerater.GenerateId())
		{
		}

		protected Disposer(long id): base(id)
		{
		}

		public virtual void Dispose()
		{
			this.Id = 0;
		}

		public override void BeginInit()
		{
		}

		public override void EndInit()
		{
			Game.ComponentEventManager.Add(this);
		}
	}
}