namespace Model
{
	public class SessionPlayerComponent : Component
	{
		public Player Player;

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			Game.Scene.GetComponent<PlayerComponent>()?.Remove(this.Player.Id);
		}
	}
}