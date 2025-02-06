namespace ET
{
    [EntitySystemOf(typeof(CoroutineLock))]
    public static partial class CoroutineLockSystem
    {
        [EntitySystem]
        private static void Awake(this CoroutineLock self, long type, long k, int count)
        {
            self.type = type;
            self.key = k;
            self.level = count;
        }
        
        [EntitySystem]
        private static void Destroy(this CoroutineLock self)
        {
            self.Scene<CoroutineLockComponent>().RunNextCoroutine(self.type, self.key, self.level + 1);
            self.type = 0;
            self.key = 0;
            self.level = 0;
        }
    }
}