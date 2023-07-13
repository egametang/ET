//--------------------------------------------------
// Author :  David Ochmann
// Website：https://easings.net/
//--------------------------------------------------
using System;

namespace UniFramework.Tween
{
	/// <summary>
	/// 公共补间方法
	/// </summary>
	public static class TweenEase
	{
		public static class Linear
		{
			public static float Default(float t, float b, float c, float d)
			{
				return c * t / d + b;
			}
			public static float EaseIn(float t, float b, float c, float d)
			{
				return c * t / d + b;
			}
			public static float EaseOut(float t, float b, float c, float d)
			{
				return c * t / d + b;
			}
			public static float EaseInOut(float t, float b, float c, float d)
			{
				return c * t / d + b;
			}
		}
		public static class Sine
		{
			public static float EaseIn(float t, float b, float c, float d)
			{
				return -c * (float)Math.Cos(t / d * ((float)Math.PI / 2)) + c + b;
			}
			public static float EaseOut(float t, float b, float c, float d)
			{
				return c * (float)Math.Sin(t / d * ((float)Math.PI / 2)) + b;
			}
			public static float EaseInOut(float t, float b, float c, float d)
			{
				return -c / 2f * ((float)Math.Cos((float)Math.PI * t / d) - 1) + b;
			}
		}
		public static class Quad
		{
			public static float EaseIn(float t, float b, float c, float d)
			{
				return c * (t /= d) * t + b;
			}
			public static float EaseOut(float t, float b, float c, float d)
			{
				return -c * (t /= d) * (t - 2) + b;
			}
			public static float EaseInOut(float t, float b, float c, float d)
			{
				if ((t /= d / 2) < 1) return c / 2 * t * t + b;
				return -c / 2 * ((--t) * (t - 2) - 1) + b;
			}
		}
		public static class Cubic
		{
			public static float EaseIn(float t, float b, float c, float d)
			{
				return c * (t /= d) * t * t + b;
			}
			public static float EaseOut(float t, float b, float c, float d)
			{
				return c * ((t = t / d - 1) * t * t + 1) + b;
			}
			public static float EaseInOut(float t, float b, float c, float d)
			{
				if ((t /= d / 2) < 1) return c / 2 * t * t * t + b;
				return c / 2 * ((t -= 2) * t * t + 2) + b;
			}
		}
		public static class Quart
		{
			public static float EaseIn(float t, float b, float c, float d)
			{
				return c * (t /= d) * t * t * t + b;
			}
			public static float EaseOut(float t, float b, float c, float d)
			{
				return -c * ((t = t / d - 1) * t * t * t - 1) + b;
			}
			public static float EaseInOut(float t, float b, float c, float d)
			{
				if ((t /= d / 2) < 1) return c / 2 * t * t * t * t + b;
				return -c / 2 * ((t -= 2) * t * t * t - 2) + b;
			}
		}
		public static class Quint
		{
			public static float EaseIn(float t, float b, float c, float d)
			{
				return c * (t /= d) * t * t * t * t + b;
			}
			public static float EaseOut(float t, float b, float c, float d)
			{
				return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
			}
			public static float EaseInOut(float t, float b, float c, float d)
			{
				if ((t /= d / 2) < 1) return c / 2 * t * t * t * t * t + b;
				return c / 2 * ((t -= 2) * t * t * t * t + 2) + b;
			}
		}
		public static class Expo
		{
			public static float EaseIn(float t, float b, float c, float d)
			{
				return (t == 0) ? b : c * (float)Math.Pow(2, 10 * (t / d - 1)) + b;
			}
			public static float EaseOut(float t, float b, float c, float d)
			{
				return (t == d) ? b + c : c * (-(float)Math.Pow(2, -10 * t / d) + 1) + b;
			}
			public static float EaseInOut(float t, float b, float c, float d)
			{
				if (t == 0) return b;
				if (t == d) return b + c;
				if ((t /= d / 2) < 1) return c / 2 * (float)Math.Pow(2, 10 * (t - 1)) + b;
				return c / 2 * (-(float)Math.Pow(2, -10 * --t) + 2) + b;
			}
		}
		public static class Circ
		{
			public static float EaseIn(float t, float b, float c, float d)
			{
				return -c * ((float)Math.Sqrt(1 - (t /= d) * t) - 1) + b;
			}
			public static float EaseOut(float t, float b, float c, float d)
			{
				return c * (float)Math.Sqrt(1 - (t = t / d - 1) * t) + b;
			}
			public static float EaseInOut(float t, float b, float c, float d)
			{
				if ((t /= d / 2) < 1) return -c / 2 * ((float)Math.Sqrt(1 - t * t) - 1) + b;
				return c / 2 * ((float)Math.Sqrt(1 - (t -= 2) * t) + 1) + b;
			}
		}
		public static class Back
		{
			public static float EaseIn(float t, float b, float c, float d)
			{
				float s = 1.70158f;
				return c * (t /= d) * t * ((s + 1) * t - s) + b;
			}
			public static float EaseOut(float t, float b, float c, float d)
			{
				float s = 1.70158f;
				return c * ((t = t / d - 1) * t * ((s + 1) * t + s) + 1) + b;
			}
			public static float EaseInOut(float t, float b, float c, float d)
			{
				float s = 1.70158f;
				if ((t /= d / 2) < 1) return c / 2 * (t * t * (((s *= (1.525f)) + 1) * t - s)) + b;
				return c / 2 * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2) + b;
			}
		}
		public static class Elastic
		{
			public static float EaseIn(float t, float b, float c, float d)
			{
				if (t == 0) return b; if ((t /= d) == 1) return b + c; float p = d * .3f;

				float a = c;
				float s = p / 4;

				return -(a * (float)Math.Pow(2, 10 * (t -= 1)) * (float)Math.Sin((t * d - s) * (2 * (float)Math.PI) / p)) + b;
			}
			public static float EaseOut(float t, float b, float c, float d)
			{
				if (t == 0) return b; if ((t /= d) == 1) return b + c; float p = d * .3f;

				float a = c;
				float s = p / 4;

				return (a * (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t * d - s) * (2 * (float)Math.PI) / p) + c + b);
			}
			public static float EaseInOut(float t, float b, float c, float d)
			{
				if (t == 0) return b; if ((t /= d / 2) == 2) return b + c; float p = d * (.3f * 1.5f);

				float a = c;
				float s = p / 4;

				if (t < 1) return -.5f * (a * (float)Math.Pow(2, 10 * (t -= 1)) * (float)Math.Sin((t * d - s) * (2 * (float)Math.PI) / p)) + b;
				return a * (float)Math.Pow(2, -10 * (t -= 1)) * (float)Math.Sin((t * d - s) * (2 * (float)Math.PI) / p) * .5f + c + b;
			}
		}
		public static class Bounce
		{
			public static float EaseIn(float t, float b, float c, float d)
			{
				return c - Bounce.EaseOut(d - t, 0, c, d) + b;
			}
			public static float EaseOut(float t, float b, float c, float d)
			{
				if ((t /= d) < (1 / 2.75f))
				{
					return c * (7.5625f * t * t) + b;
				}
				else if (t < (2 / 2.75f))
				{
					return c * (7.5625f * (t -= (1.5f / 2.75f)) * t + .75f) + b;
				}
				else if (t < (2.5f / 2.75f))
				{
					return c * (7.5625f * (t -= (2.25f / 2.75f)) * t + .9375f) + b;
				}
				else
				{
					return c * (7.5625f * (t -= (2.625f / 2.75f)) * t + .984375f) + b;
				}
			}
			public static float EaseInOut(float t, float b, float c, float d)
			{
				if (t < d / 2) return Bounce.EaseIn(t * 2, 0, c, d) * .5f + b;
				else return Bounce.EaseOut(t * 2 - d, 0, c, d) * .5f + c * .5f + b;
			}
		}
	}
}