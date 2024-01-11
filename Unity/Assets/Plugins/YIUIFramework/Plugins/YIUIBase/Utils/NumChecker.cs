namespace YIUIFramework
{
    public static class NumChecker
    {
        public static bool CheckRange(ref float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
                return false;
            }

            if (value > max)
            {
                value = max;
                return false;
            }

            return true;
        }

        public static float GetRange(float value, float min, float max)
        {
            CheckRange(ref value, min, max);
            return value;
        }
    }
}