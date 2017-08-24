namespace Model
{
	public class SessionGamerComponent : Component
	{
		public Gamer Gamer;

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			Game.Scene.GetComponent<GamerComponent>().Remove(this.Gamer.Id);
		}
	}
}