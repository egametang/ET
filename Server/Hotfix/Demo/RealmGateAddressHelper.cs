using ET.StartServer;
using System.Collections.Generic;


namespace ET
{
	public static class RealmGateAddressHelper
	{
		public static StartScene GetGate(int zone)
		{
			List<StartScene> zoneGates = Tables.Ins.TbStartScene.Gates[zone];
			
			int n = RandomHelper.RandomNumber(0, zoneGates.Count);

			return zoneGates[n];
		}
	}
}
