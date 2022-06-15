namespace ET
{
    [ChildType(typeof(Cell))]
    [ComponentOf(typeof(Scene))]
    public class AOIManagerComponent: Entity, IAwake
    {
        public const int CellSize = 10 * 1000;
    }
}