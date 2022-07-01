namespace ET
{
    public static class PoolHelper
    {
        public static T Fetch<T>(this MonoPool monoPool) where T: class
        {
            return monoPool.Fetch(typeof (T)) as T;
        }
    }
}