using TrueSync;

namespace ET.Client
{
    [ComponentOf(typeof(Room))]
    public class BattleSceneClientUpdater: Entity, IAwake, IUpdate
    {
        public LSInputInfo InputInfo = new LSInputInfo();
    }
}