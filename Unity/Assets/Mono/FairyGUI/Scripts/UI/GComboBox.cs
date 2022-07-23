using System;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// GComboBox class.
    /// </summary>
    public class GComboBox : GComponent
    {
        /// <summary>
        /// Visible item count of the drop down list.
        /// </summary>
        public int visibleItemCount;

        /// <summary>
        /// 
        /// </summary>
        public GComponent dropdown;

        /// <summary>
        /// Play sound when button is clicked.
        /// </summary>
        public NAudioClip sound;

        /// <summary>
        /// Volume of the click sound. (0-1)
        /// </summary>
        public float soundVolumeScale;

        protected GObject _titleObject;
        protected GObject _iconObject;
        protected GList _list;

        protected List<string> _items;
        protected List<string> _icons;
        protected List<string> _values;
        protected PopupDirection _popupDirection;
        protected Controller _selectionController;

        bool _itemsUpdated;
        int _selectedIndex;
        Controller _buttonController;

        bool _down;
        bool _over;

        EventListener _onChanged;

        public GComboBox()
        {
            visibleItemCount = UIConfig.defaultComboBoxVisibleItemCount;
            _itemsUpdated = true;
            _selectedIndex = -1;
            _items = new List<string>();
            _values = new List<string>();
            _popupDirection = PopupDirection.Auto;
            soundVolumeScale = 1;
        }

        /// <summary>
        /// Dispatched when selection was changed.
        /// </summary>
        public EventListener onChanged
        {
            get { return _onChanged ?? (_onChanged = new EventListener(this, "onChanged")); }
        }

        /// <summary>
        /// Icon of the combobox.
        /// </summary>
        override public string icon
        {
            get
            {
                if (_iconObject != null)
                    return _iconObject.icon;
                else
                    return null;
            }

            set
            {
                if (_iconObject != null)
                    _iconObject.icon = value;
                UpdateGear(7);
            }
        }

        /// <summary>
        /// Title of the combobox.
        /// </summary>
        public string title
        {
            get
            {
                if (_titleObject != null)
                    return _titleObject.text;
                else
                    return null;
            }
            set
            {
                if (_titleObject != null)
                    _titleObject.text = value;
                UpdateGear(6);
            }
        }

        /// <summary>
        /// Same of the title.
        /// </summary>
        override public string text
        {
            get { return this.title; }
            set { this.title = value; }
        }

        /// <summary>
        /// Text color
        /// </summary>
        public Color titleColor
        {
            get
            {
                GTextField tf = GetTextField();
                if (tf != null)
                    return tf.color;
                else
                    return Color.black;
            }
            set
            {
                GTextField tf = GetTextField();
                if (tf != null)
                    tf.color = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int titleFontSize
        {
            get
            {
                GTextField tf = GetTextField();
                if (tf != null)
                    return tf.textFormat.size;
                else
                    return 0;
            }
            set
            {
                GTextField tf = GetTextField();
                if (tf != null)
                {
                    TextFormat format = tf.textFormat;
                    format.size = value;
                    tf.textFormat = format;
                }
            }
        }

        /// <summary>
        /// Items to build up drop down list.
        /// </summary>
        public string[] items
        {
            get
            {
                return _items.ToArray();
            }
            set
            {
                _items.Clear();
                if (value != null)
                    _items.AddRange(value);
                ApplyListChange();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string[] icons
        {
            get { return _icons != null ? _icons.ToArray() : null; }
            set
            {
                this.iconList.Clear();
                if (value != null)
                    _icons.AddRange(value);
                ApplyListChange();
            }
        }

        /// <summary>
        /// Values, should be same size of the items. 
        /// </summary>
        public string[] values
        {
            get { return _values.ToArray(); }
            set
            {
                _values.Clear();
                if (value != null)
                    _values.AddRange(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> itemList
        {
            get { return _items; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> valueList
        {
            get { return _values; }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<string> iconList
        {
            get { return _icons ?? (_icons = new List<string>()); }
        }

        /// <summary>
        /// Call this method after you made changes on itemList or iconList
        /// </summary>
        public void ApplyListChange()
        {
            if (_items.Count > 0)
            {
                if (_selectedIndex >= _items.Count)
                    _selectedIndex = _items.Count - 1;
                else if (_selectedIndex == -1)
                    _selectedIndex = 0;

                this.text = _items[_selectedIndex];
                if (_icons != null && _selectedIndex < _icons.Count)
                    this.icon = _icons[_selectedIndex];
            }
            else
            {
                this.text = string.Empty;
                if (_icons != null)
                    this.icon = null;
                _selectedIndex = -1;
            }

            _itemsUpdated = true;
        }

        /// <summary>
        /// Selected index.
        /// </summary>
        public int selectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                if (_selectedIndex == value)
                    return;

                _selectedIndex = value;
                if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                {
                    this.text = (string)_items[_selectedIndex];
                    if (_icons != null && _selectedIndex < _icons.Count)
                        this.icon = _icons[_selectedIndex];
                }
                else
                {
                    this.text = string.Empty;
                    if (_icons != null)
                        this.icon = null;
                }

                UpdateSelectionController();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Controller selectionController
        {
            get { return _selectionController; }
            set { _selectionController = value; }
        }

        /// <summary>
        /// Selected value.
        /// </summary>
        public string value
        {
            get
            {
                if (_selectedIndex >= 0 && _selectedIndex < _values.Count)
                    return _values[_selectedIndex];
                else
                    return null;
            }
            set
            {
                int index = _values.IndexOf(value);
                if (index == -1 && value == null)
                    index = _values.IndexOf(string.Empty);
                if (index == -1)
                    index = 0;
                this.selectedIndex = index;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public PopupDirection popupDirection
        {
            get { return _popupDirection; }
            set { _popupDirection = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public GTextField GetTextField()
        {
            if (_titleObject is GTextField)
                return (GTextField)_titleObject;
            else if (_titleObject is GLabel)
                return ((GLabel)_titleObject).GetTextField();
            else if (_titleObject is GButton)
                return ((GButton)_titleObject).GetTextField();
            else
                return null;
        }

        protected void SetState(string value)
        {
            if (_buttonController != null)
                _buttonController.selectedPage = value;
        }

        protected void SetCurrentState()
        {
            if (this.grayed && _buttonController != null && _buttonController.HasPage(GButton.DISABLED))
                SetState(GButton.DISABLED);
            else if (dropdown != null && dropdown.parent != null)
                SetState(GButton.DOWN);
            else
                SetState(_over ? GButton.OVER : GButton.UP);
        }

        override protected void HandleGrayedChanged()
        {
            if (_buttonController != null && _buttonController.HasPage(GButton.DISABLED))
            {
                if (this.grayed)
                    SetState(GButton.DISABLED);
                else
                    SetState(GButton.UP);
            }
            else
                base.HandleGrayedChanged();
        }

        override public void HandleControllerChanged(Controller c)
        {
            base.HandleControllerChanged(c);

            if (_selectionController == c)
                this.selectedIndex = c.selectedIndex;
        }

        void UpdateSelectionController()
        {
            if (_selectionController != null && !_selectionController.changing
                && _selectedIndex < _selectionController.pageCount)
            {
                Controller c = _selectionController;
                _selectionController = null;
                c.selectedIndex = _selectedIndex;
                _selectionController = c;
            }
        }

        public override void Dispose()
        {
            if (dropdown != null)
            {
                dropdown.Dispose();
                dropdown = null;
            }
            _selectionController = null;

            base.Dispose();
        }

        override protected void ConstructExtension(ByteBuffer buffer)
        {
            buffer.Seek(0, 6);

            _buttonController = GetController("button");
            _titleObject = GetChild("title");
            _iconObject = GetChild("icon");

            string str = buffer.ReadS();
            if (str != null)
            {
                dropdown = UIPackage.CreateObjectFromURL(str) as GComponent;
                if (dropdown == null)
                {
                    Debug.LogWarning("FairyGUI: " + this.resourceURL + " should be a component.");
                    return;
                }

                _list = dropdown.GetChild("list") as GList;
                if (_list == null)
                {
                    Debug.LogWarning("FairyGUI: " + this.resourceURL + ": should container a list component named list.");
                    return;
                }
                _list.onClickItem.Add(__clickItem);

                _list.AddRelation(dropdown, RelationType.Width);
                _list.RemoveRelation(dropdown, RelationType.Height);

                dropdown.AddRelation(_list, RelationType.Height);
                dropdown.RemoveRelation(_list, RelationType.Width);

                dropdown.SetHome(this);
            }

            displayObject.onRollOver.Add(__rollover);
            displayObject.onRollOut.Add(__rollout);
            displayObject.onTouchBegin.Add(__touchBegin);
            displayObject.onTouchEnd.Add(__touchEnd);
            displayObject.onClick.Add(__click);
        }

        override public void Setup_AfterAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_AfterAdd(buffer, beginPos);

            if (!buffer.Seek(beginPos, 6))
                return;

            if ((ObjectType)buffer.ReadByte() != packageItem.objectType)
                return;

            string str;
            int itemCount = buffer.ReadShort();
            for (int i = 0; i < itemCount; i++)
            {
                int nextPos = buffer.ReadUshort();
                nextPos += buffer.position;

                _items.Add(buffer.ReadS());
                _values.Add(buffer.ReadS());
                str = buffer.ReadS();
                if (str != null)
                {
                    if (_icons == null)
                        _icons = new List<string>();
                    _icons.Add(str);
                }

                buffer.position = nextPos;
            }

            str = buffer.ReadS();
            if (str != null)
            {
                this.text = str;
                _selectedIndex = _items.IndexOf(str);
            }
            else if (_items.Count > 0)
            {
                _selectedIndex = 0;
                this.text = _items[0];
            }
            else
                _selectedIndex = -1;

            str = buffer.ReadS();
            if (str != null)
                this.icon = str;

            if (buffer.ReadBool())
                this.titleColor = buffer.ReadColor();
            int iv = buffer.ReadInt();
            if (iv > 0)
                visibleItemCount = iv;
            _popupDirection = (PopupDirection)buffer.ReadByte();

            iv = buffer.ReadShort();
            if (iv >= 0)
                _selectionController = parent.GetControllerAt(iv);

            if (buffer.version >= 5)
            {
                str = buffer.ReadS();
                if (str != null)
                    sound = UIPackage.GetItemAssetByURL(str) as NAudioClip;
                soundVolumeScale = buffer.ReadFloat();
            }
        }

        public void UpdateDropdownList()
        {
            if (_itemsUpdated)
            {
                _itemsUpdated = false;
                RenderDropdownList();
                _list.ResizeToFit(visibleItemCount);
            }
        }

        protected void ShowDropdown()
        {
            UpdateDropdownList();
            if (_list.selectionMode == ListSelectionMode.Single)
                _list.selectedIndex = -1;
            dropdown.width = this.width;
            _list.EnsureBoundsCorrect(); //avoid flicker

            this.root.TogglePopup(dropdown, this, _popupDirection);
            if (dropdown.parent != null)
            {
                dropdown.displayObject.onRemovedFromStage.Add(__popupWinClosed);
                SetState(GButton.DOWN);
            }
        }

        virtual protected void RenderDropdownList()
        {
            _list.RemoveChildrenToPool();
            int cnt = _items.Count;
            for (int i = 0; i < cnt; i++)
            {
                GObject item = _list.AddItemFromPool();
                item.text = _items[i];
                item.icon = (_icons != null && i < _icons.Count) ? _icons[i] : null;
                item.name = i < _values.Count ? _values[i] : string.Empty;
            }
        }

        private void __popupWinClosed(object obj)
        {
            dropdown.displayObject.onRemovedFromStage.Remove(__popupWinClosed);
            SetCurrentState();

            RequestFocus();
        }

        private void __clickItem(EventContext context)
        {
            if (dropdown.parent is GRoot)
                ((GRoot)dropdown.parent).HidePopup(dropdown);
            _selectedIndex = int.MinValue;
            this.selectedIndex = _list.GetChildIndex((GObject)context.data);

            DispatchEvent("onChanged", null);
        }

        private void __rollover()
        {
            _over = true;
            if (_down || dropdown != null && dropdown.parent != null)
                return;

            SetCurrentState();
        }

        private void __rollout()
        {
            _over = false;
            if (_down || dropdown != null && dropdown.parent != null)
                return;

            SetCurrentState();
        }

        private void __touchBegin(EventContext context)
        {
            if (context.initiator is InputTextField)
                return;

            _down = true;

            if (dropdown != null)
                ShowDropdown();

            context.CaptureTouch();
        }

        private void __touchEnd(EventContext context)
        {
            if (_down)
            {
                _down = false;
                if (dropdown != null && dropdown.parent != null)
                    SetCurrentState();
            }
        }

        private void __click()
        {
            if (sound != null && sound.nativeClip != null)
                Stage.inst.PlayOneShotSound(sound.nativeClip, soundVolumeScale);
        }
    }
}
