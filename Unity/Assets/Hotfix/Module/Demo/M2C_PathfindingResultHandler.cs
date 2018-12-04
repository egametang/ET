using ETModel;
using UnityEngine;

namespace ETHotfix
{
	[MessageHandler]
	public class M2C_PathfindingResultHandler : AMHandler<M2C_PathfindingResult>
	{
		protected override void Run(ETModel.Session session, M2C_PathfindingResult message)
		{
			Unit unit = ETModel.Game.Scene.GetComponent<UnitComponent>().Get(message.Id);
			
			
			unit.GetComponent<AnimatorComponent>().SetFloatValue("Speed", 5f);
			UnitPathComponent unitPathComponent = unit.GetComponent<UnitPathComponent>();

			unitPathComponent.StartMove(message).NoAwait();

			GizmosDebug.Instance.Path.Clear();
			GizmosDebug.Instance.Path.Add(new Vector3(message.X, message.Y, message.Z));
			for (int i = 0; i < message.Xs.Count; ++i)
			{
				GizmosDebug.Instance.Path.Add(new Vector3(message.Xs[i], message.Ys[i], message.Zs[i]));
			}
		}
	}
}
