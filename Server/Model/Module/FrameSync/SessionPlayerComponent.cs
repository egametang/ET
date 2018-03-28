namespace ETModel
{
	public class SessionPlayerComponent : Component
	{
		public Player Player;

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();

			Game.Scene.GetComponent<PlayerComponent>()?.Remove(this.Player.Id);
		}
	}
}