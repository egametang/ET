using ETModel;

namespace ETHotfix
{
	public static class RealmGateAddressHelper
	{
		public static StartConfig GetGate()
		{
			int count = StartConfigComponent.Instance.Gates.Count;

			int n = RandomHelper.RandomNumber(0, count);

			return StartConfigComponent.Instance.Gates[n];
		}
	}
}
