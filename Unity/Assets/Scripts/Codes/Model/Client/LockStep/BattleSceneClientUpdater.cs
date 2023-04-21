using TrueSync;

namespace ET.Client
{
    [ComponentOf(typeof(BattleScene))]
    public class BattleSceneClientUpdater: Entity, IAwake, IUpdate
    {
        public LSInputInfo InputInfo = new LSInputInfo();
    }
}