namespace ETModel
{
	public class RobotComponent: Entity
	{
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}