using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class PixelHitTestData
	{
		public int pixelWidth;
		public float scale;
		public byte[] pixels;
		public int pixelsLength;
		public int pixelsOffset;

		public void Load(ByteBuffer ba)
		{
			ba.ReadInt();
			pixelWidth = ba.ReadInt();
			scale = 1.0f / ba.ReadByte();
			pixels = ba.buffer;
			pixelsLength = ba.ReadInt();
			pixelsOffset = ba.position;
			ba.Skip(pixelsLength);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class PixelHitTest : IHitTest
	{
		public int offsetX;
		public int offsetY;
		public float scaleX;
		public float scaleY;

		PixelHitTestData _data;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offsetX"></param>
		/// <param name="offsetY"></param>
		public PixelHitTest(PixelHitTestData data, int offsetX, int offsetY)
		{
			_data = data;
			this.offsetX = offsetX;
			this.offsetY = offsetY;

			scaleX = 1;
			scaleY = 1;
		}

		public void SetEnabled(bool value)
		{
		}

		public bool HitTest(Container container, ref Vector2 localPoint)
		{
			localPoint = container.GetHitTestLocalPoint();

			int x = Mathf.FloorToInt((localPoint.x / scaleX - offsetX) * _data.scale);
			int y = Mathf.FloorToInt((localPoint.y / scaleY - offsetY) * _data.scale);
			if (x < 0 || y < 0 || x >= _data.pixelWidth)
				return false;

			int pos = y * _data.pixelWidth + x;
			int pos2 = pos / 8;
			int pos3 = pos % 8;

			if (pos2 >= 0 && pos2 < _data.pixelsLength)
				return ((_data.pixels[_data.pixelsOffset + pos2] >> pos3) & 0x1) > 0;
			else
				return false;
		}
	}
}
