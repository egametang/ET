using System;
using Base;

namespace Model
{
	public abstract class Disposer : Object, IDisposable 
	{
		protected Disposer(): base(IdGenerater.GenerateId())
		{
			Game.Disposers.Add(this);
		}

		protected Disposer(long id): base(id)
		{
			Game.Disposers.Add(this);
		}

		public virtual void Dispose()
		{
			this.Id = 0;
			Game.Disposers.Remove(this);
		}

		public override void BeginInit()
		{
		}

		public override void EndInit()
		{
			Game.Disposers.Add(this);
		}
	}
}