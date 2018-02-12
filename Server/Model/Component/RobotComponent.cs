namespace Model
{
	public class RobotComponent: Component
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