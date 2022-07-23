using System;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// Gear is a connection between object and controller.
    /// </summary>
    public class GearDisplay : GearBase
    {
        /// <summary>
        /// Pages involed in this gear.
        /// </summary>
        public string[] pages { get; set; }

        int _visible;
        uint _displayLockToken;

        public GearDisplay(GObject owner)
            : base(owner)
        {
            _displayLockToken = 1;
        }

        override protected void AddStatus(string pageId, ByteBuffer buffer)
        {
        }

        override protected void Init()
        {
            pages = null;
        }

        override public void Apply()
        {
            _displayLockToken++;
            if (_displayLockToken == 0)
                _displayLockToken = 1;

            if (pages == null || pages.Length == 0
                || Array.IndexOf(pages, _controller.selectedPageId) != -1)
                _visible = 1;
            else
                _visible = 0;
        }

        override public void UpdateState()
        {
        }

        public uint AddLock()
        {
            _visible++;
            return _displayLockToken;
        }

        public void ReleaseLock(uint token)
        {
            if (token == _displayLockToken)
                _visible--;
        }

        public bool connected
        {
            get { return _controller == null || _visible > 0; }
        }
    }
}
