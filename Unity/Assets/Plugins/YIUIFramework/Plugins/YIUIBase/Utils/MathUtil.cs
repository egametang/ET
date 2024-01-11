using System;

namespace YIUIFramework
{
    public class MathUtil
    {
        private static double PrecisionIntercept(double input)
        {
            return (int)(input * 1000.0) / 1000.0;
        }

        public const double PI = (int)(Math.PI * 1000.0) / 1000.0;

        /// <summary>
        /// 2 * pi
        /// </summary>
        public const double PI2 = PI * 2f;

        /// <summary>
        /// 弧度转角度
        /// </summary>
        public const double R2A = 180 / PI;

        /// <summary>
        /// 角度转弧度
        /// </summary>
        public const double A2R = PI / 180;

        /// <summary>
        /// 跟号2
        /// </summary>
        public static double Sqrt2 = Math.Sqrt(2);

        public static float Clamp(float value, float max, float min)
        {
            if (value < min)
            {
                value = min;
            }

            if (value > max)
            {
                value = max;
            }

            return value;
        }

        public static int Clamp(int value, int max, int min)
        {
            if (value < min)
            {
                value = min;
            }

            if (value > max)
            {
                value = max;
            }

            return value;
        }

        public static float DistanceNoSqrt(float ax, float ay, float bx, float by)
        {
            float dx = ax - bx;
            float dy = ay - by;
            return dx * dx + dy * dy;
        }

        public static float Distance(float ax, float ay, float bx, float by)
        {
            float dx = ax - bx;
            float dy = ay - by;
            return (float)PrecisionIntercept(Math.Sqrt(dx * dx + dy * dy));
        }

        public static double DistanceTo(double ax, double ay, double bx, double by)
        {
            double dx = ax - bx;
            double dy = ay - by;
            return PrecisionIntercept(Math.Sqrt(dx * dx + dy * dy));
        }

        public static int DistanceTo(int ax, int ay, int bx, int by)
        {
            int dx = ax - bx;
            int dy = ay - by;
            return (int)Math.Sqrt(dx * dx + dy * dy);
        }

        public static int CalcuIndexByXY(int x, int y, int width)
        {
            return y * width + x;
        }

        public static float Fract(float x)
        {
            return x - (int)PrecisionIntercept(Math.Floor(x));
        }

        /// <summary>
        /// 判断值是否在min and max之间（包含min and max）
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool IsRange(int value, int min, int max)
        {
            return !(value < min || value > max);
        }

        public static bool Range(ref float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
                return true;
            }
            else if (value > max)
            {
                value = max;
                return true;
            }

            return false;
        }

        public static bool Range(ref int value, int min, int max)
        {
            if (value < min)
            {
                value = min;
                return true;
            }
            else if (value > max)
            {
                value = max;
                return true;
            }

            return false;
        }

        public static bool Range(ref double value, double min, double max)
        {
            if (value < min)
            {
                value = min;
                return true;
            }
            else if (value > max)
            {
                value = max;
                return true;
            }

            return false;
        }

        public static int FixedByRange(int value, int min, int max)
        {
            Range(ref value, min, max);
            return value;
        }

        public static float FixedByRange(float value, float min, float max)
        {
            Range(ref value, min, max);
            return value;
        }

        public static double Sqrt(double d)
        {
            return PrecisionIntercept(Math.Sqrt(d));
        }

        public static double Floor(double d)
        {
            return PrecisionIntercept(Math.Floor(d));
        }

        public static double Atan2(double y, double x)
        {
            return PrecisionIntercept(Math.Atan2(y, x));
        }

        public static double Sin(double a)
        {
            return PrecisionIntercept(Math.Sin(a));
        }

        public static double Cos(double d)
        {
            return PrecisionIntercept(Math.Cos(d));
        }

        public static double Pow(double x, double y)
        {
            return PrecisionIntercept(Math.Pow(x, y));
        }

        public static double Ceiling(double d)
        {
            return PrecisionIntercept(Math.Ceiling(d));
        }
    }
}