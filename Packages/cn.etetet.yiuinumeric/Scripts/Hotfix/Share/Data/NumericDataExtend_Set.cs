namespace ET
{
    /// <summary>
    /// 覆盖
    /// </summary>
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataExtend
    {
        #region Set

        /*
         * 少量的 Set = 赋值 覆盖操作 尽可能的少使用 主要使用Change
         * 没有吧 Change 和 Set 使用一个API 用扩展参数就是为了方便查引用
         */

        public static void Set(this NumericData self, int numericType, bool value)
        {
            self.ChangeByKey(numericType, value ? 1 : 0, true, false);
        }

        public static void Set(this NumericData self, int numericType, float value)
        {
            self.ChangeByKey(numericType, (long)(value * NumericConst.FloatRate), false);
        }

        public static void Set(this NumericData self, int numericType, long value)
        {
            self.ChangeByKey(numericType, value, true, false);
        }

        public static void Set(this NumericData self, int numericType, int value)
        {
            self.ChangeByKey(numericType, value, true, false);
        }

        public static void Set(this NumericData self, ENumericType numericType, bool value)
        {
            self.ChangeByKey((int)numericType, value ? 1 : 0, true, false);
        }

        public static void Set(this NumericData self, ENumericType numericType, float value)
        {
            self.ChangeByKey((int)numericType, (long)(value * NumericConst.FloatRate), true, false);
        }

        public static void Set(this NumericData self, ENumericType numericType, int value)
        {
            self.ChangeByKey((int)numericType, value, true, false);
        }

        public static void Set(this NumericData self, ENumericType numericType, long value)
        {
            self.ChangeByKey((int)numericType, value, true, false);
        }

        //跳过检查 非必要禁止使用
        public static void SetUnCheck(this NumericData self, ENumericType numericType, long value)
        {
            self.ChangeByKey((int)numericType, value, true, false, false);
        }

        public static void SetUnCheck(this NumericData self, int numericType, long value)
        {
            self.ChangeByKey(numericType, value, true, false, false);
        }

        #region SetNoEvent

        public static void SetNoEvent(this NumericData self, int numericType, bool value)
        {
            self.ChangeByKey(numericType, value ? 1 : 0, false, false);
        }

        public static void SetNoEvent(this NumericData self, int numericType, float value)
        {
            self.ChangeByKey(numericType, (long)(value * NumericConst.FloatRate), false, false);
        }

        public static void SetNoEvent(this NumericData self, int numericType, long value)
        {
            self.ChangeByKey(numericType, value, false, false);
        }

        public static void SetNoEvent(this NumericData self, ENumericType numericType, bool value)
        {
            self.ChangeByKey((int)numericType, value ? 1 : 0, false, false);
        }

        public static void SetNoEvent(this NumericData self, ENumericType numericType, float value)
        {
            self.ChangeByKey((int)numericType, (long)(value * NumericConst.FloatRate), false, false);
        }

        public static void SetNoEvent(this NumericData self, ENumericType numericType, long value)
        {
            self.ChangeByKey((int)numericType, value, false, false);
        }

        //跳过检查 非必要禁止使用
        public static void SetNoEventUnCheck(this NumericData self, ENumericType numericType, long value)
        {
            self.ChangeByKey((int)numericType, value, false, false, false);
        }

        #endregion

        #endregion
    }
}