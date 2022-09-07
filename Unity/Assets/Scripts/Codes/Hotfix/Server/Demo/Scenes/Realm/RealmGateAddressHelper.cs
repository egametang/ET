using System.Collections.Generic;


namespace ET.Server
{
	public static class RealmGateAddressHelper
	{
		public static StartSceneConfig GetGate(int zone)
		{
			List<StartSceneConfig> zoneGates = StartSceneConfigCategory.Instance.Gates[zone];
			
			int n = RandomGenerator.RandomNumber(0, zoneGates.Count);

			return zoneGates[n];
		}
	}
}
