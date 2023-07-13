using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace UniFramework.Animation
{
	internal class AnimPlayable
	{
		private readonly List<AnimClip> _animClips = new List<AnimClip>(10);
		private readonly List<AnimMixer> _animMixers = new List<AnimMixer>(10);

		private PlayableGraph _graph;
		private AnimationPlayableOutput _output;
		private AnimationLayerMixerPlayable _mixerRoot;

		public void Create(Animator animator)
		{
			string name = animator.gameObject.name;
			_graph = PlayableGraph.Create(name);
			_graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

			_mixerRoot = AnimationLayerMixerPlayable.Create(_graph);
			_output = AnimationPlayableOutput.Create(_graph, name, animator);
			_output.SetSourcePlayable(_mixerRoot);
		}
		public void Update(float deltaTime)
		{
			_graph.Evaluate(deltaTime);

			// 更新所有层级
			for (int i = 0; i < _animMixers.Count; i++)
			{
				var mixer = _animMixers[i];
				if(mixer.IsConnect)
					mixer.Update(deltaTime);
			}
		}
		public void Destroy()
		{
			_graph.Destroy();
		}

		/// <summary>
		/// Play the graph
		/// </summary>
		public void PlayGraph()
		{
			_graph.Play();
		}

		/// <summary>
		/// Stop the graph
		/// </summary>
		public void StopGraph()
		{
			_graph.Stop();
		}

		/// <summary>
		/// 获取动画的状态
		/// </summary>
		/// <param name="name">动画名称</param>
		/// <returns>如果动画不存在返回空</returns>
		public AnimState GetAnimState(string name)
		{
			for (int i = 0; i < _animClips.Count; i++)
			{
				if (_animClips[i].Name == name)
					return _animClips[i].State;
			}
			return null;
		}

		/// <summary>
		/// 检测动画是否正在播放
		/// </summary>
		/// <param name="name">动画名称</param>
		public bool IsPlaying(string name)
		{
			AnimClip animClip = GetAnimClip(name);
			if (animClip == null)
				return false;

			return animClip.IsConnect && animClip.IsPlaying;
		}

		/// <summary>
		/// 播放一个动画
		/// </summary>
		/// <param name="name">动画名称</param>
		/// <param name="fadeLength">融合时间</param>
		public void Play(string name, float fadeLength)
		{
			var animClip = GetAnimClip(name);
			if (animClip == null)
			{
				UniLogger.Warning($"Not found animation {name}");
				return;
			}

			int layer = animClip.Layer;
			var animMixer = GetAnimMixer(layer);
			if (animMixer == null)
				animMixer = CreateAnimMixer(layer);

			if(animMixer.IsConnect == false)
				animMixer.Connect(_mixerRoot, animMixer.Layer);		

			animMixer.Play(animClip, fadeLength);
		}

		/// <summary>
		/// 停止一个动画
		/// </summary>
		/// <param name="name">动画名称</param>
		public void Stop(string name)
		{
			var animClip = GetAnimClip(name);
			if (animClip == null)
			{
				UniLogger.Warning($"Not found animation {name}");
				return;
			}

			if (animClip.IsConnect == false)
				return;

			var animMixer = GetAnimMixer(animClip.Layer);
			if (animMixer == null)
				throw new System.Exception("Should never get here.");

			animMixer.Stop(animClip.Name);
		}

		/// <summary>
		/// 添加一个动画片段
		/// </summary>
		/// <param name="name">动画名称</param>
		/// <param name="clip">动画片段</param>
		/// <param name="layer">动画层级</param>
		public bool AddAnimation(string name, AnimationClip clip, int layer = 0)
		{
			if (string.IsNullOrEmpty(name))
				throw new System.ArgumentException("Name is null or empty.");
			if (clip == null)
				throw new System.ArgumentNullException();
			if (layer < 0)
				throw new System.Exception("Layer must be greater than zero.");

			if (IsContains(name))
			{
				UniLogger.Warning($"Animation already exists : {name}");
				return false;
			}

			AnimClip animClip = new AnimClip(_graph, clip, name, layer);
			_animClips.Add(animClip);
			return true;
		}

		/// <summary>
		/// 移除一个动画片段
		/// </summary>
		/// <param name="name">动画名称</param>
		public bool RemoveAnimation(string name)
		{
			if (IsContains(name) == false)
			{
				UniLogger.Warning($"Not found Animation : {name}");
				return false;
			}

			AnimClip animClip = GetAnimClip(name);
			AnimMixer animMixer = GetAnimMixer(animClip.Layer);
			if(animMixer != null)
				animMixer.RemoveClip(animClip.Name);

			animClip.Destroy();
			_animClips.Remove(animClip);
			return true;
		}

		/// <summary>
		/// 是否包含一个动画状态
		/// </summary>
		/// <param name="name">动画名称</param>
		public bool IsContains(string name)
		{
			for (int i = 0; i < _animClips.Count; i++)
			{
				if (_animClips[i].Name == name)
					return true;
			}
			return false;
		}

		private AnimClip GetAnimClip(string name)
		{
			for (int i = 0; i < _animClips.Count; i++)
			{
				if (_animClips[i].Name == name)
					return _animClips[i];
			}
			return null;
		}
		private AnimMixer GetAnimMixer(int layer)
		{
			for (int i = 0; i < _animMixers.Count; i++)
			{
				if (_animMixers[i].Layer == layer)
					return _animMixers[i];
			}
			return null;
		}
		private AnimMixer CreateAnimMixer(int layer)
		{
			// Increase input count
			int inputCount = _mixerRoot.GetInputCount();
			if(layer == 0 && inputCount == 0)
			{
				_mixerRoot.SetInputCount(1);
			}
			else
			{
				if (layer > inputCount - 1)
				{
					_mixerRoot.SetInputCount(layer + 1);
				}
			}

			var animMixer = new AnimMixer(_graph, layer);
			_animMixers.Add(animMixer);
			return animMixer;
		}
	}
}