namespace ET
{
    [ComponentOf(typeof(Scene))]
    [ChildType(typeof(Cell))]
    public class AOIManagerComponent: Entity, IAwake
    {
        public static int CellSize = 10 * 1000;
    }
}