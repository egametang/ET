using System;
using Base;

namespace Model
{
	public abstract class Disposer : Object, IDisposable 
	{
		protected Disposer(): base(IdGenerater.GenerateId())
		{
			Game.Add(this);
		}

		protected Disposer(long id): base(id)
		{
			Game.Add(this);
		}

		public virtual void Dispose()
		{
			Game.Remove(this);
			this.Id = 0;
		}

		public override void BeginInit()
		{
		}

		public override void EndInit()
		{
			Game.Add(this);
		}
	}
}