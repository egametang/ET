
namespace ET.Server
{
    [ComponentOf(typeof(Player))] //该组件是挂着在 Session 身上的
    public class LvExpComponent: Entity, IAwake
    {
        public int Lv;
        public int Exp;
    }
}