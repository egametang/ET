using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum FillMethod
	{
		None = 0,

		/// <summary>
		/// The Image will be filled Horizontally
		/// </summary>
		Horizontal = 1,

		/// <summary>
		/// The Image will be filled Vertically.
		/// </summary>
		Vertical = 2,

		/// <summary>
		/// The Image will be filled Radially with the radial center in one of the corners.
		/// </summary>
		Radial90 = 3,

		/// <summary>
		/// The Image will be filled Radially with the radial center in one of the edges.
		/// </summary>
		Radial180 = 4,

		/// <summary>
		/// The Image will be filled Radially with the radial center at the center.
		/// </summary>
		Radial360 = 5,
	}

	/// <summary>
	/// 
	/// </summary>
	public enum OriginHorizontal
	{
		Left,
		Right,
	}

	/// <summary>
	/// 
	/// </summary>
	public enum OriginVertical
	{
		Top,
		Bottom
	}

	/// <summary>
	/// 
	/// </summary>
	public enum Origin90
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	/// <summary>
	/// 
	/// </summary>
	public enum Origin180
	{
		Top,
		Bottom,
		Left,
		Right
	}

	/// <summary>
	/// 
	/// </summary>
	public enum Origin360
	{
		Top,
		Bottom,
		Left,
		Right
	}

	/// <summary>
	/// 
	/// </summary>
	public class FillUtils
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="amount"></param>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="verts"></param>
		/// <param name="uv"></param>
		public static void FillHorizontal(OriginHorizontal origin, float amount, Rect vertRect, Rect uvRect, Vector3[] verts, Vector2[] uv)
		{
			if (origin == OriginHorizontal.Left)
			{
				vertRect.width = vertRect.width * amount;
				uvRect.width = uvRect.width * amount;
			}
			else
			{
				vertRect.x += vertRect.width * (1 - amount);
				vertRect.width = vertRect.width * amount;
				uvRect.x += uvRect.width * (1 - amount);
				uvRect.width = uvRect.width * amount;
			}

			NGraphics.FillVertsOfQuad(verts, 0, vertRect);
			NGraphics.FillUVOfQuad(uv, 0, uvRect);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="amount"></param>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="verts"></param>
		/// <param name="uv"></param>
		public static void FillVertical(OriginVertical origin, float amount, Rect vertRect, Rect uvRect, Vector3[] verts, Vector2[] uv)
		{
			if (origin == OriginVertical.Bottom)
			{
				vertRect.y += vertRect.height * (1 - amount);
				vertRect.height = vertRect.height * amount;
				uvRect.height = uvRect.height * amount;
			}
			else
			{
				vertRect.height = vertRect.height * amount;
				uvRect.y += uvRect.height * (1 - amount);
				uvRect.height = uvRect.height * amount;
			}

			NGraphics.FillVertsOfQuad(verts, 0, vertRect);
			NGraphics.FillUVOfQuad(uv, 0, uvRect);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="amount"></param>
		/// <param name="clockwise"></param>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="verts"></param>
		/// <param name="uv"></param>
		public static void FillRadial90(Origin90 origin, float amount, bool clockwise, Rect vertRect, Rect uvRect, Vector3[] verts, Vector2[] uv)
		{
			NGraphics.FillVertsOfQuad(verts, 0, vertRect);
			NGraphics.FillUVOfQuad(uv, 0, uvRect);
			if (amount < 0.001f)
			{
				verts[0] = verts[1] = verts[2] = verts[3];
				uv[0] = uv[1] = uv[2] = uv[3];
				return;
			}
			if (amount > 0.999f)
				return;

			switch (origin)
			{
				case Origin90.BottomLeft:
					{
						if (clockwise)
						{
							float v = Mathf.Tan(Mathf.PI / 2 * (1 - amount));
							float h = vertRect.width * v;
							if (h > vertRect.height)
							{
								float ratio = (h - vertRect.height) / h;
								verts[2].x -= vertRect.width * ratio;
								verts[3] = verts[2];

								uv[2].x -= uvRect.width * ratio;
								uv[3] = uv[2];
							}
							else
							{
								float ratio = h / vertRect.height;
								verts[3].y += h;
								uv[3].y += uvRect.height * ratio;
							}
						}
						else
						{
							float v = Mathf.Tan(Mathf.PI / 2 * amount);
							float h = vertRect.width * v;
							if (h > vertRect.height)
							{
								float ratio = (h - vertRect.height) / h;
								verts[1].x += vertRect.width * (1 - ratio);
								uv[1].x += uvRect.width * (1 - ratio);
							}
							else
							{
								float ratio = h / vertRect.height;
								verts[2].y -= vertRect.height * (1 - ratio);
								verts[1] = verts[2];

								uv[2].y -= uvRect.height * (1 - ratio);
								uv[1] = uv[2];
							}
						}
					}
					break;

				case Origin90.BottomRight:
					{
						if (clockwise)
						{
							float v = Mathf.Tan(Mathf.PI / 2 * amount);
							float h = vertRect.width * v;
							if (h > vertRect.height)
							{
								float ratio = (h - vertRect.height) / h;
								verts[2].x -= vertRect.width * (1 - ratio);
								uv[2].x -= uvRect.width * (1 - ratio);
							}
							else
							{
								float ratio = h / vertRect.height;
								verts[1].y -= vertRect.height * (1 - ratio);
								verts[2] = verts[3];

								uv[1].y -= uvRect.height * (1 - ratio);
								uv[2] = uv[3];
							}
						}
						else
						{
							float v = Mathf.Tan(Mathf.PI / 2 * (1 - amount));
							float h = vertRect.width * v;
							if (h > vertRect.height)
							{
								float ratio = (h - vertRect.height) / h;
								verts[1].x += vertRect.width * ratio;
								verts[0] = verts[1];

								uv[1].x += uvRect.width * ratio;
								uv[0] = uv[1];
							}
							else
							{
								float ratio = h / vertRect.height;
								verts[0].y += h;
								uv[0].y += uvRect.height * ratio;
							}
						}
					}
					break;

				case Origin90.TopLeft:
					{
						if (clockwise)
						{
							float v = Mathf.Tan(Mathf.PI / 2 * amount);
							float h = vertRect.width * v;
							if (h > vertRect.height)
							{
								float ratio = (h - vertRect.height) / h;
								verts[0].x += vertRect.width * (1 - ratio);
								uv[0].x += uvRect.width * (1 - ratio);
							}
							else
							{
								float ratio = h / vertRect.height;
								verts[3].y += vertRect.height * (1 - ratio);
								verts[0] = verts[3];

								uv[3].y += uvRect.height * (1 - ratio);
								uv[0] = uv[3];
							}
						}
						else
						{
							float v = Mathf.Tan(Mathf.PI / 2 * (1 - amount));
							float h = vertRect.width * v;
							if (h > vertRect.height)
							{
								float ratio = (h - vertRect.height) / h;
								verts[3].x -= vertRect.width * ratio;
								verts[2] = verts[3];
								uv[3].x -= uvRect.width * ratio;
								uv[2] = uv[3];
							}
							else
							{
								float ratio = h / vertRect.height;
								verts[2].y -= h;
								uv[2].y -= uvRect.height * ratio;
							}
						}
					}
					break;

				case Origin90.TopRight:
					{
						if (clockwise)
						{
							float v = Mathf.Tan(Mathf.PI / 2 * (1 - amount));
							float h = vertRect.width * v;
							if (h > vertRect.height)
							{
								float ratio = (h - vertRect.height) / h;
								verts[0].x += vertRect.width * ratio;
								verts[1] = verts[2];
								uv[0].x += uvRect.width * ratio;
								uv[1] = uv[2];
							}
							else
							{
								float ratio = h / vertRect.height;
								verts[1].y -= vertRect.height * ratio;
								uv[1].y -= uvRect.height * ratio;
							}
						}
						else
						{
							float v = Mathf.Tan(Mathf.PI / 2 * amount);
							float h = vertRect.width * v;
							if (h > vertRect.height)
							{
								float ratio = (h - vertRect.height) / h;
								verts[3].x -= vertRect.width * (1 - ratio);
								uv[3].x -= uvRect.width * (1 - ratio);
							}
							else
							{
								float ratio = h / vertRect.height;
								verts[0].y += vertRect.height * (1 - ratio);
								verts[3] = verts[0];
								uv[0].y += uvRect.height * (1 - ratio);
								uv[3] = uv[0];
							}
						}
					}
					break;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="amount"></param>
		/// <param name="clockwise"></param>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="verts"></param>
		/// <param name="uv"></param>
		public static void FillRadial180(Origin180 origin, float amount, bool clockwise, Rect vertRect, Rect uvRect, Vector3[] verts, Vector2[] uv)
		{
			switch (origin)
			{
				case Origin180.Top:
					if (amount <= 0.5f)
					{
						vertRect.width /= 2;
						uvRect.width /= 2;
						if (clockwise)
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						amount = amount / 0.5f;
						FillRadial90(clockwise ? Origin90.TopLeft : Origin90.TopRight, amount, clockwise, vertRect, uvRect, verts, uv);
						verts[4] = verts[5] = verts[6] = verts[7] = verts[0];
						uv[4] = uv[5] = uv[6] = uv[7] = uv[0];
					}
					else
					{
						vertRect.width /= 2;
						uvRect.width /= 2;
						if (!clockwise)
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						amount = (amount - 0.5f) / 0.5f;
						FillRadial90(clockwise ? Origin90.TopRight : Origin90.TopLeft, amount, clockwise, vertRect, uvRect, verts, uv);

						if (clockwise)
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						else
						{
							vertRect.x -= vertRect.width;
							uvRect.x -= uvRect.width;
						}
						NGraphics.FillVertsOfQuad(verts, 4, vertRect);
						NGraphics.FillUVOfQuad(uv, 4, uvRect);
					}
					break;

				case Origin180.Bottom:
					if (amount <= 0.5f)
					{
						vertRect.width /= 2;
						uvRect.width /= 2;
						if (!clockwise)
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						amount = amount / 0.5f;
						FillRadial90(clockwise ? Origin90.BottomRight : Origin90.BottomLeft, amount, clockwise, vertRect, uvRect, verts, uv);
						verts[4] = verts[5] = verts[6] = verts[7] = verts[0];
						uv[4] = uv[5] = uv[6] = uv[7] = uv[0];
					}
					else
					{
						vertRect.width /= 2;
						uvRect.width /= 2;
						if (clockwise)
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						amount = (amount - 0.5f) / 0.5f;
						FillRadial90(clockwise ? Origin90.BottomLeft : Origin90.BottomRight, amount, clockwise, vertRect, uvRect, verts, uv);

						if (clockwise)
						{
							vertRect.x -= vertRect.width;
							uvRect.x -= uvRect.width;
						}
						else
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						NGraphics.FillVertsOfQuad(verts, 4, vertRect);
						NGraphics.FillUVOfQuad(uv, 4, uvRect);
					}
					break;

				case Origin180.Left:
					if (amount <= 0.5f)
					{
						if (clockwise)
						{
							vertRect.height /= 2;
							uvRect.yMin += uvRect.height / 2;
						}
						else
						{
							vertRect.yMin += vertRect.height / 2;
							uvRect.yMax -= uvRect.height / 2;
						}
						amount = amount / 0.5f;
						FillRadial90(clockwise ? Origin90.BottomLeft : Origin90.TopLeft, amount, clockwise, vertRect, uvRect, verts, uv);
						verts[4] = verts[5] = verts[6] = verts[7] = verts[0];
						uv[4] = uv[5] = uv[6] = uv[7] = uv[0];
					}
					else
					{
						if (clockwise)
						{
							vertRect.yMin += vertRect.height / 2;
							uvRect.yMax -= uvRect.height / 2;
						}
						else
						{
							vertRect.height /= 2;
							uvRect.yMin += uvRect.height / 2;
						}
						amount = (amount - 0.5f) / 0.5f;
						FillRadial90(clockwise ? Origin90.TopLeft : Origin90.BottomLeft, amount, clockwise, vertRect, uvRect, verts, uv);

						if (clockwise)
						{
							vertRect.y -= vertRect.height;
							uvRect.y += uvRect.height;
						}
						else
						{
							vertRect.y += vertRect.height;
							uvRect.y -= uvRect.height;
						}
						NGraphics.FillVertsOfQuad(verts, 4, vertRect);
						NGraphics.FillUVOfQuad(uv, 4, uvRect);
					}
					break;

				case Origin180.Right:
					if (amount <= 0.5f)
					{
						if (clockwise)
						{
							vertRect.yMin += vertRect.height / 2;
							uvRect.yMax -= uvRect.height / 2;
						}
						else
						{
							vertRect.height /= 2;
							uvRect.yMin += uvRect.height / 2;
						}
						amount = amount / 0.5f;
						FillRadial90(clockwise ? Origin90.TopRight : Origin90.BottomRight, amount, clockwise, vertRect, uvRect, verts, uv);
						verts[4] = verts[5] = verts[6] = verts[7] = verts[0];
						uv[4] = uv[5] = uv[6] = uv[7] = uv[0];
					}
					else
					{
						if (clockwise)
						{
							vertRect.height /= 2;
							uvRect.yMin += uvRect.height / 2;
						}
						else
						{
							vertRect.yMin += vertRect.height / 2;
							uvRect.yMax -= uvRect.height / 2;
						}
						amount = (amount - 0.5f) / 0.5f;
						FillRadial90(clockwise ? Origin90.BottomRight : Origin90.TopRight, amount, clockwise, vertRect, uvRect, verts, uv);

						if (clockwise)
						{
							vertRect.y += vertRect.height;
							uvRect.y -= uvRect.height;
						}
						else
						{
							vertRect.y -= vertRect.height;
							uvRect.y += uvRect.height;
						}
						NGraphics.FillVertsOfQuad(verts, 4, vertRect);
						NGraphics.FillUVOfQuad(uv, 4, uvRect);
					}
					break;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="amount"></param>
		/// <param name="clockwise"></param>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="verts"></param>
		/// <param name="uv"></param>
		public static void FillRadial360(Origin360 origin, float amount, bool clockwise, Rect vertRect, Rect uvRect, Vector3[] verts, Vector2[] uv)
		{
			switch (origin)
			{
				case Origin360.Top:
					if (amount < 0.5f)
					{
						amount = amount / 0.5f;
						vertRect.width /= 2;
						uvRect.width /= 2;
						if (clockwise)
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						FillRadial180(clockwise ? Origin180.Left : Origin180.Right, amount, clockwise, vertRect, uvRect, verts, uv);
						verts[8] = verts[9] = verts[10] = verts[11] = verts[7];
						uv[8] = uv[9] = uv[10] = uv[11] = uv[7];
					}
					else
					{
						vertRect.width /= 2;
						uvRect.width /= 2;
						if (!clockwise)
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						amount = (amount - 0.5f) / 0.5f;
						FillRadial180(clockwise ? Origin180.Right : Origin180.Left, amount, clockwise, vertRect, uvRect, verts, uv);

						if (clockwise)
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						else
						{
							vertRect.x -= vertRect.width;
							uvRect.x -= uvRect.width;
						}
						NGraphics.FillVertsOfQuad(verts, 8, vertRect);
						NGraphics.FillUVOfQuad(uv, 8, uvRect);
					}
					break;

				case Origin360.Bottom:
					if (amount < 0.5f)
					{
						amount = amount / 0.5f;
						vertRect.width /= 2;
						uvRect.width /= 2;
						if (!clockwise)
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						FillRadial180(clockwise ? Origin180.Right : Origin180.Left, amount, clockwise, vertRect, uvRect, verts, uv);
						verts[8] = verts[9] = verts[10] = verts[11] = verts[7];
						uv[8] = uv[9] = uv[10] = uv[11] = uv[7];
					}
					else
					{
						vertRect.width /= 2;
						uvRect.width /= 2;
						if (clockwise)
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						amount = (amount - 0.5f) / 0.5f;
						FillRadial180(clockwise ? Origin180.Left : Origin180.Right, amount, clockwise, vertRect, uvRect, verts, uv);

						if (clockwise)
						{
							vertRect.x -= vertRect.width;
							uvRect.x -= uvRect.width;
						}
						else
						{
							vertRect.x += vertRect.width;
							uvRect.x += uvRect.width;
						}
						NGraphics.FillVertsOfQuad(verts, 8, vertRect);
						NGraphics.FillUVOfQuad(uv, 8, uvRect);
					}
					break;

				case Origin360.Left:
					if (amount < 0.5f)
					{
						amount = amount / 0.5f;
						if (clockwise)
						{
							vertRect.height /= 2;
							uvRect.yMin += uvRect.height / 2;
						}
						else
						{
							vertRect.yMin += vertRect.height / 2;
							uvRect.yMax -= uvRect.height / 2;
						}
						FillRadial180(clockwise ? Origin180.Bottom : Origin180.Top, amount, clockwise, vertRect, uvRect, verts, uv);
						verts[8] = verts[9] = verts[10] = verts[11] = verts[7];
						uv[8] = uv[9] = uv[10] = uv[11] = uv[7];
					}
					else
					{
						if (clockwise)
						{
							vertRect.yMin += vertRect.height / 2;
							uvRect.yMax -= uvRect.height / 2;
						}
						else
						{
							vertRect.height /= 2;
							uvRect.yMin += uvRect.height / 2;
						}
						amount = (amount - 0.5f) / 0.5f;
						FillRadial180(clockwise ? Origin180.Top : Origin180.Bottom, amount, clockwise, vertRect, uvRect, verts, uv);

						if (clockwise)
						{
							vertRect.y -= vertRect.height;
							uvRect.y += uvRect.height;
						}
						else
						{
							vertRect.y += vertRect.height;
							uvRect.y -= uvRect.height;
						}
						NGraphics.FillVertsOfQuad(verts, 8, vertRect);
						NGraphics.FillUVOfQuad(uv, 8, uvRect);
					}
					break;

				case Origin360.Right:
					if (amount < 0.5f)
					{
						if (clockwise)
						{
							vertRect.yMin += vertRect.height / 2;
							uvRect.yMax -= uvRect.height / 2;
						}
						else
						{
							vertRect.height /= 2;
							uvRect.yMin += uvRect.height / 2;
						}
						amount = amount / 0.5f;
						FillRadial180(clockwise ? Origin180.Top : Origin180.Bottom, amount, clockwise, vertRect, uvRect, verts, uv);
						verts[8] = verts[9] = verts[10] = verts[11] = verts[7];
						uv[8] = uv[9] = uv[10] = uv[11] = uv[7];
					}
					else
					{
						if (clockwise)
						{
							vertRect.height /= 2;
							uvRect.yMin += uvRect.height / 2;
						}
						else
						{
							vertRect.yMin += vertRect.height / 2;
							uvRect.yMax -= uvRect.height / 2;
						}

						amount = (amount - 0.5f) / 0.5f;
						FillRadial180(clockwise ? Origin180.Bottom : Origin180.Top, amount, clockwise, vertRect, uvRect, verts, uv);

						if (clockwise)
						{
							vertRect.y += vertRect.height;
							uvRect.y -= uvRect.height;
						}
						else
						{
							vertRect.y -= vertRect.height;
							uvRect.y += uvRect.height;
						}
						NGraphics.FillVertsOfQuad(verts, 8, vertRect);
						NGraphics.FillUVOfQuad(uv, 8, uvRect);
					}
					break;
			}
		}
	}
}
