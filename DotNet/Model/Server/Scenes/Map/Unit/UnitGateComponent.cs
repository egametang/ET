namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class UnitGateComponent : Entity, IAwake<long>, ITransfer
    {
        public long GateSessionActorId { get; set; }
    }
}