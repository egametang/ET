using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ETModel
{
	public class UnitPathComponent : Component
	{
		public List<Vector3> Path = new List<Vector3>();

		public Vector3 ServerPos;

		public CancellationTokenSource CancellationTokenSource;
		
		public async ETTask StartMove(CancellationToken cancellationToken)
		{
			for (int i = 0; i < this.Path.Count; ++i)
			{
				Vector3 v = this.Path[i];

				float speed = 5;

				if (i == 0)
				{
					// 矫正移动速度
					Vector3 clientPos = this.GetParent<Unit>().Position;
					float serverf = (ServerPos - v).magnitude;
					if (serverf > 0.1f)
					{
						float clientf = (clientPos - v).magnitude;
						speed = clientf / serverf * speed;
					}
				}

				this.Entity.GetComponent<TurnComponent>().Turn(v);
				await this.Entity.GetComponent<MoveComponent>().MoveToAsync(v, speed, cancellationToken);
			}
		}

		public async ETVoid StartMove(M2C_PathfindingResult message)
		{
			// 取消之前的移动协程
			this.CancellationTokenSource?.Cancel();
			this.CancellationTokenSource = new CancellationTokenSource();

			this.Path.Clear();
			for (int i = 0; i < message.Xs.Count; ++i)
			{
				this.Path.Add(new Vector3(message.Xs[i], message.Ys[i], message.Zs[i]));
			}
			ServerPos = new Vector3(message.X, message.Y, message.Z);
			
			await StartMove(this.CancellationTokenSource.Token);
			this.CancellationTokenSource.Dispose();
			this.CancellationTokenSource = null;
		}
	}
}