using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GImage class.
	/// </summary>
	public class GImage : GObject, IColorGear
	{
		Image _content;

		public GImage()
		{
		}

		override protected void CreateDisplayObject()
		{
			_content = new Image();
			_content.gOwner = this;
			displayObject = _content;
		}

		/// <summary>
		/// Color of the image. 
		/// </summary>
		public Color color
		{
			get { return _content.color; }
			set
			{
				_content.color = value;
				UpdateGear(4);
			}
		}

		/// <summary>
		/// Flip type.
		/// </summary>
		/// <seealso cref="FlipType"/>
		public FlipType flip
		{
			get { return _content.flip; }
			set { _content.flip = value; }
		}

		/// <summary>
		/// Fill method.
		/// </summary>
		/// <seealso cref="FillMethod"/>
		public FillMethod fillMethod
		{
			get { return _content.fillMethod; }
			set { _content.fillMethod = value; }
		}

		/// <summary>
		/// Fill origin.
		/// </summary>
		/// <seealso cref="OriginHorizontal"/>
		/// <seealso cref="OriginVertical"/>
		/// <seealso cref="Origin90"/>
		/// <seealso cref="Origin180"/>
		/// <seealso cref="Origin360"/>
		public int fillOrigin
		{
			get { return _content.fillOrigin; }
			set { _content.fillOrigin = value; }
		}

		/// <summary>
		/// Fill clockwise if true.
		/// </summary>
		public bool fillClockwise
		{
			get { return _content.fillClockwise; }
			set { _content.fillClockwise = value; }
		}

		/// <summary>
		/// Fill amount. (0~1)
		/// </summary>
		public float fillAmount
		{
			get { return _content.fillAmount; }
			set { _content.fillAmount = value; }
		}

		/// <summary>
		/// Set texture directly. The image wont own the texture.
		/// </summary>
		public NTexture texture
		{
			get { return _content.texture; }
			set
			{
				if (value != null)
				{
					sourceWidth = value.width;
					sourceHeight = value.height;
				}
				else
				{
					sourceWidth = 0;
					sourceHeight = 0;
				}
				initWidth = sourceWidth;
				initHeight = sourceHeight;
				_content.texture = value;
			}
		}

		/// <summary>
		/// Set material.
		/// </summary>
		public Material material
		{
			get { return _content.material; }
			set { _content.material = value; }
		}

		/// <summary>
		/// Set shader.
		/// </summary>
		public string shader
		{
			get { return _content.shader; }
			set { _content.shader = value; }
		}

		override public void ConstructFromResource()
		{
			packageItem.Load();

			sourceWidth = packageItem.width;
			sourceHeight = packageItem.height;
			initWidth = sourceWidth;
			initHeight = sourceHeight;
			_content.scale9Grid = packageItem.scale9Grid;
			_content.scaleByTile = packageItem.scaleByTile;
			_content.tileGridIndice = packageItem.tileGridIndice;

			_content.texture = packageItem.texture;

			SetSize(sourceWidth, sourceHeight);
		}

		override public void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
		{
			base.Setup_BeforeAdd(buffer, beginPos);

			buffer.Seek(beginPos, 5);

			if (buffer.ReadBool())
				_content.color = buffer.ReadColor();
			_content.flip = (FlipType)buffer.ReadByte();
			_content.fillMethod = (FillMethod)buffer.ReadByte();
			if (_content.fillMethod != FillMethod.None)
			{
				_content.fillOrigin = buffer.ReadByte();
				_content.fillClockwise = buffer.ReadBool();
				_content.fillAmount = buffer.ReadFloat();
			}
		}
	}
}
