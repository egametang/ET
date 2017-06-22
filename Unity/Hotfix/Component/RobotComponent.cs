namespace Hotfix
{
	[EntityEvent(EntityEventId.RobotComponent)]
	public class RobotComponent: HotfixComponent
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