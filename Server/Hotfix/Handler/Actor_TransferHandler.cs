using System;
using System.Net;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class Actor_TransferHandler : AMActorRpcHandler<Unit, Actor_TransferRequest, Actor_TransferResponse>
	{
		protected override async ETTask Run(Unit unit, Actor_TransferRequest message, Action<Actor_TransferResponse> reply)
		{
			Actor_TransferResponse response = new Actor_TransferResponse();

			try
			{
				long unitId = unit.Id;

				// 先在location锁住unit的地址
				await Game.Scene.GetComponent<LocationProxyComponent>().Lock(unitId, unit.InstanceId);

				// 删除unit,让其它进程发送过来的消息找不到actor，重发
				Game.EventSystem.Remove(unitId);
				
				long instanceId = unit.InstanceId;
				
				int mapIndex = message.MapIndex;

				StartConfigComponent startConfigComponent = StartConfigComponent.Instance;

				// 考虑AllServer情况
				if (startConfigComponent.Count == 1)
				{
					mapIndex = 0;
				}

				// 传送到map
				StartConfig mapConfig = startConfigComponent.MapConfigs[mapIndex];
				IPEndPoint address = mapConfig.GetComponent<InnerConfig>().IPEndPoint;
				Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(address);

				// 只删除不disponse否则M2M_TrasferUnitRequest无法序列化Unit
				Game.Scene.GetComponent<UnitComponent>().RemoveNoDispose(unitId);
				M2M_TrasferUnitResponse m2m_TrasferUnitResponse = (M2M_TrasferUnitResponse)await session.Call(new M2M_TrasferUnitRequest() { Unit = unit });
				unit.Dispose();

				// 解锁unit的地址,并且更新unit的instanceId
				await Game.Scene.GetComponent<LocationProxyComponent>().UnLock(unitId, instanceId, m2m_TrasferUnitResponse.InstanceId);

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}