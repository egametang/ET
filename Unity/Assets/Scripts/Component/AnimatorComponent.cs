using System;
using System.Collections.Generic;
using UnityEngine;

namespace ETModel
{
	[ObjectSystem]
	public class AnimatorComponentAwakeSystem : AwakeSystem<AnimatorComponent>
	{
		public override void Awake(AnimatorComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class AnimatorComponentUpdateSystem : UpdateSystem<AnimatorComponent>
	{
		public override void Update(AnimatorComponent self)
		{
			self.Update();
		}
	}

	public class AnimatorComponent : Component
	{
		public Dictionary<string, AnimationClip> animationClips = new Dictionary<string, AnimationClip>();
		public HashSet<string> Parameter = new HashSet<string>();

		public MotionType MotionType;
		public float MontionSpeed;
		public bool isStop;
		public float stopSpeed;
		public Animator Animator;

		public void Awake()
		{
			Animator animator = this.GetParent<Unit>().GameObject.GetComponent<Animator>();

			if (animator == null)
			{
				return;
			}

			if (animator.runtimeAnimatorController == null)
			{
				return;
			}

			if (animator.runtimeAnimatorController.animationClips == null)
			{
				return;
			}
			this.Animator = animator;
			foreach (AnimationClip animationClip in animator.runtimeAnimatorController.animationClips)
			{
				this.animationClips[animationClip.name] = animationClip;
			}
			foreach (AnimatorControllerParameter animatorControllerParameter in animator.parameters)
			{
				this.Parameter.Add(animatorControllerParameter.name);
			}
		}

		public void Update()
		{
			if (this.isStop)
			{
				return;
			}

			if (this.MotionType == MotionType.None)
			{
				return;
			}

			try
			{
				this.Animator.SetFloat("MotionSpeed", this.MontionSpeed);

				this.Animator.SetTrigger(this.MotionType.ToString());

				this.MontionSpeed = 1;
				this.MotionType = MotionType.None;
			}
			catch (Exception ex)
			{
				throw new Exception($"动作播放失败: {this.MotionType}", ex);
			}
		}

		public bool HasParameter(string parameter)
		{
			return this.Parameter.Contains(parameter);
		}

		public void PlayInTime(MotionType motionType, float time)
		{
			AnimationClip animationClip;
			if (!this.animationClips.TryGetValue(motionType.ToString(), out animationClip))
			{
				throw new Exception($"找不到该动作: {motionType}");
			}

			float motionSpeed = animationClip.length / time;
			if (motionSpeed < 0.01f || motionSpeed > 1000f)
			{
				Log.Error($"motionSpeed数值异常, {motionSpeed}, 此动作跳过");
				return;
			}
			this.MotionType = motionType;
			this.MontionSpeed = motionSpeed;
		}

		public void Play(MotionType motionType, float motionSpeed = 1f)
		{
			if (!this.HasParameter(motionType.ToString()))
			{
				return;
			}
			this.MotionType = motionType;
			this.MontionSpeed = motionSpeed;
		}

		public float AnimationTime(MotionType motionType)
		{
			AnimationClip animationClip;
			if (!this.animationClips.TryGetValue(motionType.ToString(), out animationClip))
			{
				throw new Exception($"找不到该动作: {motionType}");
			}
			return animationClip.length;
		}

		public void PauseAnimator()
		{
			if (this.isStop)
			{
				return;
			}
			this.isStop = true;

			if (this.Animator == null)
			{
				return;
			}
			this.stopSpeed = this.Animator.speed;
			this.Animator.speed = 0;
		}

		public void RunAnimator()
		{
			if (!this.isStop)
			{
				return;
			}

			this.isStop = false;

			if (this.Animator == null)
			{
				return;
			}
			this.Animator.speed = this.stopSpeed;
		}

		public void SetBoolValue(string name, bool state)
		{
			if (!this.HasParameter(name))
			{
				return;
			}

			this.Animator.SetBool(name, state);
		}

		public void SetFloatValue(string name, float state)
		{
			if (!this.HasParameter(name))
			{
				return;
			}

			this.Animator.SetFloat(name, state);
		}

		public void SetIntValue(string name, int value)
		{
			if (!this.HasParameter(name))
			{
				return;
			}

			this.Animator.SetInteger(name, value);
		}

		public void SetTrigger(string name)
		{
			if (!this.HasParameter(name))
			{
				return;
			}

			this.Animator.SetTrigger(name);
		}

		public void SetAnimatorSpeed(float speed)
		{
			this.stopSpeed = this.Animator.speed;
			this.Animator.speed = speed;
		}

		public void ResetAnimatorSpeed()
		{
			this.Animator.speed = this.stopSpeed;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			base.Dispose();

			this.animationClips = null;
			this.Parameter = null;
			this.Animator = null;
		}
	}
}