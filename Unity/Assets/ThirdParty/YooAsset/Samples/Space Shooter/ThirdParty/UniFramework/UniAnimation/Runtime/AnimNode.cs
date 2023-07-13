using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace UniFramework.Animation
{
	internal abstract class AnimNode
	{
		private readonly PlayableGraph _graph;
		private Playable _source;
		private Playable _parent;

		private float _fadeSpeed = 0f;
		private float _fadeWeight = 0f;
		private bool _isFading = false;


		/// <summary>
		/// 是否已经连接
		/// </summary>
		public bool IsConnect { get; private set; } = false;

		/// <summary>
		/// 输入端口
		/// </summary>
		public int InputPort { private set; get; }

		/// <summary>
		/// 是否已经完成
		/// If the duration of the playable is set, when the time of the playable reaches its duration during playback this flag will be set to true.
		/// </summary>
		public bool IsDone
		{
			get
			{
				return _source.IsDone();
			}
		}

		/// <summary>
		/// 是否有效
		/// if the Playable is properly constructed by the PlayableGraph and has not been destroyed, false otherwise.
		/// </summary>
		public bool IsValid
		{
			get
			{
				return _source.IsValid();
			}
		}

		/// <summary>
		/// 是否正在播放中
		/// </summary>
		public bool IsPlaying
		{
			get
			{
				return _source.GetPlayState() == PlayState.Playing;
			}
		}

		/// <summary>
		/// 时间轴
		/// </summary>
		public float Time
		{
			set
			{
				_source.SetTime(value);
			}
			get
			{
				return (float)_source.GetTime();
			}
		}

		/// <summary>
		/// 播放速度
		/// </summary>
		public float Speed
		{
			set
			{
				_source.SetSpeed(value);
			}
			get
			{
				return (float)_source.GetSpeed();
			}
		}

		/// <summary>
		/// 权重值
		/// </summary>
		public float Weight
		{
			set
			{
				_parent.SetInputWeight(InputPort, value);
			}
			get
			{
				return _parent.GetInputWeight(InputPort);
			}
		}


		public AnimNode(PlayableGraph graph)
		{
			_graph = graph;
		}
		public virtual void Update(float deltaTime)
		{
			if (_isFading)
			{
				Weight = Mathf.MoveTowards(Weight, _fadeWeight, _fadeSpeed * deltaTime);
				if (Mathf.Approximately(Weight, _fadeWeight))
				{
					_isFading = false;
				}
			}
		}
		public virtual void Destroy()
		{
			if (IsValid)
			{
				_graph.DestroySubgraph(_source);
			}
		}
		public virtual void PlayNode()
		{
			// NOTE : When playing, the local time of this Playable will be updated during the evaluation of the PlayableGraph.
			_source.Play();

			// NOTE : Changes a flag indicating that a playable has completed its operation.
			// Playable that reach the end of their duration are automatically marked as done.
			_source.SetDone(false);
		}
		public virtual void PauseNode()
		{
			// NOTE : When paused, the local time of this Playable will not be updated during the evaluation of the PlayableGraph.
			_source.Pause();

			// NOTE : Changes a flag indicating that a playable has completed its operation.
			// Playable that reach the end of their duration are automatically marked as done.
			_source.SetDone(true);
		}
		public virtual void ResetNode()
		{
			_fadeSpeed = 0;
			_fadeWeight = 0;
			_isFading = false;

			Time = 0;
			Speed = 1;
			Weight = 0;
		}

		/// <summary>
		/// 连接到父节点
		/// </summary>
		/// <param name="parent">父节点对象</param>
		/// <param name="inputPort">父节点上的输入端口</param>
		public void Connect(Playable parent, int parentInputPort)
		{
			if (IsConnect)
				throw new System.Exception("AnimNode is connected.");

			_parent = parent;
			InputPort = parentInputPort;

			// 重置节点
			ResetNode();

			// 连接
			_graph.Connect(_source, 0, parent, parentInputPort);
			IsConnect = true;
		}

		/// <summary>
		/// 同父节点断开连接
		/// </summary>
		public void Disconnect()
		{
			if (IsConnect == false)
				throw new System.Exception("AnimNode is disconnected.");

			// 断开
			_graph.Disconnect(_parent, InputPort);
			IsConnect = false;
		}

		/// <summary>
		/// 开始权重值过渡
		/// </summary>
		/// <param name="destWeight">目标权重值</param>
		/// <param name="fadeDuration">过渡时间</param>
		public void StartWeightFade(float destWeight, float fadeDuration)
		{
			if (fadeDuration <= 0)
			{
				Weight = destWeight;
				_isFading = false;
				return;
			}

			//注意：保持统一的渐变速度
			_fadeSpeed = 1f / fadeDuration;
			_fadeWeight = destWeight;
			_isFading = true;
		}

		protected void SetSourcePlayable(Playable playable)
		{
			_source = playable;
		}
	}
}