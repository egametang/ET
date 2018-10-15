using ETModel;
using PF;
using Vector3 = UnityEngine.Vector3;

namespace ETHotfix
{
	[MessageHandler]
	public class Actor_CreateUnitsHandler : AMHandler<Actor_CreateUnits>
	{
		protected override void Run(ETModel.Session session, Actor_CreateUnits message)
		{	
			UnitComponent unitComponent = ETModel.Game.Scene.GetComponent<UnitComponent>();
			
			foreach (UnitInfo unitInfo in message.Units)
			{
				if (unitComponent.Get(unitInfo.UnitId) != null)
				{
					continue;
				}
				Unit unit = UnitFactory.Create(unitInfo.UnitId);
				unit.Position = new Vector3(unitInfo.X, unitInfo.Y, unitInfo.Z);
			}
		}
	}
}
