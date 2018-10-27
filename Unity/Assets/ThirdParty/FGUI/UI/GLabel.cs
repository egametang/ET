using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GLabel class.
	/// </summary>
	public class GLabel : GComponent, IColorGear
	{
		protected GObject _titleObject;
		protected GObject _iconObject;

		public GLabel()
		{
		}

		/// <summary>
		/// Icon of the label.
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
		/// Title of the label.
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
		/// If title is input text.
		/// </summary>
		public bool editable
		{
			get
			{
				if (_titleObject is GTextInput)
					return _titleObject.asTextInput.editable;
				else
					return false;
			}

			set
			{
				if (_titleObject is GTextInput)
					_titleObject.asTextInput.editable = value;
			}
		}

		/// <summary>
		/// Title color of the label
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
				{
					tf.color = value;
					UpdateGear(4);
				}
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
					TextFormat format = ((GTextField)_titleObject).textFormat;
					format.size = value;
					tf.textFormat = format;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return this.titleColor; }
			set { this.titleColor = value; }
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

		override protected void ConstructExtension(ByteBuffer buffer)
		{
			_titleObject = GetChild("title");
			_iconObject = GetChild("icon");
		}

		override public void Setup_AfterAdd(ByteBuffer buffer, int beginPos)
		{
			base.Setup_AfterAdd(buffer, beginPos);

			if (!buffer.Seek(beginPos, 6))
				return;

			if ((ObjectType)buffer.ReadByte() != packageItem.objectType)
				return;

			string str;
			str = buffer.ReadS();
			if (str != null)
				this.title = str;
			str = buffer.ReadS();
			if (str != null)
				this.icon = str;
			if (buffer.ReadBool())
				this.titleColor = buffer.ReadColor();
			int iv = buffer.ReadInt();
			if (iv != 0)
				this.titleFontSize = iv;

			if (buffer.ReadBool())
			{
				GTextInput input = GetTextField() as GTextInput;
				if (input != null)
				{
					str = buffer.ReadS();
					if (str != null)
						input.promptText = str;

					str = buffer.ReadS();
					if (str != null)
						input.restrict = str;

					iv = buffer.ReadInt();
					if (iv != 0)
						input.maxLength = iv;
					iv = buffer.ReadInt();
					if (iv != 0)
						input.keyboardType = iv;
					if (buffer.ReadBool())
						input.displayAsPassword = true;
				}
				else
					buffer.Skip(13);
			}
		}
	}
}
