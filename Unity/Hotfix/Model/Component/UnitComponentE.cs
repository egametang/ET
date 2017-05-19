using Model;

namespace Hotfix
{
	[EntityEvent(EntityEventId.UnitComponent)]
	public static class UnitComponentE
	{
		public static void Awake(this UnitComponent component)
		{
			Log.Debug("UnitComponent Awake");
		}
	}
}
