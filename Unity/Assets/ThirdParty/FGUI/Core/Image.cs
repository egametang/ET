using UnityEngine;
using FairyGUI.Utils;
using System;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum FlipType
	{
		None,
		Horizontal,
		Vertical,
		Both
	}

	/// <summary>
	/// 
	/// </summary>
	public class Image : DisplayObject
	{
		protected NTexture _texture;
		protected Color _color;
		protected FlipType _flip;
		protected Rect? _scale9Grid;
		protected bool _scaleByTile;
		protected int _tileGridIndice;
		protected FillMethod _fillMethod;
		protected int _fillOrigin;
		protected float _fillAmount;
		protected bool _fillClockwise;

		public Image()
		{
			Create(null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		public Image(NTexture texture)
			: base()
		{
			Create(texture);
		}

		void Create(NTexture texture)
		{
			_touchDisabled = true;
			_fillClockwise = true;

			CreateGameObject("Image");
			graphics = new NGraphics(gameObject);
			graphics.shader = ShaderConfig.imageShader;

			_color = Color.white;
			if (texture != null)
				UpdateTexture(texture);
		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture texture
		{
			get { return _texture; }
			set
			{
				UpdateTexture(value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return _color; }
			set
			{
				if (_color != value)
				{
					_color = value;
					graphics.Tint(_color);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FlipType flip
		{
			get { return _flip; }
			set
			{
				if (_flip != value)
				{
					_flip = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FillMethod fillMethod
		{
			get { return _fillMethod; }
			set
			{
				if (_fillMethod != value)
				{
					_fillMethod = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int fillOrigin
		{
			get { return _fillOrigin; }
			set
			{
				if (_fillOrigin != value)
				{
					_fillOrigin = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool fillClockwise
		{
			get { return _fillClockwise; }
			set
			{
				if (_fillClockwise != value)
				{
					_fillClockwise = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float fillAmount
		{
			get { return _fillAmount; }
			set
			{
				if (_fillAmount != value)
				{
					_fillAmount = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Rect? scale9Grid
		{
			get { return _scale9Grid; }
			set
			{
				if (_scale9Grid != value)
				{
					_scale9Grid = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool scaleByTile
		{
			get { return _scaleByTile; }
			set
			{
				if (_scaleByTile != value)
				{
					_scaleByTile = value;
					_requireUpdateMesh = true;
				}
			}
		}

		public int tileGridIndice
		{
			get { return _tileGridIndice; }
			set
			{
				if (_tileGridIndice != value)
				{
					_tileGridIndice = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void SetNativeSize()
		{
			float oldWidth = _contentRect.width;
			float oldHeight = _contentRect.height;
			if (_texture != null)
			{
				_contentRect.width = _texture.width;
				_contentRect.height = _texture.height;
			}
			else
			{
				_contentRect.width = 0;
				_contentRect.height = 0;
			}
			if (oldWidth != _contentRect.width || oldHeight != _contentRect.height)
				OnSizeChanged(true, true);
		}

		public override void Update(UpdateContext context)
		{
			if (_requireUpdateMesh)
				Rebuild();

			base.Update(context);
		}

		virtual protected void UpdateTexture(NTexture value)
		{
			if (value == _texture)
				return;

			_requireUpdateMesh = true;
			_texture = value;
			if (_contentRect.width == 0)
				SetNativeSize();
			graphics.texture = _texture;
			InvalidateBatchingState();
		}

		static int[] gridTileIndice = new int[] { -1, 0, -1, 2, 4, 3, -1, 1, -1 };
		static float[] gridX = new float[4];
		static float[] gridY = new float[4];
		static float[] gridTexX = new float[4];
		static float[] gridTexY = new float[4];

		void GenerateGrids(Rect gridRect, Rect uvRect)
		{
			float sx = uvRect.width / (float)_texture.width;
			float sy = uvRect.height / (float)_texture.height;
			gridTexX[0] = uvRect.xMin;
			gridTexX[1] = uvRect.xMin + gridRect.xMin * sx;
			gridTexX[2] = uvRect.xMin + gridRect.xMax * sx;
			gridTexX[3] = uvRect.xMax;
			gridTexY[0] = uvRect.yMax;
			gridTexY[1] = uvRect.yMax - gridRect.yMin * sy;
			gridTexY[2] = uvRect.yMax - gridRect.yMax * sy;
			gridTexY[3] = uvRect.yMin;

			if (_contentRect.width >= (_texture.width - gridRect.width))
			{
				gridX[1] = gridRect.xMin;
				gridX[2] = _contentRect.width - (_texture.width - gridRect.xMax);
				gridX[3] = _contentRect.width;
			}
			else
			{
				float tmp = gridRect.xMin / (_texture.width - gridRect.xMax);
				tmp = _contentRect.width * tmp / (1 + tmp);
				gridX[1] = tmp;
				gridX[2] = tmp;
				gridX[3] = _contentRect.width;
			}

			if (_contentRect.height >= (_texture.height - gridRect.height))
			{
				gridY[1] = gridRect.yMin;
				gridY[2] = _contentRect.height - (_texture.height - gridRect.yMax);
				gridY[3] = _contentRect.height;
			}
			else
			{
				float tmp = gridRect.yMin / (_texture.height - gridRect.yMax);
				tmp = _contentRect.height * tmp / (1 + tmp);
				gridY[1] = tmp;
				gridY[2] = tmp;
				gridY[3] = _contentRect.height;
			}
		}

		int TileFill(Rect destRect, Rect uvRect, float sourceW, float sourceH, int vertIndex)
		{
			int hc = Mathf.CeilToInt(destRect.width / sourceW);
			int vc = Mathf.CeilToInt(destRect.height / sourceH);
			float tailWidth = destRect.width - (hc - 1) * sourceW;
			float tailHeight = destRect.height - (vc - 1) * sourceH;

			if (vertIndex == -1)
			{
				graphics.Alloc(hc * vc * 4);
				vertIndex = 0;
			}

			for (int i = 0; i < hc; i++)
			{
				for (int j = 0; j < vc; j++)
				{
					graphics.FillVerts(vertIndex, new Rect(destRect.x + i * sourceW, destRect.y + j * sourceH,
							i == (hc - 1) ? tailWidth : sourceW, j == (vc - 1) ? tailHeight : sourceH));
					Rect uvTmp = uvRect;
					if (i == hc - 1)
						uvTmp.xMax = Mathf.Lerp(uvRect.xMin, uvRect.xMax, tailWidth / sourceW);
					if (j == vc - 1)
						uvTmp.yMin = Mathf.Lerp(uvRect.yMin, uvRect.yMax, 1 - tailHeight / sourceH);

					graphics.FillUV(vertIndex, uvTmp);
					vertIndex += 4;
				}
			}

			return vertIndex;
		}

		virtual protected void Rebuild()
		{
			_requireUpdateMesh = false;
			if (_texture == null)
			{
				graphics.ClearMesh();
				return;
			}

			Rect uvRect = _texture.uvRect;
			if (_flip != FlipType.None)
				ToolSet.FlipRect(ref uvRect, _flip);

			if (_fillMethod != FillMethod.None)
			{
				graphics.DrawRectWithFillMethod(_contentRect, uvRect, _color, _fillMethod, _fillAmount, _fillOrigin, _fillClockwise);
			}
			else if (_texture.width == _contentRect.width && _texture.height == _contentRect.height)
			{
				graphics.DrawRect(_contentRect, uvRect, _color);
			}
			else if (_scaleByTile)
			{
				//如果纹理是repeat模式，而且单独占满一张纹理，那么可以用repeat的模式优化显示
				if (_texture.nativeTexture != null && _texture.nativeTexture.wrapMode == TextureWrapMode.Repeat
					&& uvRect.x == 0 && uvRect.y == 0 && uvRect.width == 1 && uvRect.height == 1)
				{
					uvRect.width *= _contentRect.width / _texture.width;
					uvRect.height *= _contentRect.height / _texture.height;
					graphics.DrawRect(_contentRect, uvRect, _color);
				}
				else
				{
					TileFill(_contentRect, uvRect, _texture.width, _texture.height, -1);
					graphics.FillColors(_color);
					graphics.FillTriangles();
				}
			}
			else if (_scale9Grid != null)
			{
				Rect gridRect = (Rect)_scale9Grid;

				if (_flip != FlipType.None)
					ToolSet.FlipInnerRect(_texture.width, _texture.height, ref gridRect, _flip);

				GenerateGrids(gridRect, uvRect);

				if (_tileGridIndice == 0)
				{
					graphics.Alloc(16);

					int k = 0;
					for (int cy = 0; cy < 4; cy++)
					{
						for (int cx = 0; cx < 4; cx++)
						{
							graphics.uv[k] = new Vector2(gridTexX[cx], gridTexY[cy]);
							graphics.vertices[k] = new Vector2(gridX[cx], -gridY[cy]);
							k++;
						}
					}
					graphics.FillTriangles(NGraphics.TRIANGLES_9_GRID);
				}
				else
				{
					int hc, vc;
					Rect drawRect;
					Rect texRect;
					int row, col;
					int part;

					//先计算需要的顶点数量
					int vertCount = 0;
					for (int pi = 0; pi < 9; pi++)
					{
						col = pi % 3;
						row = pi / 3;
						part = gridTileIndice[pi];

						if (part != -1 && (_tileGridIndice & (1 << part)) != 0)
						{
							if (part == 0 || part == 1 || part == 4)
								hc = Mathf.CeilToInt((gridX[col + 1] - gridX[col]) / gridRect.width);
							else
								hc = 1;
							if (part == 2 || part == 3 || part == 4)
								vc = Mathf.CeilToInt((gridY[row + 1] - gridY[row]) / gridRect.height);
							else
								vc = 1;
							vertCount += hc * vc * 4;
						}
						else
							vertCount += 4;
					}

					graphics.Alloc(vertCount);

					int k = 0;

					for (int pi = 0; pi < 9; pi++)
					{
						col = pi % 3;
						row = pi / 3;
						part = gridTileIndice[pi];
						drawRect = Rect.MinMaxRect(gridX[col], gridY[row], gridX[col + 1], gridY[row + 1]);
						texRect = Rect.MinMaxRect(gridTexX[col], gridTexY[row + 1], gridTexX[col + 1], gridTexY[row]);

						if (part != -1 && (_tileGridIndice & (1 << part)) != 0)
						{
							k = TileFill(drawRect, texRect,
								(part == 0 || part == 1 || part == 4) ? gridRect.width : drawRect.width,
								(part == 2 || part == 3 || part == 4) ? gridRect.height : drawRect.height,
								k);
						}
						else
						{
							graphics.FillVerts(k, drawRect);
							graphics.FillUV(k, texRect);
							k += 4;
						}
					}

					graphics.FillTriangles();
				}

				graphics.FillColors(_color);
			}
			else
			{
				graphics.DrawRect(_contentRect, uvRect, _color);
			}

			if (_texture.rotated)
				NGraphics.RotateUV(graphics.uv, ref uvRect);
			graphics.UpdateMesh();
		}

		/// <summary>
		/// 截取当前图片的一部分输出到另一个Mesh。不支持图片的填充模式、九宫格的平铺模式。
		/// </summary>
		/// <param name="mesh">目标Mesh</param>
		/// <param name="localRect">制定图片的区域</param>
		public void PrintTo(Mesh mesh, Rect localRect)
		{
			if (_requireUpdateMesh)
				Rebuild();

			Rect uvRect = _texture.uvRect;
			if (_flip != FlipType.None)
				ToolSet.FlipRect(ref uvRect, _flip);

			Vector3[] verts;
			Vector2[] uv;
			Color32[] colors;
			int[] triangles;
			int vertCount = 0;

			if (!_scaleByTile || _scale9Grid == null || (_texture.width == _contentRect.width && _texture.height == _contentRect.height))
			{
				verts = new Vector3[graphics.vertices.Length];
				uv = new Vector2[graphics.uv.Length];

				Rect bound = ToolSet.Intersection(ref _contentRect, ref localRect);

				float u0 = bound.xMin / _contentRect.width;
				float u1 = bound.xMax / _contentRect.width;
				float v0 = (_contentRect.height - bound.yMax) / _contentRect.height;
				float v1 = (_contentRect.height - bound.yMin) / _contentRect.height;
				u0 = Mathf.Lerp(uvRect.xMin, uvRect.xMax, u0);
				u1 = Mathf.Lerp(uvRect.xMin, uvRect.xMax, u1);
				v0 = Mathf.Lerp(uvRect.yMin, uvRect.yMax, v0);
				v1 = Mathf.Lerp(uvRect.yMin, uvRect.yMax, v1);
				NGraphics.FillUVOfQuad(uv, 0, Rect.MinMaxRect(u0, v0, u1, v1));

				bound.x = 0;
				bound.y = 0;
				NGraphics.FillVertsOfQuad(verts, 0, bound);
				vertCount += 4;
			}
			else if (_scaleByTile)
			{
				verts = new Vector3[graphics.vertices.Length];
				uv = new Vector2[graphics.uv.Length];

				int hc = Mathf.CeilToInt(_contentRect.width / _texture.width);
				int vc = Mathf.CeilToInt(_contentRect.height / _texture.height);
				float tailWidth = _contentRect.width - (hc - 1) * _texture.width;
				float tailHeight = _contentRect.height - (vc - 1) * _texture.height;

				Vector2 offset = Vector2.zero;
				for (int i = 0; i < hc; i++)
				{
					for (int j = 0; j < vc; j++)
					{
						Rect rect = new Rect(i * _texture.width, j * _texture.height,
								i == (hc - 1) ? tailWidth : _texture.width, j == (vc - 1) ? tailHeight : _texture.height);
						Rect uvTmp = uvRect;
						if (i == hc - 1)
							uvTmp.xMax = Mathf.Lerp(uvRect.xMin, uvRect.xMax, tailWidth / _texture.width);
						if (j == vc - 1)
							uvTmp.yMin = Mathf.Lerp(uvRect.yMin, uvRect.yMax, 1 - tailHeight / _texture.height);

						Rect bound = ToolSet.Intersection(ref rect, ref localRect);
						if (bound.xMax - bound.xMin >= 0 && bound.yMax - bound.yMin > 0)
						{
							float u0 = (bound.xMin - rect.x) / rect.width;
							float u1 = (bound.xMax - rect.x) / rect.width;
							float v0 = (rect.y + rect.height - bound.yMax) / rect.height;
							float v1 = (rect.y + rect.height - bound.yMin) / rect.height;
							u0 = Mathf.Lerp(uvTmp.xMin, uvTmp.xMax, u0);
							u1 = Mathf.Lerp(uvTmp.xMin, uvTmp.xMax, u1);
							v0 = Mathf.Lerp(uvTmp.yMin, uvTmp.yMax, v0);
							v1 = Mathf.Lerp(uvTmp.yMin, uvTmp.yMax, v1);
							NGraphics.FillUVOfQuad(uv, vertCount, Rect.MinMaxRect(u0, v0, u1, v1));

							if (i == 0 && j == 0)
								offset = new Vector2(bound.x, bound.y);
							bound.x -= offset.x;
							bound.y -= offset.y;

							NGraphics.FillVertsOfQuad(verts, vertCount, bound);

							vertCount += 4;
						}
					}
				}
			}
			else
			{
				Rect gridRect = (Rect)_scale9Grid;

				if (_flip != FlipType.None)
					ToolSet.FlipInnerRect(_texture.width, _texture.height, ref gridRect, _flip);

				GenerateGrids(gridRect, uvRect);

				verts = new Vector3[36];
				uv = new Vector2[36];
				Vector2 offset = new Vector2();

				Rect drawRect;
				Rect texRect;
				int row, col;
				float u0, u1, v0, v1;

				for (int pi = 0; pi < 9; pi++)
				{
					col = pi % 3;
					row = pi / 3;
					drawRect = Rect.MinMaxRect(gridX[col], gridY[row], gridX[col + 1], gridY[row + 1]);
					texRect = Rect.MinMaxRect(gridTexX[col], gridTexY[row + 1], gridTexX[col + 1], gridTexY[row]);
					Rect bound = ToolSet.Intersection(ref drawRect, ref localRect);
					if (bound.xMax - bound.xMin >= 0 && bound.yMax - bound.yMin > 0)
					{
						u0 = (bound.xMin - drawRect.x) / drawRect.width;
						u1 = (bound.xMax - drawRect.x) / drawRect.width;
						v0 = (drawRect.yMax - bound.yMax) / drawRect.height;
						v1 = (drawRect.yMax - bound.yMin) / drawRect.height;
						u0 = Mathf.Lerp(texRect.xMin, texRect.xMax, u0);
						u1 = Mathf.Lerp(texRect.xMin, texRect.xMax, u1);
						v0 = Mathf.Lerp(texRect.yMin, texRect.yMax, v0);
						v1 = Mathf.Lerp(texRect.yMin, texRect.yMax, v1);
						NGraphics.FillUVOfQuad(uv, vertCount, Rect.MinMaxRect(u0, v0, u1, v1));

						if (vertCount == 0)
							offset = new Vector2(bound.x, bound.y);
						bound.x -= offset.x;
						bound.y -= offset.y;
						NGraphics.FillVertsOfQuad(verts, vertCount, bound);

						vertCount += 4;
					}
				}
			}

			if (vertCount != verts.Length)
			{
				Array.Resize(ref verts, vertCount);
				Array.Resize(ref uv, vertCount);
			}
			int triangleCount = (vertCount >> 1) * 3;
			triangles = new int[triangleCount];
			int k = 0;
			for (int i = 0; i < vertCount; i += 4)
			{
				triangles[k++] = i;
				triangles[k++] = i + 1;
				triangles[k++] = i + 2;

				triangles[k++] = i + 2;
				triangles[k++] = i + 3;
				triangles[k++] = i;
			}

			colors = new Color32[vertCount];
			for (int i = 0; i < vertCount; i++)
			{
				Color col = _color;
				col.a = this.alpha;
				colors[i] = col;
			}

			if (_texture.rotated)
				NGraphics.RotateUV(uv, ref uvRect);

			mesh.Clear();
			mesh.vertices = verts;
			mesh.uv = uv;
			mesh.triangles = triangles;
			mesh.colors32 = colors;
		}
	}
}
