using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace UniFramework.Animation
{
	internal sealed class AnimMixer : AnimNode
	{
		private const float HIDE_DURATION = 0.25f;
		private readonly List<AnimClip> _animClips = new List<AnimClip>(10);
		private AnimationMixerPlayable _mixer;
		private bool _isQuiting = false;

		/// <summary>
		/// 动画层级
		/// </summary>
		public int Layer { private set; get; }


		public AnimMixer(PlayableGraph graph, int layer) : base(graph)
		{
			Layer = layer;

			_mixer = AnimationMixerPlayable.Create(graph);
			SetSourcePlayable(_mixer);
		}
		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			for (int i = 0; i < _animClips.Count; i++)
			{
				var animClip = _animClips[i];
				if (animClip != null)
					animClip.Update(deltaTime);
			}

			bool isAllDone = true;
			for (int i = 0; i < _animClips.Count; i++)
			{
				var animClip = _animClips[i];
				if (animClip != null)
				{
					if (animClip.IsDone == false)
						isAllDone = false;
				}
			}

			// 当子节点都已经完成的时候断开连接
			if (isAllDone && _isQuiting == false)
			{
				_isQuiting = true;
				StartWeightFade(0, HIDE_DURATION);
			}
			if (_isQuiting)
			{
				if (Mathf.Approximately(Weight, 0f))
					DisconnectMixer();
			}
		}

		/// <summary>
		/// 播放指定动画
		/// </summary>
		public void Play(AnimClip newAnimClip, float fadeDuration)
		{
			// 重新激活混合器
			_isQuiting = false;
			StartWeightFade(1f, 0);

			if (IsContains(newAnimClip) == false)
			{
				// 优先插入到一个空位
				int index = _animClips.FindIndex(s => s == null);
				if (index == -1)
				{
					// Increase input count
					int inputCount = _mixer.GetInputCount();
					_mixer.SetInputCount(inputCount + 1);

					newAnimClip.Connect(_mixer, inputCount);
					_animClips.Add(newAnimClip);
				}
				else
				{
					newAnimClip.Connect(_mixer, index);
					_animClips[index] = newAnimClip;
				}
			}

			for (int i = 0; i < _animClips.Count; i++)
			{
				var animClip = _animClips[i];
				if (animClip == null)
					continue;

				if (animClip == newAnimClip)
				{
					animClip.StartWeightFade(1f, fadeDuration);
					animClip.PlayNode();
				}
				else
				{
					animClip.StartWeightFade(0f, fadeDuration);
					animClip.PauseNode();
				}
			}
		}

		/// <summary>
		/// 停止指定动画，恢复为初始状态
		/// </summary>
		public void Stop(string name)
		{
			AnimClip animClip = FindClip(name);
			if (animClip == null)
				return;

			animClip.PauseNode();
			animClip.ResetNode();
		}

		/// <summary>
		/// 暂停所有动画
		/// </summary>
		public void PauseAll()
		{
			for (int i = 0; i < _animClips.Count; i++)
			{
				var animClip = _animClips[i];
				if (animClip == null)
					continue;
				animClip.PauseNode();
			}
		}

		/// <summary>
		/// 是否包含该动画
		/// </summary>
		public bool IsContains(AnimClip clip)
		{
			foreach (var animClip in _animClips)
			{
				if (animClip == clip)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 移除一个动画
		/// </summary>
		public void RemoveClip(string name)
		{
			var animClip = FindClip(name);
			if (animClip == null)
				return;

			_animClips[animClip.InputPort] = null;
			animClip.Destroy();
		}

		/// <summary>
		/// 获取指定的动画
		/// </summary>
		/// <returns>如果没有返回NULL</returns>
		private AnimClip FindClip(string name)
		{
			foreach (var animClip in _animClips)
			{
				if (animClip != null && animClip.Name == name)
					return animClip;
			}

			UniLogger.Warning($"${nameof(AnimClip)} doesn't exist : {name}");
			return null;
		}

		private void DisconnectMixer()
		{
			for (int i = 0; i < _animClips.Count; i++)
			{
				var animClip = _animClips[i];
				if (animClip == null)
					continue;

				animClip.Disconnect();
				_animClips[i] = null;
			}

			Disconnect();
		}
	}
}