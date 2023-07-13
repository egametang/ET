using UnityEngine;

namespace UniFramework.Tween
{
	public abstract class ValueNode<ValueType> : ITweenNode where ValueType : struct
	{
		/// <summary>
		/// 补间委托
		/// </summary>
		/// <param name="t">运行时长</param>
		/// <param name="b">起始数值</param>
		/// <param name="c">变化差值</param>
		/// <param name="d">总时长</param>
		public delegate float TweenEaseDelegate(float t, float b, float c, float d);

		/// <summary>
		/// 插值委托
		/// </summary>
		/// <param name="from">起始数值</param>
		/// <param name="to">目标数值</param>
		/// <param name="progress">进度值</param>
		public delegate ValueType TweenLerpDelegate(ValueType from, ValueType to, float progress);


		private readonly float _duration;
		private ValueType _valueFrom;
		private ValueType _valueTo;

		private ETweenLoop _tweenLoop = ETweenLoop.None;
		private int _loopCount = -1;
		private long _loopCounter = 0;
		private float _timeReverse = 1f;
		private float _runningTime = 0;

		private System.Action<ValueType> _onUpdate = null;
		private System.Action _onBegin = null;
		private System.Action _onComplete = null;
		private System.Action _onDispose = null;
		protected TweenEaseDelegate _easeFun = null;
		protected TweenLerpDelegate _lerpFun = null;

		/// <summary>
		/// 补间结果
		/// </summary>
		public ValueType Result { get; private set; }

		/// <summary>
		/// 节点状态
		/// </summary>
		public ETweenStatus Status { private set; get; } = ETweenStatus.Idle;


		public ValueNode(float duration, ValueType from, ValueType to)
		{
			_duration = duration;
			_valueFrom = from;
			_valueTo = to;
			_easeFun = TweenEase.Linear.Default;
		}
		void ITweenNode.OnDispose()
		{
			_onDispose?.Invoke();
		}
		void ITweenNode.OnUpdate(float deltaTime)
		{
			if(Status == ETweenStatus.Idle)
			{		
				Status = ETweenStatus.Runing;
				_onBegin?.Invoke();
			}

			if(Status == ETweenStatus.Runing)
			{
				_runningTime += (deltaTime * _timeReverse);
				if (_duration > 0 && _runningTime > 0 && _runningTime < _duration)
				{
					float progress = _easeFun.Invoke(_runningTime, 0, 1, _duration);
					Result = GetResultValue(_valueFrom, _valueTo, progress);
					_onUpdate?.Invoke(Result);
				}
				else
				{
					if (_tweenLoop == ETweenLoop.None)
					{
						Result = _valueTo;
						_onUpdate?.Invoke(Result);

						Status = ETweenStatus.Completed;
						_onComplete?.Invoke();
					}
					else if (_tweenLoop == ETweenLoop.Restart)
					{
						_runningTime = 0;
						Result = _valueTo;
						_onUpdate?.Invoke(Result);

						_loopCounter++;
						if (_loopCount > 0 && _loopCounter >= _loopCount)
						{
							Status = ETweenStatus.Completed;
							_onComplete?.Invoke();
						}
					}
					else if (_tweenLoop == ETweenLoop.PingPong)
					{
						_timeReverse *= -1;
						if (_timeReverse > 0)
						{
							_runningTime = 0;
							Result = _valueFrom;
							_onUpdate?.Invoke(Result);

							// 注意：完整PingPong算一次
							_loopCounter++;
							if (_loopCount > 0 && _loopCounter >= _loopCount)
							{
								Status = ETweenStatus.Completed;
								_onComplete?.Invoke();
							}
						}
						else
						{
							_runningTime = _duration;
							Result = _valueTo;
							_onUpdate?.Invoke(Result);
						}
					}
					else
					{
						throw new System.NotImplementedException();
					}
				}
			}
		}
		void ITweenNode.Abort()
		{
			Status = ETweenStatus.Abort;
		}

