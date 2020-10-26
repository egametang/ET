using UnityEngine;

namespace ET
{
	public class TurnComponentUpdateSystem : UpdateSystem<TurnComponent>
	{
		public override void Update(TurnComponent self)
		{
			self.Update();
		}
	}
	
    public static class TurnComponentSystem
    {
        
		public static void Update(this TurnComponent self)
		{
			self.UpdateTurn();
		}

		private static void UpdateTurn(this TurnComponent self)
		{
			//Log.Debug($"update turn: {this.t} {this.TurnTime}");
			if (self.t > self.TurnTime)
			{
				return;
			}

			self.t += Time.deltaTime;

			Quaternion v = Quaternion.Slerp(self.From, self.To, self.t / self.TurnTime);
			self.GetParent<Unit>().Rotation = v;
		}

		/// <summary>
		/// 改变Unit的朝向
		/// </summary>
		public static void Turn2D(this TurnComponent self, Vector3 dir, float turnTime = 0.1f)
		{
			Vector3 nexpos = self.GetParent<Unit>().GameObject.transform.position + dir;
			self.Turn(nexpos, turnTime);
		}

		/// <summary>
		/// 改变Unit的朝向
		/// </summary>
		public static void Turn(this TurnComponent self, Vector3 target, float turnTime = 0.1f)
		{
			Quaternion quaternion = PositionHelper.GetVector3ToQuaternion(self.GetParent<Unit>().Position, target);

			self.To = quaternion;
			self.From = self.GetParent<Unit>().Rotation;
			self.t = 0;
			self.TurnTime = turnTime;
		}

		/// <summary>
		/// 改变Unit的朝向
		/// </summary>
		/// <param name="angle">与X轴正方向的夹角</param>
		public static void Turn(this TurnComponent self, float angle, float turnTime = 0.1f)
		{
			Quaternion quaternion = PositionHelper.GetAngleToQuaternion(angle);

			self.To = quaternion;
			self.From = self.GetParent<Unit>().Rotation;
			self.t = 0;
			self.TurnTime = turnTime;
		}

		public static void Turn(this TurnComponent self, Quaternion quaternion, float turnTime = 0.1f)
		{
			self.To = quaternion;
			self.From = self.GetParent<Unit>().Rotation;
			self.t = 0;
			self.TurnTime = turnTime;
		}

		public static void TurnImmediately(this TurnComponent self, Quaternion quaternion)
		{
			self.GetParent<Unit>().Rotation = quaternion;
		}

		public static void TurnImmediately(this TurnComponent self, Vector3 target)
		{
			Vector3 nowPos = self.GetParent<Unit>().Position;
			if (nowPos == target)
			{
				return;
			}

			Quaternion quaternion = PositionHelper.GetVector3ToQuaternion(self.GetParent<Unit>().Position, target);
			self.GetParent<Unit>().Rotation = quaternion;
		}

		public static void TurnImmediately(this TurnComponent self, float angle)
		{
			Quaternion quaternion = PositionHelper.GetAngleToQuaternion(angle);
			self.GetParent<Unit>().Rotation = quaternion;
		}

    }
}