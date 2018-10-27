using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
	class GearAnimationValue
	{
		public bool playing;
		public int frame;

		public GearAnimationValue(bool playing, int frame)
		{
			this.playing = playing;
			this.frame = frame;
		}
	}

	/// <summary>
	/// Gear is a connection between object and controller.
	/// </summary>
	public class GearAnimation : GearBase
	{
		Dictionary<string, GearAnimationValue> _storage;
		GearAnimationValue _default;

		public GearAnimation(GObject owner)
			: base(owner)
		{
		}

		protected override void Init()
		{
			_default = new GearAnimationValue(((IAnimationGear)_owner).playing, ((IAnimationGear)_owner).frame);
			_storage = new Dictionary<string, GearAnimationValue>();
		}

		override protected void AddStatus(string pageId, ByteBuffer buffer)
		{
			GearAnimationValue gv;
			if (pageId == null)
				gv = _default;
			else
			{
				gv = new GearAnimationValue(false, 0);
				_storage[pageId] = gv;
			}

			gv.playing = buffer.ReadBool();
			gv.frame = buffer.ReadInt();
		}

		override public void Apply()
		{
			_owner._gearLocked = true;

			GearAnimationValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				gv = _default;

			IAnimationGear mc = (IAnimationGear)_owner;
			mc.frame = gv.frame;
			mc.playing = gv.playing;

			_owner._gearLocked = false;
		}

		override public void UpdateState()
		{
			IAnimationGear mc = (IAnimationGear)_owner;
			GearAnimationValue gv;
			if (!_storage.TryGetValue(_controller.selectedPageId, out gv))
				_storage[_controller.selectedPageId] = new GearAnimationValue(mc.playing, mc.frame);
			else
			{
				gv.playing = mc.playing;
				gv.frame = mc.frame;
			}
		}
	}
}
