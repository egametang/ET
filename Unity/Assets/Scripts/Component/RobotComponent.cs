namespace Model
{
	[DisposerEvent(typeof(RobotComponent))]
	public class RobotComponent : Component
    {
		private void Awake()
		{
		}
		
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