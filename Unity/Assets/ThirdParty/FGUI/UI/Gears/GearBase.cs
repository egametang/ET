using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	abstract public class GearBase
	{
		public static bool disableAllTweenEffect = false;

		protected GObject _owner;
		protected Controller _controller;
		protected GearTweenConfig _tweenConfig;

		public GearBase(GObject owner)
		{
			_owner = owner;
		}

		/// <summary>
		/// Controller object.
		/// </summary>
		public Controller controller
		{
			get
			{
				return _controller;
			}

			set
			{
				if (value != _controller)
				{
					_controller = value;
					if (_controller != null)
						Init();
				}
			}
		}

		public GearTweenConfig tweenConfig
		{
			get
			{
				if (_tweenConfig == null)
					_tweenConfig = new GearTweenConfig();
				return _tweenConfig;
			}
		}

		public void Setup(ByteBuffer buffer)
		{
			_controller = _owner.parent.GetControllerAt(buffer.ReadShort());
			Init();

			if (this is GearDisplay)
			{
				int cnt = buffer.ReadShort();
				string[] pages = new string[cnt];
				for (int i = 0; i < cnt; i++)
					pages[i] = buffer.ReadS();
				((GearDisplay)this).pages = pages;
			}
			else
			{
				int cnt = buffer.ReadShort();
				for (int i = 0; i < cnt; i++)
				{
					string page = buffer.ReadS();
					if (page == null)
						continue;

					AddStatus(page, buffer);
				}

				if (buffer.ReadBool())
					AddStatus(null, buffer);
			}

			if (buffer.ReadBool())
			{
				_tweenConfig = new GearTweenConfig();
				_tweenConfig.easeType = (EaseType)buffer.ReadByte();
				_tweenConfig.duration = buffer.ReadFloat();
				_tweenConfig.delay = buffer.ReadFloat();
			}
		}

		virtual public void UpdateFromRelations(float dx, float dy)
		{
		}

		abstract protected void AddStatus(string pageId, ByteBuffer buffer);
		abstract protected void Init();

		/// <summary>
		/// Call when controller active page changed.
		/// </summary>
		abstract public void Apply();

		/// <summary>
		/// Call when object's properties changed.
		/// </summary>
		abstract public void UpdateState();
	}

	public class GearTweenConfig
	{
		/// <summary>
		/// Use tween to apply change.
		/// </summary>
		public bool tween;

		/// <summary>
		/// Ease type.
		/// </summary>
		public EaseType easeType;

		/// <summary>
		/// Tween duration in seconds.
		/// </summary>
		public float duration;

		/// <summary>
		/// Tween delay in seconds.
		/// </summary>
		public float delay;

		internal uint _displayLockToken;
		internal GTweener _tweener;

		public GearTweenConfig()
		{
			tween = true;
			easeType = EaseType.QuadOut;
			duration = 0.3f;
			delay = 0;
		}
	}
}
