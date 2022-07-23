using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class Image : DisplayObject, IMeshFactory
    {
        protected Rect? _scale9Grid;
        protected bool _scaleByTile;
        protected Vector2 _textureScale;
        protected int _tileGridIndice;
        protected FillMesh _fillMesh;

        public Image() : this(null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        public Image(NTexture texture)
            : base()
        {
            _flags |= Flags.TouchDisabled;

            CreateGameObject("Image");
            graphics = new NGraphics(gameObject);
            graphics.shader = ShaderConfig.imageShader;
            graphics.meshFactory = this;

            _textureScale = Vector2.one;

            if (texture != null)
                UpdateTexture(texture);
        }

        /// <summary>
        /// 
        /// </summary>
        public NTexture texture
        {
            get { return graphics.texture; }
            set
            {
                UpdateTexture(value);
            }
        }

        public Vector2 textureScale
        {
            get { return _textureScale; }
            set
            {
                _textureScale = value;
                graphics.SetMeshDirty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Color color
        {
            get
            {
                return graphics.color;
            }
            set
            {
                graphics.color = value;
                graphics.Tint();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public FillMethod fillMethod
        {
            get { return _fillMesh != null ? _fillMesh.method : FillMethod.None; }
            set
            {
                if (_fillMesh == null)
                {
                    if (value == FillMethod.None)
                        return;

                    _fillMesh = new FillMesh();
                }
                if (_fillMesh.method != value)
                {
                    _fillMesh.method = value;
                    graphics.SetMeshDirty();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int fillOrigin
        {
            get { return _fillMesh != null ? _fillMesh.origin : 0; }
            set
            {
                if (_fillMesh == null)
                    _fillMesh = new FillMesh();

                if (_fillMesh.origin != value)
                {
                    _fillMesh.origin = value;
                    graphics.SetMeshDirty();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool fillClockwise
        {
            get { return _fillMesh != null ? _fillMesh.clockwise : true; }
            set
            {
                if (_fillMesh == null)
                    _fillMesh = new FillMesh();

                if (_fillMesh.clockwise != value)
                {
                    _fillMesh.clockwise = value;
                    graphics.SetMeshDirty();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float fillAmount
        {
            get { return _fillMesh != null ? _fillMesh.amount : 0; }
            set
            {
                if (_fillMesh == null)
                    _fillMesh = new FillMesh();

                if (_fillMesh.amount != value)
                {
                    _fillMesh.amount = value;
                    graphics.SetMeshDirty();
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
                    graphics.SetMeshDirty();
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
                    graphics.SetMeshDirty();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int tileGridIndice
        {
            get { return _tileGridIndice; }
            set
            {
                if (_tileGridIndice != value)
                {
                    _tileGridIndice = value;
                    graphics.SetMeshDirty();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetNativeSize()
        {
            if (graphics.texture != null)
                SetSize(graphics.texture.width, graphics.texture.height);
            else
                SetSize(0, 0);
        }

        virtual protected void UpdateTexture(NTexture value)
        {
            if (value == graphics.texture)
                return;

            graphics.texture = value;
            _textureScale = Vector2.one;
            if (_contentRect.width == 0)
                SetNativeSize();
            InvalidateBatchingState();
        }

        public void OnPopulateMesh(VertexBuffer vb)
        {
            if (_fillMesh != null && _fillMesh.method != FillMethod.None)
            {
                _fillMesh.OnPopulateMesh(vb);
            }
            else if (_scaleByTile)
            {
                NTexture texture = graphics.texture;
                if (texture.root == texture
                            && texture.nativeTexture != null
                            && texture.nativeTexture.wrapMode == TextureWrapMode.Repeat)
                {
                    Rect uvRect = vb.uvRect;
                    uvRect.width *= vb.contentRect.width / texture.width * _textureScale.x;
                    uvRect.height *= vb.contentRect.height / texture.height * _textureScale.y;

                    vb.AddQuad(vb.contentRect, vb.vertexColor, uvRect);
                    vb.AddTriangles();
                }
                else
                {
                    Rect contentRect = vb.contentRect;
                    contentRect.width *= _textureScale.x;
                    contentRect.height *= _textureScale.y;

                    TileFill(vb, contentRect, vb.uvRect, texture.width, texture.height);
                    vb.AddTriangles();
                }
            }
            else if (_scale9Grid != null)
            {
                SliceFill(vb);
            }
            else
                graphics.OnPopulateMesh(vb);
        }

        static int[] TRIANGLES_9_GRID = new int[] {
            4,0,1,1,5,4,
            5,1,2,2,6,5,
            6,2,3,3,7,6,
            8,4,5,5,9,8,
            9,5,6,6,10,9,
            10,6,7,7,11,10,
            12,8,9,9,13,12,
            13,9,10,10,14,13,
            14,10,11,
            11,15,14
        };

        static int[] gridTileIndice = new int[] { -1, 0, -1, 2, 4, 3, -1, 1, -1 };
        static float[] gridX = new float[4];
        static float[] gridY = new float[4];
        static float[] gridTexX = new float[4];
        static float[] gridTexY = new float[4];

        public void SliceFill(VertexBuffer vb)
        {
            NTexture texture = graphics.texture;
            Rect gridRect = (Rect)_scale9Grid;
            Rect contentRect = vb.contentRect;
            contentRect.width *= _textureScale.x;
            contentRect.height *= _textureScale.y;
            Rect uvRect = vb.uvRect;

            float sourceW = texture.width;
            float sourceH = texture.height;

            if (graphics.flip != FlipType.None)
            {
                if (graphics.flip == FlipType.Horizontal || graphics.flip == FlipType.Both)
                {
                    gridRect.x = sourceW - gridRect.xMax;
                    gridRect.xMax = gridRect.x + gridRect.width;
                }

                if (graphics.flip == FlipType.Vertical || graphics.flip == FlipType.Both)
                {
                    gridRect.y = sourceH - gridRect.yMax;
                    gridRect.yMax = gridRect.y + gridRect.height;
                }
            }

            float sx = uvRect.width / sourceW;
            float sy = uvRect.height / sourceH;
            float xMax = uvRect.xMax;
            float yMax = uvRect.yMax;
            float xMax2 = gridRect.xMax;
            float yMax2 = gridRect.yMax;

            gridTexX[0] = uvRect.x;
            gridTexX[1] = uvRect.x + gridRect.x * sx;
            gridTexX[2] = uvRect.x + xMax2 * sx;
            gridTexX[3] = xMax;
            gridTexY[0] = yMax;
            gridTexY[1] = yMax - gridRect.y * sy;
            gridTexY[2] = yMax - yMax2 * sy;
            gridTexY[3] = uvRect.y;

            if (contentRect.width >= (sourceW - gridRect.width))
            {
                gridX[1] = gridRect.x;
                gridX[2] = contentRect.width - (sourceW - xMax2);
                gridX[3] = contentRect.width;
            }
            else
            {
                float tmp = gridRect.x / (sourceW - xMax2);
                tmp = contentRect.width * tmp / (1 + tmp);
                gridX[1] = tmp;
                gridX[2] = tmp;
                gridX[3] = contentRect.width;
            }

            if (contentRect.height >= (sourceH - gridRect.height))
            {
                gridY[1] = gridRect.y;
                gridY[2] = contentRect.height - (sourceH - yMax2);
                gridY[3] = contentRect.height;
            }
            else
            {
                float tmp = gridRect.y / (sourceH - yMax2);
                tmp = contentRect.height * tmp / (1 + tmp);
                gridY[1] = tmp;
                gridY[2] = tmp;
                gridY[3] = contentRect.height;
            }

            if (_tileGridIndice == 0)
            {
                for (int cy = 0; cy < 4; cy++)
                {
                    for (int cx = 0; cx < 4; cx++)
                        vb.AddVert(new Vector2(gridX[cx] / _textureScale.x, gridY[cy] / _textureScale.y), vb.vertexColor, new Vector2(gridTexX[cx], gridTexY[cy]));
                }
                vb.AddTriangles(TRIANGLES_9_GRID);
            }
            else
            {
                Rect drawRect;
                Rect texRect;
                int row, col;
                int part;

                for (int pi = 0; pi < 9; pi++)
                {
                    col = pi % 3;
                    row = pi / 3;
                    part = gridTileIndice[pi];
                    drawRect = Rect.MinMaxRect(gridX[col], gridY[row], gridX[col + 1], gridY[row + 1]);
                    texRect = Rect.MinMaxRect(gridTexX[col], gridTexY[row + 1], gridTexX[col + 1], gridTexY[row]);

                    if (part != -1 && (_tileGridIndice & (1 << part)) != 0)
                    {
                        TileFill(vb, drawRect, texRect,
                            (part == 0 || part == 1 || part == 4) ? gridRect.width : drawRect.width,
                            (part == 2 || part == 3 || part == 4) ? gridRect.height : drawRect.height);
                    }
                    else
                    {
                        drawRect.x /= _textureScale.x;
                        drawRect.y /= _textureScale.y;
                        drawRect.width /= _textureScale.x;
                        drawRect.height /= _textureScale.y;

                        vb.AddQuad(drawRect, vb.vertexColor, texRect);
                    }
                }

                vb.AddTriangles();
            }
        }

        void TileFill(VertexBuffer vb, Rect contentRect, Rect uvRect, float sourceW, float sourceH)
        {
            int hc = Mathf.CeilToInt(contentRect.width / sourceW);
            int vc = Mathf.CeilToInt(contentRect.height / sourceH);
            float tailWidth = contentRect.width - (hc - 1) * sourceW;
            float tailHeight = contentRect.height - (vc - 1) * sourceH;
            float xMax = uvRect.xMax;
            float yMax = uvRect.yMax;

            for (int i = 0; i < hc; i++)
            {
                for (int j = 0; j < vc; j++)
                {
                    Rect uvTmp = uvRect;
                    if (i == hc - 1)
                        uvTmp.xMax = Mathf.Lerp(uvRect.x, xMax, tailWidth / sourceW);
                    if (j == vc - 1)
                        uvTmp.yMin = Mathf.Lerp(uvRect.y, yMax, 1 - tailHeight / sourceH);

                    Rect drawRect = new Rect(contentRect.x + i * sourceW, contentRect.y + j * sourceH,
                            i == (hc - 1) ? tailWidth : sourceW, j == (vc - 1) ? tailHeight : sourceH);

                    drawRect.x /= _textureScale.x;
                    drawRect.y /= _textureScale.y;
                    drawRect.width /= _textureScale.x;
                    drawRect.height /= _textureScale.y;

                    vb.AddQuad(drawRect, vb.vertexColor, uvTmp);
                }
            }
        }
    }
}
