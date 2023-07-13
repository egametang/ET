using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace UniFramework.Animation
{
	public class AnimState
	{
		private readonly AnimClip _animClip;

		private AnimState()
		{
		}
		internal AnimState(AnimClip animClip)
		{
			_animClip = animClip;
		}


		/// <summary>
		/// The name of animation.
		/// </summary>
		public string Name
		{
			get
			{
				return _animClip.Name;
			}
		}

		/// <summary>
		/// The length of the animation clip in seconds.
		/// </summary>
		public float Length
		{
			get
			{
				return _animClip.ClipLength;
			}
		}

		/// <summary>
		/// The layer of animation.
		/// </summary>
		public int Layer
		{
			get
			{
				return _animClip.Layer;
			}
		}

		/// <summary>
		///  Wrapping mode of the animation.
		/// </summary>
		public WrapMode WrapMode
		{
			get
			{
				return _animClip.WrapMode;
			}
		}


		/// <summary>
		/// The weight of animation.
		/// </summary>
		public float Weight
		{
			get
			{
				return _animClip.Weight;
			}
			set
			{
				_animClip.Weight = value;
			}
		}

		/// <summary>
		/// The current time of the animation.
		/// </summary>
		public float Time
		{
			get
			{
				return _animClip.Time;
			}
			set
			{
				_animClip.Time = value;
			}
		}

		/// <summary>
		/// The normalized time of the animation.
		/// </summary>
		public float NormalizedTime
		{
			get
			{
				return _animClip.NormalizedTime;
			}
			set
			{
				_animClip.NormalizedTime = value;
			}
		}

		/// <summary>
		/// The playback speed of the animation. 1 is normal playback speed.
		/// </summary>
		public float Speed
		{
			get
			{
				return _animClip.Speed;
			}
			set
			{
				_animClip.Speed = value;
			}
		}
	}
}