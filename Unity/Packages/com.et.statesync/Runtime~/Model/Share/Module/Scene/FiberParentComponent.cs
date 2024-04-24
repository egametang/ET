namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class FiberParentComponent: Entity, IAwake
    {
        public int ParentFiberId { get; set; }
    }
}