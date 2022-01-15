namespace ET
{
    public static class MathHelper
    {
        public static float RadToDeg(float radians)
        {
            return (float)(radians * 180 / System.Math.PI);
        }
        
        public static float DegToRad(float degrees)
        {
            return (float)(degrees * System.Math.PI / 180);
        }
    }
}