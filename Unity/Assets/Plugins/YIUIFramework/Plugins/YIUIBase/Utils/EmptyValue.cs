namespace YIUIFramework
{
    /// <summary>
    /// 静态获取对应空值的类型
    /// </summary>
    public static class EmptyValue<T> where T : new()
    {
        private static T g_value;

        public static T Value
        {
            get
            {
                if (g_value == null)
                {
                    g_value = new T();
                }

                return g_value;
            }
        }
    }
}