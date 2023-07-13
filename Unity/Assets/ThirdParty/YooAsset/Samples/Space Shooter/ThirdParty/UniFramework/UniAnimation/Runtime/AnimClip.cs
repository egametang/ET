using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace UniFramework.Animation
{
	internal sealed class AnimClip : AnimNode
	{
		public readonly string Name;
		private readonly AnimationClip _clip;
		public AnimationClipPlayable _clipPlayable;

		/// <summary>
		/// 动画层级
		/// </summary>
		public int Layer = 0;

		/// <summary>
		/// 动画长度
		/// </summary>
		public float ClipLength
		{
			get
			{
				if (_clip == null)
					return 0f;
				if (Speed == 0f)
					return Mathf.Infinity;
				return _clip.length / Speed;
			}
		}

		/// <summary>
		/// 归一化时间轴
		/// </summary>
		public float NormalizedTime
		{
			set
			{
				if (_clip == null)
					return;
				Time = _clip.length * value;
			}

			get
			{
				if (_clip == null)
					return 1f;
				return Time / _clip.length;
			}
		}

		/// <summary>
		/// 动画模式
		/// </summary>
		public WrapMode WrapMode
		{
			set
			{
				if (_clip != null)
					_clip.wrapMode = value;
			}
			get
			{
				if (_clip == null)
					return WrapMode.Default;
				return _clip.wrapMode;
			}
		}

		/// <summary>
		/// 动画状态
		/// </summary>
		public AnimState State { private set; get; }

		public AnimClip(PlayableGraph graph, AnimationClip clip, string name, int layer) : base(graph)
		{
			_clip = clip;
			Name = name;
			Layer = layer;

			_clipPlayable = AnimationClipPlayable.Create(graph, clip);
			_clipPlayable.SetApplyFootIK(false);
			_clipPlayable.SetApplyPlayableIK(false);
			SetSourcePlayable(_clipPlayable);

			if (clip.wrapMode == WrapMode.Once)
			{
				_clipPlayable.SetDuration(clip.length);
			}

			State = new AnimState(this);
		}
		public override void PlayNode()
		{
			if (_clip.wrapMode == WrapMode.Once || _clip.wrapMode == WrapMode.ClampForever)
			{
				Time = 0;
			}

			base.PlayNode();
		}
	}
}