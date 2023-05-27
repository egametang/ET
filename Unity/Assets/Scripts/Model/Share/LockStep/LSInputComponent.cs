namespace ET
{
    [ComponentOf(typeof(LSUnit))]
    public class LSInputComponent: LSEntity, ILSUpdate, IAwake, ISerializeToEntity
    {
        public LSInput LSInput { get; set; }
    }
}