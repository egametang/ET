namespace ET.Client
{
    [EntitySystemOf(typeof(PCCaseComponent))]
    public static partial class PCCaseComponentSystem
    {
        [EntitySystem]
        private static void Awake(this PCCaseComponent self)
        {
            Log.Debug("PCCaseComponent Awake");
        }
    }
}