using System.Collections.Generic;


namespace ET.Server
{
	public static partial class RealmGateAddressHelper
	{
		public static StartSceneConfig GetGate(int zone, string account)
		{
			long hash = account.GetLongHashCode();
			
			List<StartSceneConfig> zoneGates = StartSceneConfigCategory.Instance.Gates[zone];
			
			return zoneGates[(int)(hash % zoneGates.Count)];
		}
	}
}