		public ValueNode<ValueType> SetRunningTime(float runingTime)
		{
			_runningTime = runingTime;
			return this;
		}
		public ValueNode<ValueType> SetValueFrom(ValueType value)
		{
			_valueFrom = value;
			return this;
		}
		public ValueNode<ValueType> SetValueTo(ValueType value)
		{
			_valueTo = value;
			return this;
		}
		public ValueNode<ValueType> SetLoop(ETweenLoop tweenLoop, int loopCount = -1)
		{
			_tweenLoop = tweenLoop;
			_loopCount = loopCount;
			return this;
		}
		public ValueNode<ValueType> SetEase(AnimationCurve easeCurve)
		{
			if (easeCurve == null)
			{
				UniLogger.Error($"{nameof(AnimationCurve)} param is null.");
				return this;
			}

			// 获取动画总时长
			float length = 0f;
			for (int i = 0; i < easeCurve.keys.Length; i++)
			{
				var key = easeCurve.keys[i];
				if (key.time > length)
					length = key.time;
			}

			_easeFun = delegate (float t, float b, float c, float d)
			{
				float time = length * (t / d);
				return easeCurve.Evaluate(time) * c + b;
			};

			return this;
		}
		public ValueNode<ValueType> SetEase(TweenEaseDelegate easeFun)
		{
			if (easeFun == null)
			{
				UniLogger.Error($"{nameof(TweenEaseDelegate)} param is null.");
				return this;
			}

			_easeFun = easeFun;
			return this;
		}
		public ValueNode<ValueType> SetLerp(TweenLerpDelegate lerpFun)
		{
			if (lerpFun == null)
			{
				UniLogger.Error($"{nameof(TweenLerpDelegate)} param is null.");
				return this;
			}

			_lerpFun = lerpFun;
			return this;
		}
		public ValueNode<ValueType> SetOnUpdate(System.Action<ValueType> onUpdate)
		{
			_onUpdate = onUpdate;
			return this;
		}
		public ValueNode<ValueType> SetOnBegin(System.Action onBegin)
		{
			_onBegin = onBegin;
			return this;
		}
		public ValueNode<ValueType> SetOnComplete(System.Action onComplete)
		{
			_onComplete = onComplete;
			return this;
		}
		public ValueNode<ValueType> SetOnDispose(System.Action onDispose)
		{
			_onDispose = onDispose;
			return this;
		}

		protected abstract ValueType GetResultValue(ValueType from, ValueType to, float progress);
	}

	/// <summary>
	/// Float Tween
	/// </summary>
	public sealed class FloatTween : ValueNode<float>
	{
		public static FloatTween Allocate(float duration, float from, float to)
		{
			return new FloatTween(duration, from, to);
		}

		public FloatTween(float duration, float from, float to) : base(duration, from, to)
		{
		}
		protected override float GetResultValue(float from, float to, float progress)
		{
			if (_lerpFun != null)
				return _lerpFun.Invoke(from, to, progress);
			else
				return Mathf.LerpUnclamped(from, to, progress);
		}
	}

	/// <summary>
	/// Vector2 Tween
	/// </summary>
	public sealed class Vector2Tween : ValueNode<Vector2>
	{
		public static Vector2Tween Allocate(float duration, Vector2 from, Vector2 to)
		{
			return new Vector2Tween(duration, from, to);
		}

		public Vector2Tween(float duration, Vector2 from, Vector2 to) : base(duration, from, to)
		{
		}
		protected override Vector2 GetResultValue(Vector2 from, Vector2 to, float progress)
		{
			if (_lerpFun != null)
				return _lerpFun.Invoke(from, to, progress);
			else
				return Vector2.LerpUnclamped(from, to, progress);
		}
	}

	/// <summary>
	/// Vector3 Tween
	/// </summary>
	public sealed class Vector3Tween : ValueNode<Vector3>
	{
		public static Vector3Tween Allocate(float duration, Vector3 from, Vector3 to)
		{
			return new Vector3Tween(duration, from, to);
		}

		public Vector3Tween(float duration, Vector3 from, Vector3 to) : base(duration, from, to)
		{
		}
		protected override Vector3 GetResultValue(Vector3 from, Vector3 to, float progress)
		{
			if (_lerpFun != null)
				return _lerpFun.Invoke(from, to, progress);
			else
				return Vector3.LerpUnclamped(from, to, progress);
		}
	}

	/// <summary>
	/// Vector4 Tween
	/// </summary>
	public sealed class Vector4Tween : ValueNode<Vector4>
	{
		public static Vector4Tween Allocate(float duration, Vector4 from, Vector4 to)
		{
			return new Vector4Tween(duration, from, to);
		}

		public Vector4Tween(float duration, Vector4 from, Vector4 to) : base(duration, from, to)
		{
		}
		protected override Vector4 GetResultValue(Vector4 from, Vector4 to, float progress)
		{
			if (_lerpFun != null)
				return _lerpFun.Invoke(from, to, progress);
			else
				return Vector4.LerpUnclamped(from, to, progress);
		}
	}

	/// <summary>
	/// Color Tween
	/// </summary>
	public sealed class ColorTween : ValueNode<Color>
	{
		public static ColorTween Allocate(float duration, Color from, Color to)
		{
			return new ColorTween(duration, from, to);
		}

		public ColorTween(float duration, Color from, Color to) : base(duration, from, to)
		{
		}
		protected override Color GetResultValue(Color from, Color to, float progress)
		{
			if (_lerpFun != null)
				return _lerpFun.Invoke(from, to, progress);
			else
				return Color.LerpUnclamped(from, to, progress);
		}
	}

	/// <summary>
	/// Quaternion Tween
	/// </summary>
	public sealed class QuaternionTween : ValueNode<Quaternion>
	{
		public static QuaternionTween Allocate(float duration, Quaternion from, Quaternion to)
		{
			return new QuaternionTween(duration, from, to);
		}

		public QuaternionTween(float duration, Quaternion from, Quaternion to) : base(duration, from, to)
		{
		}
		protected override Quaternion GetResultValue(Quaternion from, Quaternion to, float progress)
		{
			if (_lerpFun != null)
				return _lerpFun.Invoke(from, to, progress);
			else
				return Quaternion.LerpUnclamped(from, to, progress);
		}
	}
}