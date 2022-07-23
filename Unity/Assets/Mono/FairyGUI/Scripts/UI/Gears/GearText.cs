using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// Gear is a connection between object and controller.
    /// </summary>
    public class GearText : GearBase
    {
        Dictionary<string, string> _storage;
        string _default;

        public GearText(GObject owner)
            : base(owner)
        {
        }

        protected override void Init()
        {
            _default = _owner.text;
            _storage = new Dictionary<string, string>();
        }

        override protected void AddStatus(string pageId, ByteBuffer buffer)
        {
            if (pageId == null)
                _default = buffer.ReadS();
            else
                _storage[pageId] = buffer.ReadS();
        }

        override public void Apply()
        {
            _owner._gearLocked = true;

            string cv;
            if (!_storage.TryGetValue(_controller.selectedPageId, out cv))
                cv = _default;

            _owner.text = cv;

            _owner._gearLocked = false;
        }

        override public void UpdateState()
        {
            _storage[_controller.selectedPageId] = _owner.text;
        }
    }
}
