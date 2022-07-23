using System;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// Gear is a connection between object and controller.
    /// </summary>
    public class GearDisplay2 : GearBase
    {
        /// <summary>
        /// Pages involed in this gear.
        /// </summary>
        public string[] pages { get; set; }
        public int condition;

        int _visible;

        public GearDisplay2(GObject owner)
            : base(owner)
        {
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
            if (pages == null || pages.Length == 0
                || Array.IndexOf(pages, _controller.selectedPageId) != -1)
                _visible = 1;
            else
                _visible = 0;
        }

        override public void UpdateState()
        {
        }
        public bool Evaluate(bool connected)
        {
            bool v = _controller == null || _visible > 0;
            if (this.condition == 0)
                v = v && connected;
            else
                v = v || connected;
            return v;
        }
    }
}
