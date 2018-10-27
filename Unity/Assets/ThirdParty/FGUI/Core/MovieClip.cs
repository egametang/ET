using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class MovieClip : Image
	{
		/// <summary>
		/// 
		/// </summary>
		public class Frame
		{
			public Rect rect;
			public float addDelay;
			public Rect uvRect;
			public bool rotated;
		}

		/// <summary>
		/// 
		/// </summary>
		public float interval;

		/// <summary>
		/// 
		/// </summary>
		public bool swing;

		/// <summary>
		/// 
		/// </summary>
		public float repeatDelay;

		/// <summary>
		/// 
		/// </summary>
		public int frameCount { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public Frame[] frames { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public float timeScale;

		/// <summary>
		/// Whether to ignore Unity time scale.
		/// </summary>
		public bool ignoreEngineTimeScale;

		/// <summary>
		/// 
		/// </summary>
		public EventListener onPlayEnd { get; private set; }

		int _frame;
		bool _playing;
		int _start;
		int _end;
		int _times;
		int _endAt;
		int _status; //0-none, 1-next loop, 2-ending, 3-ended

		float _frameElapsed; //当前帧延迟
		bool _reversed;
		int _repeatedCount;
		int _displayFrame;
		TimerCallback _timerDelegate;

		/// <summary>
		/// 
		/// </summary>
		public MovieClip()
		{
			interval = 0.1f;
			_playing = true;
			_timerDelegate = OnTimer;
			timeScale = 1;
			ignoreEngineTimeScale = false;

			onPlayEnd = new EventListener(this, "onPlayEnd");

			if (Application.isPlaying)
			{
				onAddedToStage.Add(OnAddedToStage);
				onRemovedFromStage.Add(OnRemoveFromStage);
			}

			SetPlaySettings();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="frames"></param>
		/// <param name="boundsRect"></param>
		public void SetData(NTexture texture, Frame[] frames, Rect boundsRect)
		{
			this.frames = frames;
			this.frameCount = frames.Length;
			_contentRect = boundsRect;

			if (_end == -1 || _end > frameCount - 1)
				_end = frameCount - 1;
			if (_endAt == -1 || _endAt > frameCount - 1)
				_endAt = frameCount - 1;

			if (_frame < 0 || _frame > frameCount - 1)
				_frame = frameCount - 1;

			graphics.texture = texture;
			OnSizeChanged(true, true);
			InvalidateBatchingState();

			_displayFrame = -1;
			_frameElapsed = 0;
			_repeatedCount = 0;
			_reversed = false;

			CheckTimer();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			this.frameCount = 0;
			graphics.texture = null;
			graphics.ClearMesh();
		}

		/// <summary>
		/// 
		/// </summary>
		public bool playing
		{
			get { return _playing; }
			set
			{
				if (_playing != value)
				{
					_playing = value;
					CheckTimer();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int frame
		{
			get { return _frame; }
			set
			{
				if (_frame != value)
				{
					if (frames != null && value >= frameCount)
						value = frameCount - 1;

					_frame = value;
					_frameElapsed = 0;
					_displayFrame = -1;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Rewind()
		{
			_frame = 0;
			_frameElapsed = 0;
			_reversed = false;
			_repeatedCount = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="anotherMc"></param>
		public void SyncStatus(MovieClip anotherMc)
		{
			_frame = anotherMc._frame;
			_frameElapsed = anotherMc._frameElapsed;
			_reversed = anotherMc._reversed;
			_repeatedCount = anotherMc._repeatedCount;
			_displayFrame = -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="time"></param>
		public void Advance(float time)
		{
			int beginFrame = _frame;
			bool beginReversed = _reversed;
			float backupTime = time;
			while (true)
			{
				float tt = interval + frames[_frame].addDelay;
				if (_frame == 0 && _repeatedCount > 0)
					tt += repeatDelay;
				if (time < tt)
				{
					_frameElapsed = 0;
					break;
				}

				time -= tt;

				if (swing)
				{
					if (_reversed)
					{
						_frame--;
						if (_frame <= 0)
						{
							_frame = 0;
							_repeatedCount++;
							_reversed = !_reversed;
						}
					}
					else
					{
						_frame++;
						if (_frame > frameCount - 1)
						{
							_frame = Mathf.Max(0, frameCount - 2);
							_repeatedCount++;
							_reversed = !_reversed;
						}
					}
				}
				else
				{
					_frame++;
					if (_frame > frameCount - 1)
					{
						_frame = 0;
						_repeatedCount++;
					}
				}

				if (_frame == beginFrame && _reversed == beginReversed) //走了一轮了
				{
					float roundTime = backupTime - time; //这就是一轮需要的时间
					time -= Mathf.FloorToInt(time / roundTime) * roundTime; //跳过
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void SetPlaySettings()
		{
			SetPlaySettings(0, -1, 0, -1);
		}

		/// <summary>
		/// 从start帧开始，播放到end帧（-1表示结尾），重复times次（0表示无限循环），循环结束后，停止在endAt帧（-1表示参数end）
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="times"></param>
		/// <param name="endAt"></param>
		public void SetPlaySettings(int start, int end, int times, int endAt)
		{
			_start = start;
			_end = end;
			if (_end == -1 || _end > frameCount - 1)
				_end = frameCount - 1;
			_times = times;
			_endAt = endAt;
			if (_endAt == -1)
				_endAt = _end;
			_status = 0;
			this.frame = start;
		}

		void OnAddedToStage()
		{
			if (_playing && frameCount > 0)
				Timers.inst.AddUpdate(_timerDelegate);
		}

		void OnRemoveFromStage()
		{
			Timers.inst.Remove(_timerDelegate);
		}

		void CheckTimer()
		{
			if (!Application.isPlaying)
				return;

			if (_playing && frameCount > 0 && this.stage != null)
				Timers.inst.AddUpdate(_timerDelegate);
			else
				Timers.inst.Remove(_timerDelegate);
		}

		void OnTimer(object param)
		{
			if (!_playing || frameCount == 0 || _status == 3)
				return;

			float dt;
			if (ignoreEngineTimeScale)
			{
				dt = Time.unscaledDeltaTime;
				if (dt > 0.1f)
					dt = 0.1f;
			}
			else
				dt = Time.deltaTime;
			if (timeScale != 1)
				dt *= timeScale;

			_frameElapsed += dt;
			float tt = interval + frames[_frame].addDelay;
			if (_frame == 0 && _repeatedCount > 0)
				tt += repeatDelay;
			if (_frameElapsed < tt)
				return;

			_frameElapsed -= tt;
			if (_frameElapsed > interval)
				_frameElapsed = interval;

			if (swing)
			{
				if (_reversed)
				{
					_frame--;
					if (_frame <= 0)
					{
						_frame = 0;
						_repeatedCount++;
						_reversed = !_reversed;
					}
				}
				else
				{
					_frame++;
					if (_frame > frameCount - 1)
					{
						_frame = Mathf.Max(0, frameCount - 2);
						_repeatedCount++;
						_reversed = !_reversed;
					}
				}
			}
			else
			{
				_frame++;
				if (_frame > frameCount - 1)
				{
					_frame = 0;
					_repeatedCount++;
				}
			}

			if (_status == 1) //new loop
			{
				_frame = _start;
				_frameElapsed = 0;
				_status = 0;
			}
			else if (_status == 2) //ending
			{
				_frame = _endAt;
				_frameElapsed = 0;
				_status = 3; //ended

				onPlayEnd.Call();
			}
			else
			{
				if (_frame == _end)
				{
					if (_times > 0)
					{
						_times--;
						if (_times == 0)
							_status = 2;  //ending
						else
							_status = 1; //new loop
					}
					else if (_start != 0)
						_status = 1; //new loop
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		public override void Update(UpdateContext context)
		{
			if (frameCount > 0 && _frame != _displayFrame)
				DrawFrame();

			base.Update(context);
		}

		void DrawFrame()
		{
			_displayFrame = _frame;

			if (_frame >= frames.Length)
				graphics.ClearMesh();
			else
			{
				Frame frame = frames[_frame];

				if (frame.rect.width == 0)
					graphics.ClearMesh();
				else
				{
					Rect uvRect = frame.uvRect;
					if (_flip != FlipType.None)
						ToolSet.FlipRect(ref uvRect, _flip);

					graphics.DrawRect(frame.rect, uvRect, _color);
					if (frame.rotated)
						NGraphics.RotateUV(graphics.uv, ref uvRect);
					graphics.UpdateMesh();
				}
			}
		}

		protected override void Rebuild()
		{
			if (_texture != null)
				base.Rebuild();
			else if (frameCount > 0)
			{
				_requireUpdateMesh = false;
				DrawFrame();
			}
		}
	}
}
