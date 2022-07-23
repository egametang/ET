using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// 接收用户输入的文本控件。因为支持直接输入表情，所以从RichTextField派生。
    /// </summary>
    public class InputTextField : RichTextField
    {
        /// <summary>
        /// 
        /// </summary>
        public int maxLength { get; set; }

        /// <summary>
        /// 如果是true，则当文本获得焦点时，弹出键盘进行输入，如果是false则不会。
        /// 默认是使用Stage.keyboardInput的值。
        /// </summary>
        public bool keyboardInput { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int keyboardType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool hideInput { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool disableIME { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool mouseWheelEnabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static Action<InputTextField, string> onCopy;

        /// <summary>
        /// 
        /// </summary>
        public static Action<InputTextField> onPaste;

        /// <summary>
        /// 
        /// </summary>
        public static PopupMenu contextMenu;

        string _text;
        string _restrict;
        Regex _restrictPattern;
        bool _displayAsPassword;
        string _promptText;
        string _decodedPromptText;
        int _border;
        int _corner;
        Color _borderColor;
        Color _backgroundColor;
        bool _editable;

        bool _editing;
        int _caretPosition;
        int _selectionStart;
        int _composing;
        char _highSurrogateChar;
        string _textBeforeEdit;

        EventListener _onChanged;
        EventListener _onSubmit;

        Shape _caret;
        SelectionShape _selectionShape;
        float _nextBlink;

        const int GUTTER_X = 2;
        const int GUTTER_Y = 2;

        public InputTextField()
        {
            gameObject.name = "InputTextField";

            _text = string.Empty;
            maxLength = 0;
            _editable = true;
            _composing = 0;
            keyboardInput = Stage.keyboardInput;
            _borderColor = Color.black;
            _backgroundColor = Color.clear;
            mouseWheelEnabled = true;
            this.tabStop = true;
            cursor = "text-ibeam";

            /* 因为InputTextField定义了ClipRect，而ClipRect是四周缩进了2个像素的（GUTTER)，默认的点击测试
             * 是使用ClipRect的，那会造成无法点击四周的空白区域。所以这里自定义了一个HitArea
             */
            this.hitArea = new RectHitTest();
            this.touchChildren = false;

            onFocusIn.Add(__focusIn);
            onFocusOut.AddCapture(__focusOut);
            onKeyDown.Add(__keydown);
            onTouchBegin.AddCapture(__touchBegin);
            onTouchMove.AddCapture(__touchMove);
            onMouseWheel.Add(__mouseWheel);
            onClick.Add(__click);
            onRightClick.Add(__rightClick);
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onChanged
        {
            get { return _onChanged ?? (_onChanged = new EventListener(this, "onChanged")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public EventListener onSubmit
        {
            get { return _onSubmit ?? (_onSubmit = new EventListener(this, "onSubmit")); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                ClearSelection();
                UpdateText();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TextFormat textFormat
        {
            get
            {
                return base.textFormat;
            }
            set
            {
                base.textFormat = value;
                if (_editing)
                {
                    _caret.height = textField.textFormat.size;
                    _caret.DrawRect(0, Color.clear, textField.textFormat.color);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string restrict
        {
            get { return _restrict; }
            set
            {
                _restrict = value;
                if (string.IsNullOrEmpty(_restrict))
                    _restrictPattern = null;
                else
                    _restrictPattern = new Regex(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int caretPosition
        {
            get
            {
                textField.Redraw();
                return _caretPosition;
            }
            set
            {
                SetSelection(value, 0);
            }
        }

        public int selectionBeginIndex
        {
            get { return _selectionStart < _caretPosition ? _selectionStart : _caretPosition; }
        }

        public int selectionEndIndex
        {
            get { return _selectionStart < _caretPosition ? _caretPosition : _selectionStart; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string promptText
        {
            get
            {
                return _promptText;
            }
            set
            {
                _promptText = value;
                if (!string.IsNullOrEmpty(_promptText))
                    _decodedPromptText = UBBParser.inst.Parse(XMLUtils.EncodeString(_promptText));
                else
                    _decodedPromptText = null;
                UpdateText();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool displayAsPassword
        {
            get { return _displayAsPassword; }
            set
            {
                if (_displayAsPassword != value)
                {
                    _displayAsPassword = value;
                    UpdateText();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool editable
        {
            get { return _editable; }
            set
            {
                _editable = value;
                if (_caret != null)
                    _caret.visible = _editable;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int border
        {
            get { return _border; }
            set
            {
                _border = value;
                UpdateShape();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int corner
        {
            get { return _corner; }
            set
            {
                _corner = value;
                UpdateShape();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Color borderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                UpdateShape();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Color backgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                UpdateShape();
            }
        }

        void UpdateShape()
        {
            if (_border > 0 || _backgroundColor.a > 0)
            {
                CreateGraphics();

                graphics.enabled = true;
                RoundedRectMesh mesh = graphics.GetMeshFactory<RoundedRectMesh>();
                mesh.lineWidth = _border;
                mesh.lineColor = _borderColor;
                mesh.fillColor = _backgroundColor;
                mesh.topLeftRadius = mesh.topRightRadius = mesh.bottomLeftRadius = mesh.bottomRightRadius = corner;
                graphics.SetMeshDirty();
            }
            else
            {
                if (graphics != null)
                    graphics.enabled = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length">-1 means the rest count from start</param>
        public void SetSelection(int start, int length)
        {
            if (!_editing)
                Stage.inst.focus = this;

            _selectionStart = start;
            _caretPosition = length < 0 ? int.MaxValue : (start + length);
            if (!textField.Redraw())
            {
                int cnt = textField.charPositions.Count;
                if (_caretPosition >= cnt)
                    _caretPosition = cnt - 1;
                if (_selectionStart >= cnt)
                    _selectionStart = cnt - 1;
                UpdateCaret();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void ReplaceSelection(string value)
        {
            if (keyboardInput && Stage.keyboardInput && !Stage.inst.keyboard.supportsCaret)
            {
                this.text = _text + value;
                OnChanged();
                return;
            }

            if (!_editing)
                Stage.inst.focus = this;

            textField.Redraw();

            int t0, t1;
            if (_selectionStart != _caretPosition)
            {
                if (_selectionStart < _caretPosition)
                {
                    t0 = _selectionStart;
                    t1 = _caretPosition;
                    _caretPosition = _selectionStart;
                }
                else
                {
                    t0 = _caretPosition;
                    t1 = _selectionStart;
                    _selectionStart = _caretPosition;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                    return;

                t0 = t1 = _caretPosition;
            }

            StringBuilder buffer = new StringBuilder();
            GetPartialText(0, t0, buffer);
            if (!string.IsNullOrEmpty(value))
            {
                value = ValidateInput(value);
                buffer.Append(value);

                _caretPosition += GetTextlength(value);
            }
            GetPartialText(t1 + _composing, -1, buffer);

            string newText = buffer.ToString();
            if (maxLength > 0)
            {
                string newText2 = TruncateText(newText, maxLength);
                if (newText2.Length != newText.Length)
                    _caretPosition += (newText2.Length - newText.Length);
                newText = newText2;
            }

            this.text = newText;
            OnChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void ReplaceText(string value)
        {
            if (value == _text)
                return;

            if (value == null)
                value = string.Empty;

            value = ValidateInput(value);

            if (maxLength > 0)
                value = TruncateText(value, maxLength);

            _caretPosition = value.Length;

            this.text = value;
            OnChanged();
        }

        void GetPartialText(int startIndex, int endIndex, StringBuilder buffer)
        {
            int elementCount = textField.htmlElements.Count;
            int lastIndex = startIndex;
            string tt;
            if (_displayAsPassword)
                tt = _text;
            else
                tt = textField.parsedText;
            if (endIndex < 0)
                endIndex = tt.Length;
            for (int i = 0; i < elementCount; i++)
            {
                HtmlElement element = textField.htmlElements[i];
                if (element.htmlObject != null && element.text != null)
                {
                    if (element.charIndex >= startIndex && element.charIndex < endIndex)
                    {
                        buffer.Append(tt.Substring(lastIndex, element.charIndex - lastIndex));
                        buffer.Append(element.text);
                        lastIndex = element.charIndex + 1;
                    }
                }
            }
            if (lastIndex < tt.Length)
                buffer.Append(tt.Substring(lastIndex, endIndex - lastIndex));
        }

        int GetTextlength(string value)
        {
            int textLen = value.Length;
            int ret = textLen;
            for (int i = 0; i < textLen; i++)
            {
                if (char.IsHighSurrogate(value[i]))
                    ret--;
            }
            return ret;
        }

        string TruncateText(string value, int length)
        {
            int textLen = value.Length;
            int len = 0;
            int i = 0;
            while (i < textLen)
            {
                if (len == length)
                    return value.Substring(0, i);

                if (char.IsHighSurrogate(value[i]))
                    i++;
                i++;
                len++;
            }
            return value;
        }

        string ValidateInput(string source)
        {
            if (_restrict != null)
            {
                StringBuilder sb = new StringBuilder();
                Match mc = _restrictPattern.Match(source);
                int lastPos = 0;
                string s;
                while (mc != Match.Empty)
                {
                    if (mc.Index != lastPos)
                    {
                        //保留tab和回车
                        for (int i = lastPos; i < mc.Index; i++)
                        {
                            if (source[i] == '\n' || source[i] == '\t')
                                sb.Append(source[i]);
                        }
                    }

                    s = mc.ToString();
                    lastPos = mc.Index + s.Length;
                    sb.Append(s);

                    mc = mc.NextMatch();
                }
                for (int i = lastPos; i < source.Length; i++)
                {
                    if (source[i] == '\n' || source[i] == '\t')
                        sb.Append(source[i]);
                }

                return sb.ToString();
            }
            else
                return source;
        }

        void UpdateText()
        {
            if (!_editing && _text.Length == 0 && !string.IsNullOrEmpty(_decodedPromptText))
            {
                textField.htmlText = _decodedPromptText;
                return;
            }

            if (_displayAsPassword)
                textField.text = EncodePasswordText(_text);
            else
                textField.text = _text;

            _composing = Input.compositionString.Length;
            if (_composing > 0)
            {
                StringBuilder buffer = new StringBuilder();
                GetPartialText(0, _caretPosition, buffer);
                buffer.Append(Input.compositionString);
                GetPartialText(_caretPosition, -1, buffer);

                textField.text = buffer.ToString();
            }
        }

        string EncodePasswordText(string value)
        {
            int textLen = value.Length;
            StringBuilder tmp = new StringBuilder(textLen);
            int i = 0;
            while (i < textLen)
            {
                char c = value[i];
                if (c == '\n')
                    tmp.Append(c);
                else
                {
                    if (char.IsHighSurrogate(c))
                        i++;
                    tmp.Append("*");
                }
                i++;
            }
            return tmp.ToString();
        }

        void ClearSelection()
        {
            if (_selectionStart != _caretPosition)
            {
                if (_selectionShape != null)
                    _selectionShape.Clear();
                _selectionStart = _caretPosition;
            }
        }

        public string GetSelection()
        {
            if (_selectionStart == _caretPosition)
                return string.Empty;

            StringBuilder buffer = new StringBuilder();
            if (_selectionStart < _caretPosition)
                GetPartialText(_selectionStart, _caretPosition, buffer);
            else
                GetPartialText(_caretPosition, _selectionStart, buffer);
            return buffer.ToString();
        }

        void Scroll(int hScroll, int vScroll)
        {
            vScroll = Mathf.Clamp(vScroll, 0, textField.lines.Count - 1);
            TextField.LineInfo line = textField.lines[vScroll];
            hScroll = Mathf.Clamp(hScroll, 0, line.charCount - 1);

            TextField.CharPosition cp = GetCharPosition(line.charIndex + hScroll);
            Vector2 pt = GetCharLocation(cp);
            MoveContent(new Vector2(GUTTER_X - pt.x, GUTTER_Y - pt.y), false);
        }

        void AdjustCaret(TextField.CharPosition cp, bool moveSelectionHeader = false)
        {
            _caretPosition = cp.charIndex;
            if (moveSelectionHeader)
                _selectionStart = _caretPosition;

            UpdateCaret();
        }

        void UpdateCaret(bool forceUpdate = false)
        {
            TextField.CharPosition cp;
            if (_editing)
                cp = GetCharPosition(_caretPosition + Input.compositionString.Length);
            else
                cp = GetCharPosition(_caretPosition);

            Vector2 pos = GetCharLocation(cp);
            TextField.LineInfo line = textField.lines[cp.lineIndex];
            Vector2 offset = pos + textField.xy;

            if (offset.x < textField.textFormat.size)
                offset.x += Mathf.Min(50, _contentRect.width * 0.5f);
            else if (offset.x > _contentRect.width - GUTTER_X - textField.textFormat.size)
                offset.x -= Mathf.Min(50, _contentRect.width * 0.5f);

            if (offset.x < GUTTER_X)
                offset.x = GUTTER_X;
            else if (offset.x > _contentRect.width - GUTTER_X)
                offset.x = Mathf.Max(GUTTER_X, _contentRect.width - GUTTER_X);

            if (offset.y < GUTTER_Y)
                offset.y = GUTTER_Y;
            else if (offset.y + line.height >= _contentRect.height - GUTTER_Y)
                offset.y = Mathf.Max(GUTTER_Y, _contentRect.height - line.height - GUTTER_Y);

            MoveContent(offset - pos, forceUpdate);

            if (_editing)
            {
                _caret.position = textField.xy + pos;
                _caret.height = line.height > 0 ? line.height : textField.textFormat.size;

                if (_editable)
                {
                    Vector2 cursorPos = _caret.LocalToWorld(new Vector2(0, _caret.height));
                    cursorPos = StageCamera.main.WorldToScreenPoint(cursorPos);
#if !UNITY_2019_OR_NEWER
                    if (Stage.devicePixelRatio == 1)
                    {
#endif
                        cursorPos.y = Screen.height - cursorPos.y;
                        cursorPos = cursorPos / Stage.devicePixelRatio;
                        Input.compositionCursorPos = cursorPos + new Vector2(0, 20);
#if !UNITY_2019_OR_NEWER
                    }
                    else
                        Input.compositionCursorPos = cursorPos - new Vector2(0, 20);
#endif
                }

                _nextBlink = Time.time + 0.5f;
                _caret.graphics.enabled = true;

                UpdateSelection(cp);
            }
        }

        void MoveContent(Vector2 pos, bool forceUpdate)
        {
            float ox = textField.x;
            float oy = textField.y;
            float nx = pos.x;
            float ny = pos.y;
            float rectWidth = _contentRect.width - 1; //-1 to avoid cursor be clipped
            if (rectWidth - nx > textField.textWidth)
                nx = rectWidth - textField.textWidth;
            if (_contentRect.height - ny > textField.textHeight)
                ny = _contentRect.height - textField.textHeight;
            if (nx > 0)
                nx = 0;
            if (ny > 0)
                ny = 0;
            nx = (int)nx;
            ny = (int)ny;

            if (nx != ox || ny != oy || forceUpdate)
            {
                if (_caret != null)
                {
                    _caret.SetXY(nx + _caret.x - ox, ny + _caret.y - oy);
                    _selectionShape.SetXY(nx, ny);
                }
                textField.SetXY(nx, ny);

                List<HtmlElement> elements = textField.htmlElements;
                int count = elements.Count;
                for (int i = 0; i < count; i++)
                {
                    HtmlElement element = elements[i];
                    if (element.htmlObject != null)
                        element.htmlObject.SetPosition(element.position.x + nx, element.position.y + ny);
                }
            }
        }

        void UpdateSelection(TextField.CharPosition cp)
        {
            if (_selectionStart == _caretPosition)
            {
                _selectionShape.Clear();
                return;
            }

            TextField.CharPosition start;
            if (_editing && Input.compositionString.Length > 0)
            {
                if (_selectionStart < _caretPosition)
                {
                    cp = GetCharPosition(_caretPosition);
                    start = GetCharPosition(_selectionStart);
                }
                else
                    start = GetCharPosition(_selectionStart + Input.compositionString.Length);
            }
            else
                start = GetCharPosition(_selectionStart);
            if (start.charIndex > cp.charIndex)
            {
                TextField.CharPosition tmp = start;
                start = cp;
                cp = tmp;
            }

            Vector2 v1 = GetCharLocation(start);
            Vector2 v2 = GetCharLocation(cp);

            _selectionShape.rects.Clear();
            textField.GetLinesShape(start.lineIndex, v1.x, cp.lineIndex, v2.x, false, _selectionShape.rects);
            _selectionShape.Refresh();
        }

        TextField.CharPosition GetCharPosition(int caretIndex)
        {
            if (caretIndex < 0)
                caretIndex = 0;
            else if (caretIndex >= textField.charPositions.Count)
                caretIndex = textField.charPositions.Count - 1;

            return textField.charPositions[caretIndex];
        }

        /// <summary>
        /// 通过本地坐标获得字符索引位置
        /// </summary>
        /// <param name="location">本地坐标</param>
        /// <returns></returns>
        TextField.CharPosition GetCharPosition(Vector2 location)
        {
            if (textField.charPositions.Count <= 1)
                return textField.charPositions[0];

            location.x -= textField.x;
            location.y -= textField.y;

            List<TextField.LineInfo> lines = textField.lines;
            int len = lines.Count;
            TextField.LineInfo line;
            int i;
            for (i = 0; i < len; i++)
            {
                line = lines[i];
                if (line.y + line.height > location.y)
                    break;
            }
            if (i == len)
                i = len - 1;

            int lineIndex = i;

            len = textField.charPositions.Count;
            TextField.CharPosition v;
            int firstInLine = -1;
            for (i = 0; i < len; i++)
            {
                v = textField.charPositions[i];
                if (v.lineIndex == lineIndex)
                {
                    if (firstInLine == -1)
                        firstInLine = i;
                    if (v.offsetX + v.width * 0.5f > location.x)
                        return v;
                }
                else if (firstInLine != -1)
                    return v;
            }

            return textField.charPositions[i - 1];
        }

        /// <summary>
        /// 获得字符的坐标。
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        Vector2 GetCharLocation(TextField.CharPosition cp)
        {
            TextField.LineInfo line = textField.lines[cp.lineIndex];
            Vector2 pos;
            if (line.charCount == 0 || textField.charPositions.Count == 0)
            {
                if (textField.align == AlignType.Center)
                    pos.x = (int)(_contentRect.width / 2);
                else
                    pos.x = GUTTER_X;
            }
            else
            {
                TextField.CharPosition v = textField.charPositions[Math.Min(cp.charIndex, textField.charPositions.Count - 1)];
                pos.x = v.offsetX;
            }
            pos.y = line.y;
            return pos;
        }

        override internal void RefreshObjects()
        {
            base.RefreshObjects();

            if (_editing)
            {
                SetChildIndex(_selectionShape, 0);
                SetChildIndex(_caret, this.numChildren - 1);
            }

            int cnt = textField.charPositions.Count;
            if (_caretPosition >= cnt)
                _caretPosition = cnt - 1;
            if (_selectionStart >= cnt)
                _selectionStart = cnt - 1;

            UpdateCaret(true);
        }

        protected void OnChanged()
        {
            DispatchEvent("onChanged", null);

            TextInputHistory.inst.MarkChanged(this);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            Rect rect = _contentRect;
            rect.x += GUTTER_X;
            rect.y += GUTTER_Y;
            rect.width -= GUTTER_X * 2;
            rect.height -= GUTTER_Y * 2;
            this.clipRect = rect;
            ((RectHitTest)this.hitArea).rect = _contentRect;
        }

        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (_editing)
            {
                if (_nextBlink < Time.time)
                {
                    _nextBlink = Time.time + 0.5f;
                    _caret.graphics.enabled = !_caret.graphics.enabled;
                }
            }
        }

        public override void Dispose()
        {
            if ((_flags & Flags.Disposed) != 0)
                return;

            _editing = false;
            if (_caret != null)
            {
                _caret.Dispose();
                _selectionShape.Dispose();
            }

            base.Dispose();
        }

        void DoCopy(string value)
        {
            if (onCopy != null)
            {
                onCopy(this, value);
                return;
            }

#if UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
            TextEditor textEditor = new TextEditor();
#if UNITY_5_3_OR_NEWER
            textEditor.text = value;
#else
            textEditor.content = new GUIContent(value);
#endif
            textEditor.OnFocus();
            textEditor.Copy();
#endif
        }

        void DoPaste()
        {
            if (onPaste != null)
            {
                onPaste(this);
                return;
            }

#if UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
            TextEditor textEditor = new TextEditor();
#if UNITY_5_3_OR_NEWER
            textEditor.text = string.Empty;
#else
            textEditor.content = new GUIContent(string.Empty);
#endif
            textEditor.multiline = !textField.singleLine;
            textEditor.Paste();
#if UNITY_5_3_OR_NEWER
            string value = textEditor.text;
#else
            string value = textEditor.content.text;
#endif
            if (!string.IsNullOrEmpty(value))
                ReplaceSelection(value);
#endif
        }

        void CreateCaret()
        {
            _caret = new Shape();
            _caret.gameObject.name = "Caret";
            _caret.touchable = false;
            _caret._flags |= Flags.SkipBatching;
            _caret.xy = textField.xy;

            _selectionShape = new SelectionShape();
            _selectionShape.gameObject.name = "Selection";
            _selectionShape.color = UIConfig.inputHighlightColor;
            _selectionShape._flags |= Flags.SkipBatching;
            _selectionShape.touchable = false;
            _selectionShape.xy = textField.xy;
        }

        void __touchBegin(EventContext context)
        {
            if (!_editing || textField.charPositions.Count <= 1
                || keyboardInput && Stage.keyboardInput && !Stage.inst.keyboard.supportsCaret
                || context.inputEvent.button != 0)
                return;

            ClearSelection();

            Vector3 v = Stage.inst.touchPosition;
            v = this.GlobalToLocal(v);
            TextField.CharPosition cp = GetCharPosition(v);

            AdjustCaret(cp, true);

            context.CaptureTouch();
        }

        void __touchMove(EventContext context)
        {
            if (!_editing)
                return;

            Vector3 v = Stage.inst.touchPosition;
            v = this.GlobalToLocal(v);
            if (float.IsNaN(v.x))
                return;

            TextField.CharPosition cp = GetCharPosition(v);
            if (cp.charIndex != _caretPosition)
                AdjustCaret(cp);
        }

        void __mouseWheel(EventContext context)
        {
            if (_editing && mouseWheelEnabled)
            {
                context.StopPropagation();

                TextField.CharPosition cp = GetCharPosition(new Vector2(GUTTER_X, GUTTER_Y));
                int vScroll = cp.lineIndex;
                int hScroll = cp.charIndex - textField.lines[cp.lineIndex].charIndex;
                if (context.inputEvent.mouseWheelDelta < 0)
                    vScroll--;
                else
                    vScroll++;
                Scroll(hScroll, vScroll);
            }
        }

        void __focusIn(EventContext context)
        {
            if (!Application.isPlaying)
                return;

            _editing = true;
            _textBeforeEdit = _text;

            if (_caret == null)
                CreateCaret();

            if (!string.IsNullOrEmpty(_promptText))
                UpdateText();

            float caretSize;
            //如果界面缩小过，光标很容易看不见，这里放大一下
            if (UIConfig.inputCaretSize == 1 && UIContentScaler.scaleFactor < 1)
                caretSize = UIConfig.inputCaretSize / UIContentScaler.scaleFactor;
            else
                caretSize = UIConfig.inputCaretSize;
            _caret.SetSize(caretSize, textField.textFormat.size);
            _caret.DrawRect(0, Color.clear, textField.textFormat.color);
            _caret.visible = _editable;
            AddChild(_caret);

            _selectionShape.Clear();
            AddChildAt(_selectionShape, 0);

            if (!textField.Redraw())
            {
                TextField.CharPosition cp = GetCharPosition(_caretPosition);
                AdjustCaret(cp);
            }

            if (Stage.keyboardInput)
            {
                if (keyboardInput)
                {
                    Stage.inst.OpenKeyboard(_text, false, _displayAsPassword ? false : !textField.singleLine,
                        _displayAsPassword, false, null, keyboardType, hideInput);

                    SetSelection(0, -1);
                }
            }
            else
            {
                if (!disableIME && !_displayAsPassword)
                    Input.imeCompositionMode = IMECompositionMode.On;
                else
                    Input.imeCompositionMode = IMECompositionMode.Off;
                _composing = 0;

                if ((string)context.data == "key") //select all if got focus by tab key
                    SetSelection(0, -1);

                TextInputHistory.inst.StartRecord(this);
            }
        }

        void __focusOut(EventContext contxt)
        {
            if (!_editing)
                return;

            _editing = false;
            if (Stage.keyboardInput)
            {
                if (keyboardInput)
                    Stage.inst.CloseKeyboard();
            }
            else
            {
                Input.imeCompositionMode = IMECompositionMode.Auto;
                TextInputHistory.inst.StopRecord(this);
            }

            if (!string.IsNullOrEmpty(_promptText))
                UpdateText();

            _caret.RemoveFromParent();
            _selectionShape.RemoveFromParent();

            if (contextMenu != null && contextMenu.contentPane.onStage)
                contextMenu.Hide();
        }

        void __keydown(EventContext context)
        {
            if (!_editing)
                return;

            if (HandleKey(context.inputEvent))
                context.StopPropagation();
        }

        bool HandleKey(InputEvent evt)
        {
            bool keyCodeHandled = true;
            switch (evt.keyCode)
            {
                case KeyCode.Backspace:
                    {
                        if (evt.command)
                        {
                            //for mac:CMD+Backspace=Delete
                            if (_selectionStart == _caretPosition && _caretPosition < textField.charPositions.Count - 1)
                                _selectionStart = _caretPosition + 1;
                        }
                        else
                        {
                            if (_selectionStart == _caretPosition && _caretPosition > 0)
                                _selectionStart = _caretPosition - 1;
                        }
                        if (_editable)
                            ReplaceSelection(null);
                        break;
                    }

                case KeyCode.Delete:
                    {
                        if (_selectionStart == _caretPosition && _caretPosition < textField.charPositions.Count - 1)
                            _selectionStart = _caretPosition + 1;
                        if (_editable)
                            ReplaceSelection(null);
                        break;
                    }

                case KeyCode.LeftArrow:
                    {
                        if (!evt.shift)
                            ClearSelection();
                        if (_caretPosition > 0)
                        {
                            if (evt.command) //mac keyboard
                            {
                                TextField.CharPosition cp = GetCharPosition(_caretPosition);
                                TextField.LineInfo line = textField.lines[cp.lineIndex];
                                cp = GetCharPosition(new Vector2(int.MinValue, line.y + textField.y));
                                AdjustCaret(cp, !evt.shift);
                            }
                            else
                            {
                                TextField.CharPosition cp = GetCharPosition(_caretPosition - 1);
                                AdjustCaret(cp, !evt.shift);
                            }
                        }
                        break;
                    }

                case KeyCode.RightArrow:
                    {
                        if (!evt.shift)
                            ClearSelection();
                        if (_caretPosition < textField.charPositions.Count - 1)
                        {
                            if (evt.command)
                            {
                                TextField.CharPosition cp = GetCharPosition(_caretPosition);
                                TextField.LineInfo line = textField.lines[cp.lineIndex];
                                cp = GetCharPosition(new Vector2(int.MaxValue, line.y + textField.y));
                                AdjustCaret(cp, !evt.shift);
                            }
                            else
                            {
                                TextField.CharPosition cp = GetCharPosition(_caretPosition + 1);
                                AdjustCaret(cp, !evt.shift);
                            }
                        }
                        break;
                    }

                case KeyCode.UpArrow:
                    {
                        if (!evt.shift)
                            ClearSelection();

                        TextField.CharPosition cp = GetCharPosition(_caretPosition);
                        if (cp.lineIndex > 0)
                        {
                            TextField.LineInfo line = textField.lines[cp.lineIndex - 1];
                            cp = GetCharPosition(new Vector2(_caret.x, line.y + textField.y));
                            AdjustCaret(cp, !evt.shift);
                        }
                        break;
                    }

                case KeyCode.DownArrow:
                    {
                        if (!evt.shift)
                            ClearSelection();

                        TextField.CharPosition cp = GetCharPosition(_caretPosition);
                        if (cp.lineIndex == textField.lines.Count - 1)
                            cp.charIndex = textField.charPositions.Count - 1;
                        else
                        {
                            TextField.LineInfo line = textField.lines[cp.lineIndex + 1];
                            cp = GetCharPosition(new Vector2(_caret.x, line.y + textField.y));
                        }
                        AdjustCaret(cp, !evt.shift);
                        break;
                    }

                case KeyCode.PageUp:
                    {
                        ClearSelection();
                        break;
                    }

                case KeyCode.PageDown:
                    {
                        ClearSelection();
                        break;
                    }

                case KeyCode.Home:
                    {
                        if (!evt.shift)
                            ClearSelection();

                        TextField.CharPosition cp = GetCharPosition(_caretPosition);
                        TextField.LineInfo line = textField.lines[cp.lineIndex];
                        cp = GetCharPosition(new Vector2(int.MinValue, line.y + textField.y));
                        AdjustCaret(cp, !evt.shift);
                        break;
                    }

                case KeyCode.End:
                    {
                        if (!evt.shift)
                            ClearSelection();

                        TextField.CharPosition cp = GetCharPosition(_caretPosition);
                        TextField.LineInfo line = textField.lines[cp.lineIndex];
                        cp = GetCharPosition(new Vector2(int.MaxValue, line.y + textField.y));
                        AdjustCaret(cp, !evt.shift);

                        break;
                    }

                //Select All
                case KeyCode.A:
                    {
                        if (evt.ctrlOrCmd)
                        {
                            _selectionStart = 0;
                            AdjustCaret(GetCharPosition(int.MaxValue));
                        }
                        break;
                    }

                //Copy
                case KeyCode.C:
                    {
                        if (evt.ctrlOrCmd && !_displayAsPassword)
                        {
                            string s = GetSelection();
                            if (!string.IsNullOrEmpty(s))
                                DoCopy(s);
                        }
                        break;
                    }

                //Paste
                case KeyCode.V:
                    {
                        if (evt.ctrlOrCmd && _editable)
                            DoPaste();
                        break;
                    }

                //Cut
                case KeyCode.X:
                    {
                        if (evt.ctrlOrCmd && !_displayAsPassword)
                        {
                            string s = GetSelection();
                            if (!string.IsNullOrEmpty(s))
                            {
                                DoCopy(s);
                                if (_editable)
                                    ReplaceSelection(null);
                            }
                        }
                        break;
                    }

                case KeyCode.Z:
                    {
                        if (evt.ctrlOrCmd && _editable)
                        {
                            if (evt.shift)
                                TextInputHistory.inst.Redo(this);
                            else
                                TextInputHistory.inst.Undo(this);
                        }
                        break;
                    }

                case KeyCode.Y:
                    {
                        if (evt.ctrlOrCmd && _editable)
                            TextInputHistory.inst.Redo(this);
                        break;
                    }

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    {
                        if (textField.singleLine)
                        {
                            Stage.inst.focus = parent;
                            DispatchEvent("onSubmit", null);
                            DispatchEvent("onKeyDown", null); //for backward compatibility
                        }
                        break;
                    }

                case KeyCode.Tab:
                    {
                        if (textField.singleLine)
                        {
                            Stage.inst.DoKeyNavigate(evt.shift);
                            keyCodeHandled = false;
                        }
                        break;
                    }

                case KeyCode.Escape:
                    {
                        this.text = _textBeforeEdit;
                        Stage.inst.focus = parent;
                        break;
                    }

                default:
                    keyCodeHandled = (int)evt.keyCode <= 272 && !evt.ctrlOrCmd;
                    break;
            }

            char c = evt.character;
            if (c != 0)
            {
                if (evt.ctrlOrCmd)
                    return true;

                if (c == '\r' || c == 3)
                    c = '\n';

                if (c == 25)/*shift+tab*/
                    c = '\t';

                if (c == 27/*escape*/ || textField.singleLine && (c == '\n' || c == '\t'))
                    return true;

                if (char.IsHighSurrogate(c))
                {
                    _highSurrogateChar = c;
                    return true;
                }

                if (_editable)
                {
                    if (char.IsLowSurrogate(c))
                        ReplaceSelection(char.ConvertFromUtf32(((int)c & 0x03FF) + ((((int)_highSurrogateChar & 0x03FF) + 0x40) << 10)));
                    else
                        ReplaceSelection(c.ToString());
                }

                return true;
            }
            else
            {
                if (Input.compositionString.Length > 0 && _editable)
                {
                    int composing = _composing;
                    _composing = Input.compositionString.Length;

                    StringBuilder buffer = new StringBuilder();
                    GetPartialText(0, _caretPosition, buffer);
                    buffer.Append(Input.compositionString);
                    GetPartialText(_caretPosition + composing, -1, buffer);

                    textField.text = buffer.ToString();
                }

                return keyCodeHandled;
            }
        }

        internal void CheckComposition()
        {
            if (_composing != 0 && Input.compositionString.Length == 0)
                UpdateText();
        }

        void __click(EventContext context)
        {
            if (_editing && context.inputEvent.isDoubleClick)
            {
                context.StopPropagation();
                _selectionStart = 0;
                AdjustCaret(GetCharPosition(int.MaxValue));
            }
        }

        void __rightClick(EventContext context)
        {
            if (contextMenu != null)
            {
                context.StopPropagation();
                contextMenu.Show();
            }
        }
    }

    class TextInputHistory
    {
        static TextInputHistory _inst;
        public static TextInputHistory inst
        {
            get
            {
                if (_inst == null)
                    _inst = new TextInputHistory();
                return _inst;
            }
        }

        List<string> _undoBuffer;
        List<string> _redoBuffer;
        string _currentText;
        InputTextField _textField;
        bool _lock;
        int _changedFrame;

        public const int maxHistoryLength = 5;

        public TextInputHistory()
        {
            _undoBuffer = new List<string>();
            _redoBuffer = new List<string>();
        }

        public void StartRecord(InputTextField textField)
        {
            _undoBuffer.Clear();
            _redoBuffer.Clear();
            _textField = textField;
            _lock = false;
            _currentText = textField.text;
            _changedFrame = 0;
        }

        public void MarkChanged(InputTextField textField)
        {
            if (_textField != textField)
                return;

            if (_lock)
                return;

            string newText = _textField.text;
            if (_currentText == newText)
                return;

            if (_changedFrame != Time.frameCount)
            {
                _changedFrame = Time.frameCount;
                _undoBuffer.Add(_currentText);
                if (_undoBuffer.Count > maxHistoryLength)
                    _undoBuffer.RemoveAt(0);
            }
            else
            {
                int cnt = _undoBuffer.Count;
                if (cnt > 0 && newText == _undoBuffer[cnt - 1])
                    _undoBuffer.RemoveAt(cnt - 1);
            }
            _currentText = newText;
        }

        public void StopRecord(InputTextField textField)
        {
            if (_textField != textField)
                return;

            _undoBuffer.Clear();
            _redoBuffer.Clear();
            _textField = null;
            _currentText = null;
        }

        public void Undo(InputTextField textField)
        {
            if (_textField != textField)
                return;

            if (_undoBuffer.Count == 0)
                return;

            string text = _undoBuffer[_undoBuffer.Count - 1];
            _undoBuffer.RemoveAt(_undoBuffer.Count - 1);
            _redoBuffer.Add(_currentText);
            _lock = true;
            int caretPos = _textField.caretPosition;
            _textField.text = text;
            int dlen = text.Length - _currentText.Length;
            if (dlen < 0)
                _textField.caretPosition = caretPos + dlen;
            _currentText = text;
            _lock = false;
        }

        public void Redo(InputTextField textField)
        {
            if (_textField != textField)
                return;

            if (_redoBuffer.Count == 0)
                return;

            string text = _redoBuffer[_redoBuffer.Count - 1];
            _redoBuffer.RemoveAt(_redoBuffer.Count - 1);
            _undoBuffer.Add(_currentText);
            _lock = true;
            int caretPos = _textField.caretPosition;
            _textField.text = text;
            int dlen = text.Length - _currentText.Length;
            if (dlen > 0)
                _textField.caretPosition = caretPos + dlen;
            _currentText = text;
            _lock = false;
        }
    }
}
