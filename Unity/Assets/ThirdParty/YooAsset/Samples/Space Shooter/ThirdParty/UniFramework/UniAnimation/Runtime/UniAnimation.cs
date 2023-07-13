using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Animation
{
	[RequireComponent(typeof(Animator))]
	public class UniAnimation : MonoBehaviour
	{
		[Serializable]
		public class AnimationWrapper
		{
			public int Layer;
			public WrapMode Mode;
			public AnimationClip Clip;
		}

		private AnimPlayable _animPlayable;
		private Animator _animator;

		[SerializeField]
		private AnimationWrapper[] _animations;

		[SerializeField]
		private bool _playAutomatically = true;

		[SerializeField]
		private bool _animatePhysics = false;

		/// <summary>
		/// 自动播放动画
		/// </summary>
		public bool PlayAutomatically
		{
			get
			{
				return _playAutomatically;
			}
			set
			{
				_playAutomatically = value;
			}
		}

		/// <summary>
		/// 物理更新模式
		/// </summary>
		public bool AnimatePhysics
		{
			get
			{
				return _animatePhysics;
			}
			set
			{
				_animatePhysics = value;
				_animator.updateMode = _animatePhysics ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal;
			}
		}


		public void Awake()
		{
			_animator = GetComponent<Animator>();
			_animator.updateMode = _animatePhysics ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal;

			_animPlayable = new AnimPlayable();
			_animPlayable.Create(_animator);

			// 添加列表动作
			for (int i = 0; i < _animations.Length; i++)
			{
				var wrapper = _animations[i];
				if (wrapper == null || wrapper.Clip == null)
					continue;

				wrapper.Clip.wrapMode = wrapper.Mode;
				_animPlayable.AddAnimation(wrapper.Clip.name, wrapper.Clip, wrapper.Layer);
			}
		}
		public void OnEnable()
		{
			_animPlayable.PlayGraph();

			if (PlayAutomatically)
			{
				var wrapper = GetDefaultWrapper();
				if (wrapper != null)
				{
					Play(wrapper.Clip.name, 0f);
				}
			}

			_animPlayable.Update(float.MaxValue);
		}
		public void OnDisable()
		{
			_animPlayable.StopGraph();
		}
		public void OnDestroy()
		{
			_animPlayable.Destroy();
		}
		public void Update()
		{
			_animPlayable.Update(Time.deltaTime);
		}

		/// <summary>
		/// 添加一个动画
		/// </summary>
		/// <param name="clip">动画片段</param>
		/// <param name="layer">动画层级</param>
		public bool AddAnimation(AnimationClip clip, int layer = 0)
		{
			return _animPlayable.AddAnimation(clip.name, clip, layer);
		}

		/// <summary>
		/// 移除动画
		/// </summary>
		/// <param name="name">动画名称</param>
		public bool RemoveAnimation(string name)
		{
			return _animPlayable.RemoveAnimation(name);
		}

		/// <summary>
		/// 获取动画状态
		/// </summary>
		public AnimState GetState(string name)
		{
			return _animPlayable.GetAnimState(name);
		}

		/// <summary>
		/// 动画是否在播放中
		/// </summary>
		public bool IsPlaying(string name)
		{
			return _animPlayable.IsPlaying(name);
		}

		/// <summary>
		/// 是否包含动画片段
		/// </summary>
		public bool IsContains(string name)
		{
			return _animPlayable.IsContains(name);
		}

		/// <summary>
		/// 播放动画
		/// </summary>
		public void Play(string name, float fadeLength = 0.25f)
		{
			_animPlayable.Play(name, fadeLength);
		}

		/// <summary>
		/// 停止动画
		/// </summary>
		public void Stop(string name)
		{
			_animPlayable.Stop(name);
		}

		private AnimationWrapper GetDefaultWrapper()
		{
			for (int i = 0; i < _animations.Length; i++)
			{
				var wrapper = _animations[i];
				if (wrapper == null || wrapper.Clip == null)
					continue;

				return wrapper;
			}
			return null;
		}
	}
}