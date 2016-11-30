namespace Model
{
	[EntityEvent(typeof(RobotComponent))]
	public class RobotComponent : Component
    {
		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
    }
}