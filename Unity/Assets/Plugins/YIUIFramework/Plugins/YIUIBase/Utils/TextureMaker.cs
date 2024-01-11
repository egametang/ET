using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// The texture maker used to build some simple texture used in program.
    /// </summary>
    public static class TextureMaker
    {
        /// <summary>
        /// Create a gray texture with specify gray.
        /// </summary>
        public static Texture2D Gray(float gray)
        {
            return Monochromatic(new Color(gray, gray, gray, 0.5f));
        }

        /// <summary>
        /// Create a gray texture with specify gray.
        /// </summary>
        public static Texture2D Gray(int size, float gray)
        {
            return Monochromatic(size, new Color(gray, gray, gray, 0.5f));
        }

        /// <summary>
        /// Create a monochromatic texture with specify color.
        /// </summary>
        public static Texture2D Monochromatic(Color color)
        {
            var texture = new Texture2D(
                1, 1, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode   = TextureWrapMode.Clamp;
            texture.SetPixel(0, 0, color);
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Create a dot texture with specify size and color.
        /// </summary>
        public static Texture2D Monochromatic(int size, Color color)
        {
            var texture = new Texture2D(
                size, size, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode   = TextureWrapMode.Clamp;

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    texture.SetPixel(i, j, color);
                }
            }

            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Create a dot texture with specify size and color.
        /// </summary>
        public static Texture2D Dot(int size, Color fg, Color bg)
        {
            var texture = new Texture2D(
                size, size, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode   = TextureWrapMode.Clamp;

            var radius       = size / 2;
            var squareRadius = (size / 2) * (size / 2);
            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    var x = i - radius;
                    var y = j - radius;
                    if ((x * x) + (y * y) < squareRadius)
                    {
                        texture.SetPixel(i, j, fg);
                    }
                    else
                    {
                        texture.SetPixel(i, j, bg);
                    }
                }
            }

            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Create the top left corner texture with specify size and color.
        /// </summary>
        public static Texture2D CornerTopLeft(int size, Color fg, Color bg)
        {
            var texture = new Texture2D(
                size, size, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode   = TextureWrapMode.Clamp;

            var halfsize = size / 2;
            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    if (i < halfsize && j > halfsize)
                    {
                        texture.SetPixel(i, j, fg);
                    }
                    else
                    {
                        texture.SetPixel(i, j, bg);
                    }
                }
            }

            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Create the top right corner texture with specify size and color.
        /// </summary>
        public static Texture2D CornerTopRight(int size, Color fg, Color bg)
        {
            var texture = new Texture2D(
                size, size, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode   = TextureWrapMode.Clamp;

            var halfsize = size / 2;
            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    if (i > halfsize && j > halfsize)
                    {
                        texture.SetPixel(i, j, fg);
                    }
                    else
                    {
                        texture.SetPixel(i, j, bg);
                    }
                }
            }

            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Create the bottom left corner texture with specify size and color.
        /// </summary>
        public static Texture2D CornerBottomLeft(int size, Color fg, Color bg)
        {
            var texture = new Texture2D(
                size, size, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode   = TextureWrapMode.Clamp;

            var halfsize = size / 2;
            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    if (i < halfsize && j < halfsize)
                    {
                        texture.SetPixel(i, j, fg);
                    }
                    else
                    {
                        texture.SetPixel(i, j, bg);
                    }
                }
            }

            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Create the bottom right corner texture with specify size and color.
        /// </summary>
        public static Texture2D CornerBottomRight(int size, Color fg, Color bg)
        {
            var texture = new Texture2D(
                size, size, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode   = TextureWrapMode.Clamp;

            var halfsize = size / 2;
            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    if (i > halfsize && j < halfsize)
                    {
                        texture.SetPixel(i, j, fg);
                    }
                    else
                    {
                        texture.SetPixel(i, j, bg);
                    }
                }
            }

            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Create a cross texture with specify size and color.
        /// </summary>
        public static Texture2D Cross(
            int size, int thickness, Color fg, Color bg)
        {
            var texture = new Texture2D(
                size, size, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode   = TextureWrapMode.Clamp;

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    if (Mathf.Abs(i - j) <= thickness ||
                        Mathf.Abs(i + j - size) <= thickness)
                    {
                        texture.SetPixel(i, j, fg);
                    }
                    else
                    {
                        texture.SetPixel(i, j, bg);
                    }
                }
            }

            texture.Apply();

            return texture;
        }
    }
}