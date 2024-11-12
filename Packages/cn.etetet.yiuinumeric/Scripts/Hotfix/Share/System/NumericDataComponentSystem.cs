namespace ET
{
    [EntitySystemOf(typeof(NumericDataComponent))]
    [FriendOf(typeof(NumericDataComponent))]
    public static partial class NumericDataComponentSystem
    {
        [EntitySystem]
        private static void Awake(this NumericDataComponent self)
        {
            self.NumericData.UpdateOwnerEntity(self);
        }

        [EntitySystem]
        private static void Destroy(this NumericDataComponent self)
        {
            self.NumericData.Dispose();
        }

        [EntitySystem]
        private static void Deserialize(this NumericDataComponent self)
        {
            self.Awake();
        }
    }
}
