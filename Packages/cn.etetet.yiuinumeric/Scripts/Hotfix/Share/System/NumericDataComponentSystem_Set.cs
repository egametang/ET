namespace ET
{
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataComponentSystem
    {
        #region Set

        /*
         * 少量的 Set = 赋值 覆盖操作 尽可能的少使用 主要使用Change
         * 没有吧 Change 和 Set 使用一个API 用扩展参数就是为了方便查引用
         */

        public static void Set(this NumericDataComponent self, int numericType, bool value)
        {
            self.NumericData.Set(numericType, value);
        }

        public static void Set(this NumericDataComponent self, int numericType, float value)
        {
            self.NumericData.Set(numericType, value);
        }

        public static void Set(this NumericDataComponent self, int numericType, long value)
        {
            self.NumericData.Set(numericType, value);
        }

        public static void Set(this NumericDataComponent self, int numericType, int value)
        {
            self.NumericData.Set(numericType, value);
        }

        public static void Set(this NumericDataComponent self, ENumericType numericType, bool value)
        {
            self.NumericData.Set(numericType, value);
        }

        public static void Set(this NumericDataComponent self, ENumericType numericType, float value)
        {
            self.NumericData.Set(numericType, value);
        }

        public static void Set(this NumericDataComponent self, ENumericType numericType, int value)
        {
            self.NumericData.Set(numericType, value);
        }

        public static void Set(this NumericDataComponent self, ENumericType numericType, long value)
        {
            self.NumericData.Set(numericType, value);
        }

        //跳过检查 非必要禁止使用
        public static void SetUnCheck(this NumericDataComponent self, ENumericType numericType, long value)
        {
            self.NumericData.SetUnCheck(numericType, value);
        }

        public static void SetUnCheck(this NumericDataComponent self, int numericType, long value)
        {
            self.NumericData.SetUnCheck(numericType, value);
        }

        #region SetNoEvent

        public static void SetNoEvent(this NumericDataComponent self, int numericType, bool value)
        {
            self.NumericData.SetNoEvent(numericType, value);
        }

        public static void SetNoEvent(this NumericDataComponent self, int numericType, float value)
        {
            self.NumericData.SetNoEvent(numericType, value);
        }

        public static void SetNoEvent(this NumericDataComponent self, int numericType, long value)
        {
            self.NumericData.SetNoEvent(numericType, value);
        }

        public static void SetNoEvent(this NumericDataComponent self, ENumericType numericType, bool value)
        {
            self.NumericData.SetNoEvent(numericType, value);
        }

        public static void SetNoEvent(this NumericDataComponent self, ENumericType numericType, float value)
        {
            self.NumericData.SetNoEvent(numericType, value);
        }

        public static void SetNoEvent(this NumericDataComponent self, ENumericType numericType, long value)
        {
            self.NumericData.SetNoEvent(numericType, value);
        }

        //跳过检查 非必要禁止使用
        public static void SetNoEventUnCheck(this NumericDataComponent self, ENumericType numericType, long value)
        {
            self.NumericData.SetNoEventUnCheck(numericType, value);
        }

        #endregion

        #endregion
    }
}