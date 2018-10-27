using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class GTextInput : GTextField
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
		public InputTextField inputTextField { get; private set; }

		public GTextInput()
		{
			onFocusIn = new EventListener(this, "onFocusIn");
			onFocusOut = new EventListener(this, "onFocusOut");
			onChanged = new EventListener(this, "onChanged");
			onSubmit = new EventListener(this, "onSubmit");

			this.focusable = true;
			_textField.autoSize = AutoSizeType.None;
			_textField.wordWrap = false;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool editable
		{
			get { return inputTextField.editable; }
			set { inputTextField.editable = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool hideInput
		{
			get { return inputTextField.hideInput; }
			set { inputTextField.hideInput = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int maxLength
		{
			get { return inputTextField.maxLength; }
			set { inputTextField.maxLength = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string restrict
		{
			get { return inputTextField.restrict; }
			set { inputTextField.restrict = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool displayAsPassword
		{
			get { return inputTextField.displayAsPassword; }
			set { inputTextField.displayAsPassword = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int caretPosition
		{
			get { return inputTextField.caretPosition; }
			set { inputTextField.caretPosition = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string promptText
		{
			get { return inputTextField.promptText; }
			set { inputTextField.promptText = value; }
		}

		/// <summary>
		/// 在移动设备上是否使用键盘输入。如果false，则文本在获得焦点后不会弹出键盘。
		/// </summary>
		public bool keyboardInput
		{
			get { return inputTextField.keyboardInput; }
			set { inputTextField.keyboardInput = value; }
		}

		/// <summary>
		/// <see cref="UnityEngine.TouchScreenKeyboardType"/>
		/// </summary>
		public int keyboardType
		{
			get { return inputTextField.keyboardType; }
			set { inputTextField.keyboardType = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<uint, Emoji> emojies
		{
			get { return inputTextField.emojies; }
			set { inputTextField.emojies = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="length"></param>
		public void SetSelection(int start, int length)
		{
			inputTextField.SetSelection(start, length);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void ReplaceSelection(string value)
		{
			inputTextField.ReplaceSelection(value);
		}

		override protected void SetTextFieldText()
		{
			inputTextField.text = _text;
		}

		override protected void GetTextFieldText()
		{
			_text = inputTextField.text;
		}

		override protected void CreateDisplayObject()
		{
			inputTextField = new InputTextField();
			inputTextField.gOwner = this;
			displayObject = inputTextField;

			_textField = inputTextField.textField;
		}

		public override void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
		{
			base.Setup_BeforeAdd(buffer, beginPos);

			buffer.Seek(beginPos, 4);

			string str = buffer.ReadS();
			if (str != null)
				inputTextField.promptText = str;

			str = buffer.ReadS();
			if (str != null)
				inputTextField.restrict = str;

			int iv = buffer.ReadInt();
			if (iv != 0)
				inputTextField.maxLength = iv;
			iv = buffer.ReadInt();
			if (iv != 0)
				inputTextField.keyboardType = iv;
			if (buffer.ReadBool())
				inputTextField.displayAsPassword = true;
		}
	}
}