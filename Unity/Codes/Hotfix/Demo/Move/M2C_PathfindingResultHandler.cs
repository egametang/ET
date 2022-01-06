using System.Collections.Generic;
using UnityEngine;

namespace ET
{
	[MessageHandler]
	public class M2C_PathfindingResultHandler : AMHandler<M2C_PathfindingResult>
	{
		protected override async ETTask Run(Session session, M2C_PathfindingResult message)
		{
			Unit unit = session.DomainScene().CurrentScene().GetComponent<UnitComponent>().Get(message.Id);

			float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);

			using (ListComponent<Vector3> list = ListComponent<Vector3>.Create())
			{
				for (int i = 0; i < message.Xs.Count; ++i)
				{
					list.Add(new Vector3(message.Xs[i], message.Ys[i], message.Zs[i]));
				}

				await unit.GetComponent<MoveComponent>().MoveToAsync(list, speed);
			}
		}
	}
}
