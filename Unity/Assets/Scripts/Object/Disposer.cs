using System;
using Base;

namespace Model
{
	public abstract class Disposer : Object, IDisposable 
	{
		protected Disposer(): base(IdGenerater.GenerateId())
		{
			Game.DisposerEventManager.Add(this);
		}

		protected Disposer(long id): base(id)
		{
			Game.DisposerEventManager.Add(this);
		}

		public virtual void Dispose()
		{
			Game.DisposerEventManager.Remove(this);
			this.Id = 0;
		}

		public override void BeginInit()
		{
		}

		public override void EndInit()
		{
			Game.DisposerEventManager.Add(this);
		}
	}
}