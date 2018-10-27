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
		public EventListener onFocusIn { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onFocusOut { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onChanged { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onSubmit { get; private set; }

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
		public bool editable { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool hideInput { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="textField"></param>
		/// <param name="text"></param>
		public delegate void CopyHandler(InputTextField textField, string text);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="textField"></param>
		public delegate void PasteHandler(InputTextField textField);

		/// <summary>
		/// 
		/// </summary>
		public static CopyHandler onCopy;

		/// <summary>
		/// 
		/// </summary>
		public static PasteHandler onPaste;

		string _text;
		string _restrict;
		Regex _restrictPattern;
		bool _displayAsPassword;
		string _promptText;
		string _decodedPromptText;

		bool _editing;
		int _caretPosition;
		int _selectionStart;
		int _composing;
		char _highSurrogateChar;

		static Shape _caret;
		static SelectionShape _selectionShape;
		static float _nextBlink;

		const int GUTTER_X = 2;
		const int GUTTER_Y = 2;

		public InputTextField()
		{
			gameObject.name = "InputTextField";

			onFocusIn = new EventListener(this, "onFocusIn");
			onFocusOut = new EventListener(this, "onFocusOut");
			onChanged = new EventListener(this, "onChanged");
			onSubmit = new EventListener(this, "onSubmit");

			_text = string.Empty;
			maxLength = 0;
			editable = true;
			_composing = 0;
			keyboardInput = Stage.keyboardInput;

			/* 因为InputTextField定义了ClipRect，而ClipRect是四周缩进了2个像素的（GUTTER)，默认的点击测试
			 * 是使用ClipRect的，那会造成无法点击四周的空白区域。所以这里自定义了一个HitArea
			 */
			this.hitArea = new RectHitTest();
			this.touchChildren = false;

			onFocusIn.Add(__focusIn);
			onFocusOut.AddCapture(__focusOut);
			onKeyDown.AddCapture(__keydown);
			onTouchBegin.AddCapture(__touchBegin);
			onTouchMove.AddCapture(__touchMove);
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
		/// <param name="start"></param>
		/// <param name="length">-1 means the rest count from start</param>
		public void SetSelection(int start, int length)
		{
			if (!_editing)
				Stage.inst.focus = this;

			_selectionStart = start;
			_caretPosition = start + (length < 0 ? int.MaxValue : length);
			if (!textField.Redraw())
			{
				int cnt = textField.charPositions.Count;
				if (_caretPosition >= cnt)
					_caretPosition = cnt - 1;
				if (_selectionStart >= cnt)
					_selectionStart = cnt - 1;
				UpdateCaret(false);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void ReplaceSelection(string value)
		{
			if (!editable)
				throw new Exception("InputTextField is not editable.");

			if (keyboardInput && Stage.keyboardInput && !Stage.inst.keyboard.supportsCaret)
			{
				this.text = _text + value;
				onChanged.Call();
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
				newText = TruncateText(newText, maxLength);

			this.text = newText;
			onChanged.Call();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void ReplaceText(string value)
		{
			if (value == _text)
				return;

			value = ValidateInput(value);

			if (maxLength > 0)
				value = TruncateText(value, maxLength);

			this.text = value;
			onChanged.Call();
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
			int composing = _composing;
			_composing = 0;

			if (!_editing && _text.Length == 0 && !string.IsNullOrEmpty(_decodedPromptText))
				textField.htmlText = _decodedPromptText;
			else if (_displayAsPassword)
				textField.text = EncodePasswordText(_text);
			else if (Input.compositionString.Length > 0)
			{
				StringBuilder buffer = new StringBuilder();
				GetPartialText(0, _caretPosition, buffer);
				buffer.Append(Input.compositionString);
				GetPartialText(_caretPosition + composing, -1, buffer);

				_composing = Input.compositionString.Length;

				string newText = buffer.ToString();
				textField.text = newText;
			}
			else
				textField.text = _text;
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

		string GetSelection()
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

		void AdjustCaret(TextField.CharPosition cp, bool moveSelectionHeader = false)
		{
			_caretPosition = cp.charIndex;
			if (moveSelectionHeader)
				_selectionStart = _caretPosition;

			UpdateCaret(false);
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
			pos.y = line.y + textField.y;
			Vector2 newPos = pos;

			if (newPos.x < textField.textFormat.size)
				newPos.x += Math.Min(50, (int)(_contentRect.width / 2));
			else if (newPos.x > _contentRect.width - GUTTER_X - textField.textFormat.size)
				newPos.x -= Math.Min(50, (int)(_contentRect.width / 2));

			if (newPos.x < GUTTER_X)
				newPos.x = GUTTER_X;
			else if (newPos.x > _contentRect.width - GUTTER_X)
				newPos.x = Math.Max(GUTTER_X, _contentRect.width - GUTTER_X);

			if (newPos.y < GUTTER_Y)
				newPos.y = GUTTER_Y;
			else if (newPos.y + line.height >= _contentRect.height - GUTTER_Y)
				newPos.y = Math.Max(GUTTER_Y, _contentRect.height - line.height - GUTTER_Y);

			pos += MoveContent(newPos - pos, forceUpdate);

			if (_editing)
			{
				if (line.height > 0) //将光标居中
					pos.y += (int)(line.height - textField.textFormat.size) / 2;

				_caret.SetPosition(pos.x, pos.y, 0);

				Vector2 cursorPos = _caret.LocalToGlobal(new Vector2(0, _caret.height));
				Input.compositionCursorPos = cursorPos;

				_nextBlink = Time.time + 0.5f;
				_caret.graphics.enabled = true;

				UpdateSelection(cp);
			}
		}

		Vector2 MoveContent(Vector2 delta, bool forceUpdate)
		{
			float ox = textField.x;
			float oy = textField.y;
			float nx = ox + delta.x;
			float ny = oy + delta.y;
			if (_contentRect.width - nx > textField.textWidth)
				nx = _contentRect.width - textField.textWidth;
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

			delta.x = nx - ox;
			delta.y = ny - oy;
			return delta;
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

			List<Rect> rects = _selectionShape.rects;
			if (rects == null)
				rects = new List<Rect>(2);
			else
				rects.Clear();
			textField.GetLinesShape(start.lineIndex, v1.x - textField.x, cp.lineIndex, v2.x - textField.x, false, rects);
			_selectionShape.rects = rects;
			_selectionShape.xy = textField.xy;
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
					if (v.offsetX > location.x)
					{
						if (i > firstInLine)
						{
							//最后一个字符有点难点
							if (v.offsetX - location.x < 2)
								return v;
							else
								return textField.charPositions[i - 1];
						}
						else
							return textField.charPositions[firstInLine];
					}
				}
				else if (firstInLine != -1)
					break;
			}

			return textField.charPositions[i - 1];
		}

		/// <summary>
		/// 获得字符的坐标。这个坐标是容器的坐标，不是文本里的坐标。
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
				pos.x = v.offsetX - 1;
			}
			pos.x += textField.x;
			pos.y = textField.y + line.y;
			return pos;
		}

		override internal void RefreshObjects()
		{
			base.RefreshObjects();

			if (_editing)
			{
				SetChildIndex(_selectionShape, this.numChildren - 1);
				SetChildIndex(_caret, this.numChildren - 2);
			}

			int cnt = textField.charPositions.Count;
			if (_caretPosition >= cnt)
				_caretPosition = cnt - 1;
			if (_selectionStart >= cnt)
				_selectionStart = cnt - 1;

			UpdateCaret(true);
		}

		protected override void OnSizeChanged(bool widthChanged, bool heightChanged)
		{
			base.OnSizeChanged(widthChanged, heightChanged);

			Rect rect = _contentRect;
			rect.x += GUTTER_X;
			rect.y += GUTTER_Y;
			rect.width -= GUTTER_X * 2;
			//高度不减GUTTER_X * 2，因为怕高度不小心截断文字
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
			if (_disposed)
				return;

			if (_editing)
			{
				_caret.RemoveFromParent();
				_selectionShape.RemoveFromParent();
				_editing = false;
			}
			base.Dispose();
		}

		void DoCopy(string value)
		{
#if UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
			CopyPastePatch.OnCopy(this, value);
#else
			if (onCopy != null)
				onCopy(this, value);
#endif
		}

		void DoPaste()
		{
#if UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR
			CopyPastePatch.OnPaste(this);
#else
			if (onPaste != null)
				onPaste(this);
#endif
		}

		static void CreateCaret()
		{
			_caret = new Shape();
			_caret.gameObject.name = "InputCaret";
			_caret.touchable = false;
			_caret._skipInFairyBatching = true;
			_caret.graphics.dontClip = true;
			_caret.home = Stage.inst.cachedTransform;

			_selectionShape = new SelectionShape();
			_selectionShape.gameObject.name = "InputSelection";
			_selectionShape.color = UIConfig.inputHighlightColor;
			_selectionShape._skipInFairyBatching = true;
			_selectionShape.touchable = false;
			_selectionShape.home = Stage.inst.cachedTransform;
		}

		void __touchBegin(EventContext context)
		{
			if (!_editing || textField.charPositions.Count <= 1
				|| keyboardInput && Stage.keyboardInput && !Stage.inst.keyboard.supportsCaret)
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

		void __focusIn(EventContext context)
		{
			if (!editable || !Application.isPlaying)
				return;

			_editing = true;

			if (_caret == null)
				CreateCaret();

			if (!string.IsNullOrEmpty(_promptText))
				UpdateText();

			float caretSize;
			//如果界面缩小过，光标很容易看不见，这里放大一下
			if (UIConfig.inputCaretSize == 1 && GRoot.contentScaleFactor < 1)
				caretSize = (float)UIConfig.inputCaretSize / GRoot.contentScaleFactor;
			else
				caretSize = UIConfig.inputCaretSize;
			_caret.SetSize(caretSize, textField.textFormat.size);
			_caret.DrawRect(0, Color.clear, textField.textFormat.color);
			AddChild(_caret);

			_selectionShape.Clear();
			AddChild(_selectionShape);

			if (!textField.Redraw())
			{
				TextField.CharPosition cp = GetCharPosition(_caretPosition);
				AdjustCaret(cp);
			}

			if (Stage.keyboardInput)
			{
				if (keyboardInput)
					Stage.inst.OpenKeyboard(_text, false, _displayAsPassword ? false : !textField.singleLine,
						_displayAsPassword, false, null, keyboardType, hideInput);
			}
			else
			{
				Input.imeCompositionMode = IMECompositionMode.On;
				_composing = 0;
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
				Input.imeCompositionMode = IMECompositionMode.Auto;

			if (!string.IsNullOrEmpty(_promptText))
				UpdateText();

			_caret.RemoveFromParent();
			_selectionShape.RemoveFromParent();
		}

		void __keydown(EventContext context)
		{
			if (!_editing || context.isDefaultPrevented)
				return;

			InputEvent evt = context.inputEvent;

			switch (evt.keyCode)
			{
				case KeyCode.Backspace:
					{
						context.PreventDefault();
						if (_selectionStart == _caretPosition && _caretPosition > 0)
							_selectionStart = _caretPosition - 1;
						ReplaceSelection(null);
						break;
					}

				case KeyCode.Delete:
					{
						context.PreventDefault();
						if (_selectionStart == _caretPosition && _caretPosition < textField.charPositions.Count - 1)
							_selectionStart = _caretPosition + 1;
						ReplaceSelection(null);
						break;
					}

				case KeyCode.LeftArrow:
					{
						context.PreventDefault();
						if (!evt.shift)
							ClearSelection();
						if (_caretPosition > 0)
						{
							TextField.CharPosition cp = GetCharPosition(_caretPosition - 1);
							AdjustCaret(cp, !evt.shift);
						}
						break;
					}

				case KeyCode.RightArrow:
					{
						context.PreventDefault();
						if (!evt.shift)
							ClearSelection();
						if (_caretPosition < textField.charPositions.Count - 1)
						{
							TextField.CharPosition cp = GetCharPosition(_caretPosition + 1);
							AdjustCaret(cp, !evt.shift);
						}
						break;
					}

				case KeyCode.UpArrow:
					{
						context.PreventDefault();
						if (!evt.shift)
							ClearSelection();

						TextField.CharPosition cp = GetCharPosition(_caretPosition);
						if (cp.lineIndex == 0)
							return;

						TextField.LineInfo line = textField.lines[cp.lineIndex - 1];
						cp = GetCharPosition(new Vector2(_caret.x, line.y + textField.y));
						AdjustCaret(cp, !evt.shift);
						break;
					}

				case KeyCode.DownArrow:
					{
						context.PreventDefault();
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
						context.PreventDefault();
						ClearSelection();

						break;
					}

				case KeyCode.PageDown:
					{
						context.PreventDefault();
						ClearSelection();

						break;
					}

				case KeyCode.Home:
					{
						context.PreventDefault();
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
						context.PreventDefault();
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
						if (evt.ctrl)
						{
							context.PreventDefault();
							_selectionStart = 0;
							AdjustCaret(GetCharPosition(int.MaxValue));
						}
						break;
					}

				//Copy
				case KeyCode.C:
					{
						if (evt.ctrl && !_displayAsPassword)
						{
							context.PreventDefault();
							string s = GetSelection();
							if (!string.IsNullOrEmpty(s))
								DoCopy(s);
						}
						break;
					}

				//Paste
				case KeyCode.V:
					{
						if (evt.ctrl)
						{
							context.PreventDefault();
							DoPaste();
						}
						break;
					}

				//Cut
				case KeyCode.X:
					{
						if (evt.ctrl && !_displayAsPassword)
						{
							context.PreventDefault();
							string s = GetSelection();
							if (!string.IsNullOrEmpty(s))
							{
								DoCopy(s);
								ReplaceSelection(null);
							}
						}
						break;
					}

				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					{
						if (textField.singleLine)
						{
							onSubmit.Call();
							return;
						}
						break;
					}
			}

			char c = evt.character;
			if (c != 0)
			{
				if (evt.ctrl)
					return;

				if (c == '\r' || (int)c == 3)
					c = '\n';

				if (c == 127 || textField.singleLine && c == '\n')
					return;

				if (char.IsHighSurrogate(c))
				{
					_highSurrogateChar = c;
					return;
				}

				if (char.IsLowSurrogate(c))
					ReplaceSelection(char.ConvertFromUtf32(((int)c & 0x03FF) + ((((int)_highSurrogateChar & 0x03FF) + 0x40) << 10)));
				else
					ReplaceSelection(c.ToString());
			}
			else
			{
				if (Input.compositionString.Length > 0)
					UpdateText();
			}
		}

		internal void CheckComposition()
		{
			if (_composing != 0 && Input.compositionString.Length == 0)
				UpdateText();
		}
	}
}
