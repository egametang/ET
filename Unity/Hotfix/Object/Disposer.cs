using Model;

namespace Hotfix
{
	public abstract class Disposer: Object
	{
		protected Disposer(): base(IdGenerater.GenerateId())
		{
			Game.EntityEventManager.Add(this);
		}

		protected Disposer(long id): base(id)
		{
			Game.EntityEventManager.Add(this);
		}

		public override void Dispose()
		{
			this.Id = 0;
			Game.EntityEventManager.Remove(this);
		}
	}
}