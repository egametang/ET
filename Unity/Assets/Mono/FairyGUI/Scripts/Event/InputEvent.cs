using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class InputEvent
    {
        /// <summary>
        /// x position in stage coordinates.
        /// </summary>
        public float x { get; internal set; }

        /// <summary>
        /// y position in stage coordinates.
        /// </summary>
        public float y { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public KeyCode keyCode { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public char character { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public EventModifiers modifiers { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public float mouseWheelDelta { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public int touchId { get; internal set; }

        /// <summary>
        /// -1-none,0-left,1-right,2-middle
        /// </summary>
        public int button { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public int clickCount { get; internal set; }

        /// <summary>
        /// Duraion of holding the button. You can read this in touchEnd or click event.
        /// </summary>
        /// <value></value>
        public float holdTime { get; internal set; }

        public InputEvent()
        {
            touchId = -1;
            x = 0;
            y = 0;
            clickCount = 0;
            keyCode = KeyCode.None;
            character = '\0';
            modifiers = 0;
            mouseWheelDelta = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector2 position
        {
            get { return new Vector2(x, y); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool isDoubleClick
        {
            get { return clickCount > 1 && button == 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ctrlOrCmd
        {
            get
            {
                return ctrl || command;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ctrl
        {
            get
            {
                return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool shift
        {
            get
            {
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool alt
        {
            get
            {
                return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool command
        {
            get
            {
                //In win, as long as the win key and other keys are pressed at the same time, the getKey will continue to return true. So it can only be shielded.
                if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
                    return Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
                else
                    return false;
            }
        }
    }
}
