namespace ET
{
    public class LSInputComponent: LSEntity, ILSUpdate, IAwake
    {
        public LSInput LSInput { get; set; } = new LSInput();

    }
}