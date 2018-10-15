using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class MoveComponentAwakeSystem : AwakeSystem<MoveComponent>
	{
		public override void Awake(MoveComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class MoveComponentUpdateSystem : UpdateSystem<MoveComponent>
	{
		public override void Update(MoveComponent self)
		{
			self.Update();
		}
	}

	public class MoveComponent : Component
	{
		private AnimatorComponent animatorComponent;

		public long mainSpeed;
		public Vector3 speed;

		// turn
		public Quaternion To;
		public Quaternion From;
		public float t = float.MaxValue;
		public float TurnTime = 0.1f;

		public bool IsArrived { get; private set; } = true;

		public Vector3 Dest;

		public TaskCompletionSource<bool> moveTcs;

		public Vector3 Speed
		{
			get
			{
				return speed;
			}
			set
			{
				speed = value;
				animatorComponent?.SetFloatValue("Speed", speed.magnitude);
			}
		}

		public void Awake()
		{
			this.animatorComponent = this.Entity.GetComponent<AnimatorComponent>();
		}

		public void Update()
		{
			UpdateTurn();

			if (this.IsArrived)
			{
				return;
			}

			if (this.Speed.sqrMagnitude < 0.01f)
			{
				return;
			}

			Unit unit = this.GetParent<Unit>();
			Vector3 moveVector3 = this.Speed * Time.deltaTime;
			
			float dist = (this.Dest - unit.Position).magnitude;
			if (moveVector3.magnitude >= dist || dist < 0.1f)
			{
				unit.Position = this.Dest;
				this.IsArrived = true;
				this.moveTcs?.SetResult(true);
				return;
			}

			unit.Position = unit.Position + moveVector3;
		}

		private void UpdateTurn()
		{
			//Log.Debug($"update turn: {this.t} {this.TurnTime}");
			if (this.t > this.TurnTime)
			{
				return;
			}

			this.t += Time.deltaTime;

			Quaternion v = Quaternion.Slerp(this.From, this.To, this.t / this.TurnTime);
			this.GetParent<Unit>().Rotation = v;
		}

		public void MoveTo(Vector3 dest, float speedValue)
		{
			if ((dest - this.GetParent<Unit>().Position).magnitude < 0.1f)
			{
				this.IsArrived = true;
				return;
			}
			
			if ((dest - this.Dest).magnitude < 0.1f)
			{
				return;
			}
			
			this.IsArrived = false;
			Vector3 spd = dest - this.GetParent<Unit>().Position;
			spd = spd.normalized * speedValue;
			this.Speed = spd;
			this.Dest = dest;
		}

		public Task<bool> MoveToAsync(Vector3 dest, float speedValue, CancellationToken cancellationToken)
		{
			if ((dest - this.GetParent<Unit>().Position).magnitude < 0.1f)
			{
				this.IsArrived = true;
				return Task.FromResult(false);
			}

			if ((dest - this.Dest).magnitude < 0.1f)
			{
				return Task.FromResult(false);
			}
			
			this.moveTcs = new TaskCompletionSource<bool>();
			this.IsArrived = false;
			Vector3 spd = dest - this.GetParent<Unit>().Position;
			spd = spd.normalized * speedValue;
			this.Speed = spd;
			this.Dest = dest;

			cancellationToken.Register(() => this.moveTcs = null);

			return this.moveTcs.Task;
		}

		/// <summary>
		/// 停止移动Unit,只停止地面正常移动,不停止击飞等移动
		/// </summary>
		public void Stop()
		{
			this.speed = Vector3.zero;
			this.animatorComponent?.SetFloatValue("Speed", 0);
		}

		/// <summary>
		/// 改变Unit的朝向
		/// </summary>
		public void Turn2D(Vector3 dir, float turnTime = 0.1f)
		{
			Vector3 nexpos = this.GetParent<Unit>().GameObject.transform.position + dir;
			Turn(nexpos, turnTime);
		}

		/// <summary>
		/// 改变Unit的朝向
		/// </summary>
		public void Turn(Vector3 target, float turnTime = 0.1f)
		{
			Quaternion quaternion = PositionHelper.GetVector3ToQuaternion(this.GetParent<Unit>().Position, target);

			this.To = quaternion;
			this.From = this.GetParent<Unit>().Rotation;
			this.t = 0;
			this.TurnTime = turnTime;
		}

		/// <summary>
		/// 改变Unit的朝向
		/// </summary>
		/// <param name="angle">与X轴正方向的夹角</param>
		public void Turn(float angle, float turnTime = 0.1f)
		{
			Quaternion quaternion = PositionHelper.GetAngleToQuaternion(angle);

			this.To = quaternion;
			this.From = this.GetParent<Unit>().Rotation;
			this.t = 0;
			this.TurnTime = turnTime;
		}

		public void Turn(Quaternion quaternion, float turnTime = 0.1f)
		{
			this.To = quaternion;
			this.From = this.GetParent<Unit>().Rotation;
			this.t = 0;
			this.TurnTime = turnTime;
		}

		public void TurnImmediately(Quaternion quaternion)
		{
			this.GetParent<Unit>().Rotation = quaternion;
		}

		public void TurnImmediately(Vector3 target)
		{
			Vector3 nowPos = this.GetParent<Unit>().Position;
			if (nowPos == target)
			{
				return;
			}

			Quaternion quaternion = PositionHelper.GetVector3ToQuaternion(this.GetParent<Unit>().Position, target);
			this.GetParent<Unit>().Rotation = quaternion;
		}

		public void TurnImmediately(float angle)
		{
			Quaternion quaternion = PositionHelper.GetAngleToQuaternion(angle);
			this.GetParent<Unit>().Rotation = quaternion;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();
		}
	}
}