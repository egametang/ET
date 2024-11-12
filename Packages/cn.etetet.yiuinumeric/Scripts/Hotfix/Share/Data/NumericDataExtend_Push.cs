namespace ET
{
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataExtend
    {
        /// <summary>
        /// 数值改变推送
        /// 会根据改变类型做不同分发
        /// 这个类型不会太多扩展 如果你需要一直扩展 那就是设计有问题
        /// </summary>
        public static void PushEvent(this NumericData self, int numericType, long newValue, long oldValue)
        {
            if (self.OwnerEntity == null || self.OwnerEntity.IsDisposed) return;
            var numericChange = NumericChangeHelper.Create(self.OwnerEntity.Parent, numericType, oldValue, newValue);
            EventSystem.Instance.Publish(self.OwnerEntity.Scene(), numericChange);
        }

        /// <summary>
        /// 全数值推送
        /// </summary>
        public static void PushEventAll(this NumericData self)
        {
            if (self.OwnerEntity == null || self.OwnerEntity.IsDisposed) return;
            foreach (var data in self.NumericDic)
            {
                self.PushEvent(data.Key, data.Value, 0);
            }
        }
    }
}
