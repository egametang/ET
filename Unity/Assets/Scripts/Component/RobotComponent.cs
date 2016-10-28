using Base;

namespace Model
{
	[ObjectEvent]
	public class RobotComponentEvent : ObjectEvent<RobotComponent>, IAwake
	{
		public void Awake()
		{
			RobotComponent component = this.GetValue();
			component.Awake();
		}
	}

	public class RobotComponent : Component
    {
		public void Awake()
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